using Microsoft.AspNetCore.Mvc;
using GameZoneManagementSystem.Models;
using System.Data.SqlClient;
using GameZoneManagementSystem.Controllers;
namespace GameZoneManagementSystem.Controllers
{
    
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {   
            return View();
        }
        public IActionResult Register()
        {
            //HomeController home = new HomeController();
            //if(home.isLoggedIn())
            //{
            //    return RedirectToAction("Index");
            //}
            //else
            //{
                return View();
            //}
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            string script = "<script>window.location='/Home/Index'</script>";
            return Content(script, "text/html");
        }
        public IActionResult Login()
        {
            return RedirectToAction("Logout","Home");
        }
        [HttpPost]
        public IActionResult Register(User user)
        {
            // Ensure all required fields are provided
            HashPasswordController hp=new HashPasswordController();
            using (SqlConnection connection = new SqlConnection("Data Source=NAISHALTUF;Initial Catalog=GZMS;Integrated Security=True;"))
            {
                connection.Open();
                string query = @"INSERT INTO tbl_User 
                                         (FirstName, SecondName, LastName, Dob, Password, Email, Phone, Role, Gender) 
                                         VALUES (@FirstName, @SecondName, @LastName, @Dob, @Password, @Email, @Phone, @Role, @Gender)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", user.FirstName);
                    command.Parameters.AddWithValue("@SecondName", user.SecondName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LastName", user.LastName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Dob", user.Dob ?? (object)DBNull.Value);
                    
                    var hashedPassword = hp.HashPassword(user.Password);
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", user.Phone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Role", user.Role ?? "c");
                    command.Parameters.AddWithValue("@Gender", user.Gender);


                    if (command.ExecuteNonQuery() > 0)
                    {
                        string script = "<script>alert('User registered successfully!');window.location='/Customer/Index';</script>";
                        return Content(script, "text/html");
                    }
                    else
                    {
                        string script = "<script>alert('User registeration Failed!');window.location='/Customer/Registeration';</script>";
                        return Content(script, "text/html");
                    }

                }
            }

        }
           
    }
}
