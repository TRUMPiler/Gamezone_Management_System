using Microsoft.AspNetCore.Mvc;
using GameZoneManagementSystem.Models;
using System.Data.SqlClient;
using GameZoneManagementSystem.Controllers;
namespace GameZoneManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        SqlConnection con=new SqlConnection("Data Source=NAISHALTUF;Initial Catalog=GZMS;Integrated Security=True;");
        String otp = "";
        public Boolean isLoggedin()
        {
            if (HttpContext.Session.GetString("Login") != null)
            {
                return true;
            }
            
            return false;
            
        }

        public IActionResult Otp()
        {

            return View();
        }
        public IActionResult Index()
        {   
            return View();
        }
        [HttpPost]
        public IActionResult Otp(OtpGen otps)
        {
            this.otp = HttpContext.Session.GetString("otp").ToString();
            if (otps.Otp.Equals(this.otp))
            {
                if(updateStatus(true, HttpContext.Session.GetString("Email").ToString())>0)
                {
                    HttpContext.Session.Clear();
                    string script = "<script>alert('otp verified');window.location='/Home/Login'</script>";
                    return Content(script, "text/html");
                }
               else
                {
                    string script = "<script>alert('otp verified but something went wrong please contact the admins for futher assitance');window.location='/Home/Login'</script>";
                    return Content(script, "text/html");
                }
            }
            else
            {
                string script = "<script>alert('otp not matched');</script>";
                return Content(script, "text/html");
            }
            return View();
        }
       
        public IActionResult Register()
        {
            if(isLoggedin())
            {
                string script = "<script>alert('You are already logged in. \n no need to register')</script>" +
                    "<script>window.location='/Customer/Index'</script>";
                return Content(script, "text/html");
            }
            
                return View();
            
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

        private int updateStatus(bool status,string Email)
        {

            SqlCommand cmd = new SqlCommand("update Tbl_User set status=@Status where Email=@Email", con);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Email", Email);
            con.Open();
            return cmd.ExecuteNonQuery();
            
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            HashPasswordController hp = new HashPasswordController();

            using (SqlConnection connection = new SqlConnection("Data Source=NAISHALTUF;Initial Catalog=GZMS;Integrated Security=True;"))
            {
                connection.Open();

                string checkUserQuery = @"SELECT COUNT(*) FROM Tbl_User WHERE Email = @Email OR Phone = @Phone";
                using (SqlCommand checkCommand = new SqlCommand(checkUserQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Email", user.Email);
                    checkCommand.Parameters.AddWithValue("@Phone", user.Phone);

                    int userCount = (int)checkCommand.ExecuteScalar();
                    if (userCount > 0)
                    {
                        string script = "<script>alert('You are already registered. Please Login');window.location='/Home/Login';</script>";
                        return Content(script, "text/html");
                    }
                }

                string query = @"INSERT INTO Tbl_User 
                         (FirstName, SecondName, LastName, Dob, Password, Email, Phone, Role, Gender, Status) 
                         VALUES (@FirstName, @SecondName, @LastName, @Dob, @Password, @Email, @Phone, @Role, @Gender, @Status)";

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
                    command.Parameters.AddWithValue("@Status", 0);

                    if (command.ExecuteNonQuery() > 0)
                    {
                        OtpController otpcon = new OtpController();
                        HttpContext.Session.SetString("Email", user.Email);
                        otp = otpcon.SendMail(user.Email, "Verify Email");
                        HttpContext.Session.SetString("otp", otp);

                        string script = "<script>alert('User registered successfully!');window.location='/Customer/Otp';</script>";
                        return Content(script, "text/html");
                    }
                    else
                    {
                        string script = "<script>alert('User registration Failed!');window.location='/Customer/Registration';</script>";
                        return Content(script, "text/html");
                    }
                }
            }
        }

    }
}
