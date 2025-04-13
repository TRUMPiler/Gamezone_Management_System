using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using GameZoneManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Configuration;

namespace GameZoneManagementSystem.Controllers
{
    public class HomeController : Controller
    {

        private readonly IConfiguration configuration;
        

        public HomeController(IConfiguration config)
        {
            
            configuration = config;
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
    //    public IActionResult Games()
    //        {
    //            List<Games> games = new List<Games>();

    //        string query = @"
    //SELECT G.ID, G.Game AS Name, G.Game_Description, G.SubCatID, G.Image, 
    //       ISNULL(P.Credits, 0) AS Price
    //FROM Tbl_Game G
    //LEFT JOIN Tbl_Price P ON G.ID = P.GameID";

    //        using (SqlCommand cmd = new SqlCommand(query, con))
    //        {
    //            con.Open();
    //            using (SqlDataReader reader = cmd.ExecuteReader())
    //            {
    //                while (reader.Read())
    //                {
    //                    Games game = new Games();
    //                    game.Id = (int)reader["ID"];
    //                    game.Name = reader["Name"].ToString();
    //                    game.Game_Description = reader["Game_Description"].ToString();
    //                    game.SubCatID = (int)reader["SubCatID"];
    //                    game.image = reader["Image"].ToString();
    //                    game.price = Convert.ToDecimal(reader["Price"]);

    //                    games.Add(game);
    //                }
    //            }
    //            con.Close();
    //        }

    //        return View(games);

    //    }

        [HttpPost]
        public IActionResult ForgetPassword(User user)
        {
            HttpContext.Session.SetString("UserEmail", user.Email);
            OtpController otpcon = new OtpController();
            string otp = otpcon.SendMail(user.Email, "Forgot Password");
            HttpContext.Session.SetString("ForgetPasswordotp", otp);
            return View();
        }
        public IActionResult FixPassword()
        {
            if(HttpContext.Session.GetString("UserEmail")==null)
            {
                string script = "<script>alert('No Password Resetting Token Found');window.location='/Home/Login'</script>";
                return Content(script, "text/html");
            }
            return View();
        }
        //[HttpPost]
        //public IActionResult ForgetOtp()
        //{

        //}
        [HttpPost]
        public IActionResult FixPassword(string newPassword, string confirmPassword)
        {
            // Retrieve the email from session
            string userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail)|| string.IsNullOrEmpty(newPassword))

            {
                string script = "<script>alert('Session expired. Please try again.');window.location='/Home/Forgets'</script>";
                return Content(script, "text/html");
            }

            if (!newPassword.Equals(confirmPassword))
            {
                string script = "<script>alert('Passwords do not match.');window.location='/Home/FixPassword'</script>";
                return Content(script, "text/html");
            }
            else { 

            // Update the user's password in the database
            try
            {
               

                using (SqlConnection connection = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
                {
                    connection.Open();

                    string updateQuery = "UPDATE Tbl_Users SET Password = @Password WHERE Email = @Email";
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        HashPasswordController hash = new HashPasswordController();

                        // Hash the password (optional, for security)
                        string hashedPassword = hash.HashPassword(newPassword);

                        command.Parameters.AddWithValue("@Password", hashedPassword);
                        command.Parameters.AddWithValue("@Email", userEmail);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            string successScript = "<script>alert('Password updated successfully. Please log in.');window.location='/Home/Login'</script>";
                            return Content(successScript, "text/html");
                        }
                        else
                        {
                            string script = "<script>alert('User not found or update failed.');window.location='/Home/Forgets'</script>";
                            return Content(script, "text/html");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorScript = $"<script>alert('An error occurred: {ex.Message}');window.location='/Home/FixPassword'</script>";
                return Content(errorScript, "text/html");
            }
            }
        }
        public IActionResult Forget()
        {
            return View();
        }
        [HttpPost]
        public IActionResult VerifyOtp(string Otp)
        {
            if (Otp.Equals(HttpContext.Session.GetString("ForgetPasswordotp")))
            {
                return Json(new { success = true, message = "Otp Verified", redirectUrl = "/Home/FixPassword" });
            }
            else
            {
                return Json(new { success = false, message = "Otp is Incorrect" });
            }
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
            using (SqlConnection connection = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
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
                            user.id = (int)reader["id"];
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
                if (user.Role == 1)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (user.Role == 2)
                {
                    return RedirectToAction("Index", "Customer");
                }
                else if (user.Role == 3)
                {
                    return RedirectToAction("Index", "Staff");
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
