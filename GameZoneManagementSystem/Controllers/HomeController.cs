using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using GameZoneManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;

namespace GameZoneManagementSystem.Controllers
{
    public class HomeController : Controller
    {
       
        string con = "Data Source=NAISHALTUF;Initial Catalog=GZMS;Integrated Security=True;";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController>? logger = null)
        {
            _logger = logger;
        }

        public bool isLoggedIn()
        {
            return HttpContext.Session.GetString("Login") != null;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/Home/Index");
        }

        public IActionResult Login()
        {
            if (isLoggedIn())
            {
                return RedirectToAction("Index", "Customer");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {

            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                TempData["Error"] = "Email and Password are required!";
                return RedirectToAction("Login");
            }

            string storedHashedPassword = null;
            using (SqlConnection connection = new SqlConnection(con))
            {
                
                connection.Open();
                string query = "SELECT id,Password, RoleID FROM Tbl_Users WHERE Email = @Email AND Status=@Status";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@Status", true);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user.id =(int) reader["id"];
                            storedHashedPassword = reader["Password"].ToString();
                            user.Role = (int)reader["RoleID"];
                        }

                    }
                }
            }

            HashPasswordController hp = new HashPasswordController();
            if (!string.IsNullOrEmpty(storedHashedPassword) && hp.VerifyPassword(user.Password, storedHashedPassword))
            {
                HttpContext.Session.SetInt32("Login", 1);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("Role", user.Role.ToString());
                HttpContext.Session.SetString("Userid", user.id.ToString());
                if(user.Role==1)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if(user.Role == 2)
                {
                    return RedirectToAction("Index", "Customer");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                string script = "<script>alert('Credentials Not Found, please make sure you are using proper credentials');window.location='/Home/Login'</script>";
                return Content(script, "text/html");
            }
        }

       

        
        

       
        
    }
}
