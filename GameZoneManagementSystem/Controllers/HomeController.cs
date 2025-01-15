using System.Data.SqlClient;
using System.Diagnostics;
using GameZoneManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
namespace GameZoneManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        string SessionName = "Login";
        string SessionEmail = "Email";
        string SessionRole = "Role";
        string script="";
        string con = "Data Source=NAISHALTUF;Initial Catalog=GZMS;Integrated Security=True;";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController>? logger=null)
        {
            _logger = logger;
        }
        
        public bool isLoggedIn()
        {

            //if (HttpContext.Session == null)
            //{
            //    Console.WriteLine("Session object is null");
            //    return true;
            //}

            //if (!HttpContext.Session.Keys.Contains(SessionName))
            //{
            //    Console.WriteLine($"Session key '{SessionName}' does not exist");
            //    return true;
            //}

            //int? sessionValue = HttpContext.Session.GetInt32(SessionName);
            //if (!sessionValue.HasValue)
            //{
            //    Console.WriteLine("Session value is null");
            //    return true;
            //}
            return false;

        }
        public IActionResult Index()
        {
            if(HttpContext.Session.GetInt32(SessionName)!=null)
            {
                return RedirectToAction("Index","Customer");
            }
            else
            {
                return View();
            }
            
        }
        public String ReturnView(string role)
        {
            if(role =="c")
            {
                return "/Customer/Index";
            }
            else
            {
                return "/Home/Index";
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Login()
        {
            if (isLoggedIn())
            {
                script = "<script>alert('You are already logged in');</script>";
                script += "<script>" +  ReturnView(HttpContext.Session.GetString("Role"))+"</script>";
                return Content(script,"text/html");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public IActionResult login(User user)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                string script = "<script>alert('Email and Password are required!');window.location='/Home/Login';</script>";
                return Content(script, "text/html");
            }

            string storedHashedPassword = null;
            

            
            
            using (SqlConnection connection = new SqlConnection(con))
            {
                connection.Open();
                string query = "SELECT Password, Role,FirstName FROM Tbl_User WHERE Email = @Email";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", user.Email);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            storedHashedPassword = reader["Password"].ToString();
                            user.Role = reader["Role"].ToString();
                        }
                    }
                }
            }

            
            HashPasswordController hp = new HashPasswordController();
            if (!string.IsNullOrEmpty(storedHashedPassword) && hp.VerifyPassword(user.Password, storedHashedPassword))
            {
                HttpContext.Session.SetInt32(SessionName, 1);
                HttpContext.Session.SetString(SessionEmail, user.Email.ToString());
                HttpContext.Session.SetString(SessionRole, user.Role.ToString());

                //if (user.Role=="c")
                //{
                    
                //    user.Role = "customer";
                //    script = $"<script>alert('Login successful! Welcome {user.Role}');window.location='/Customer/Index';</script>";
                //    return Content(script, "text/html");
                //}
                script = $"<script>alert('Login successful! Welcome {user.Role}');window.location='/Home/Index';</script>";
                return Content(script, "text/html");
            }
            else
            {
                script = "<script>alert('Invalid credentials. Please try again.');window.location='/Home/Login';</script>";
                return Content(script, "text/html");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
