using Microsoft.AspNetCore.Mvc;
using GameZoneManagementSystem.Models;
using System.Data.SqlClient;
using GameZoneManagementSystem.Controllers;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GameZoneManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        public bool CheckRole()
        {

            if(HttpContext.Session.GetString("Role")!=null)
            {
                if(HttpContext.Session.GetString("Role")=="2")
                {
                    return true;
                }
            }
            return false;
        }
        IConfiguration configuration;
        public CustomerController(IConfiguration config)
        {
            configuration = config;
        }
        
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
            if(CheckRole())
            {
                return View();
            }
            else
            {
                string script = "<script>alert('Role not matched');window.location='/Home/Index';</script>";
                return Content(script, "text/html");
            }
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
                string script = "<script>alert('otp not matched');window.location='/Customer/Otp'</script>";
                return Content(script, "text/html");
            }
            //return View();
        }



        
        public IActionResult Profile()
        {
            int userId = Int32.Parse(HttpContext.Session.GetString("Userid") ?? "0");

            if (userId == 0)
            {
                string script = "<script>alert('User id not found');window.location='/Home/Index'</script>";
                return Content(script, "text/html");
            }

            User user = null;
            string query = "SELECT * FROM Tbl_Users WHERE ID = @ID";

            using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ID", userId);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                id = (int)reader["ID"],
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                Gender = Convert.ToChar(reader["Gender"]),
                                Dob = reader["DOB"] != DBNull.Value ? (DateTime?)reader["DOB"] : null, // Handling nullable DateTime
                                Role = (int)reader["RoleID"],
                                Status = (bool)reader["Status"]
                            };
                        }
                    }
                }
            }

            return View(user);
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

        //-------------------------------------------------------------------

//        public IActionResult Feedback()
//        {
//            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
//            if (!HasBookedSlot(userId))
//            {
//                return RedirectToAction("Index", "Home");
//            }

//            FeedbackViewModel model = new FeedbackViewModel();
//            model.Games = new List<SelectListItem>();

//            string query = "SELECT DISTINCT g.ID, g.Game FROM Tbl_Game g " +
//                           "INNER JOIN Tbl_Game_Slot gs ON g.ID = gs.GameID " +
//                           "WHERE gs.ID IN (SELECT SlotID FROM Tbl_Booking WHERE UserID = @userID)";

//            using (SqlCommand cmd = new SqlCommand(query, con))
//            {
//                cmd.Parameters.AddWithValue("@userID", userId);
//                con.Open();
//                SqlDataReader reader = cmd.ExecuteReader();
//                while (reader.Read())
//                {
//                    model.Games.Add(new SelectListItem
//                    {
//                        Value = reader["ID"].ToString(),
//                        Text = reader["Game"].ToString()
//                    });
//                }
//                con.Close();
//            }

//            return View(model);
//        }

//        [HttpPost]
//        public IActionResult Feedback(FeedbackViewModel model)
//        {
//            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
//            string query = "INSERT INTO Tbl_Feedbacks(UserID, GameID, Message) VALUES(@UserID, @GameID, @Message)";

//            using (SqlCommand cmd = new SqlCommand(query, con))
//            {
//                cmd.Parameters.AddWithValue("@UserID", userId);
//                cmd.Parameters.AddWithValue("@GameID", model.GameID);
//                cmd.Parameters.AddWithValue("@Message", model.Message);

//                con.Open();
//                cmd.ExecuteNonQuery();
//                con.Close();
//            }

//            TempData["Success"] = "Feedback submitted successfully.";
//            return RedirectToAction("Feedback");
//        }

//        public bool HasBookedSlot(int userId)
//        {
//            string query = "SELECT COUNT(*) FROM Tbl_Booking WHERE UserID = @userID";
//            using (SqlCommand cmd = new SqlCommand(query, con))
//            {
//                cmd.Parameters.AddWithValue("@userID", userId);
//                con.Open();
//                int count = (int)cmd.ExecuteScalar();
//                con.Close();
//                return count > 0;
//            }
//        }
//}


        //-------------------------------------------------------------------------

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
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            SqlCommand cmd = new SqlCommand("update Tbl_Users set status=@Status where Email=@Email", con);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Email", Email);
            con.Open();
            return cmd.ExecuteNonQuery();
            
        }
        public IActionResult Games()
        {
            List<Games> games = new List<Games>();
            string query = "select * from Tbl_Games where Status=1";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {

            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            HashPasswordController hp = new HashPasswordController();

            using (SqlConnection connection = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
            {
                connection.Open();

                string checkUserQuery = @"SELECT COUNT(*) FROM Tbl_Users WHERE Email = @Email OR Phone = @Phone";
                using (SqlCommand checkCommand = new SqlCommand(checkUserQuery, connection))
                {
                    // Ensure user.Email and user.Phone are not null or empty
                    if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Phone))
                    {
                        throw new ArgumentException("Email or Phone cannot be null or empty.");
                    }

                    // Add parameters explicitly
                    checkCommand.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = user.Email;
                    checkCommand.Parameters.Add("@Phone", SqlDbType.NVarChar, 15).Value = user.Phone;

                    // ExecuteScalar must be called as a method
                    int userCount = (int)checkCommand.ExecuteScalar();
                    if (userCount > 0)
                    {
                        string script = "<script>alert('You are already registered. Please Login');window.location='/Home/Login';</script>";
                        return Content(script, "text/html");
                    }
                }
            }
            

            string query = @"INSERT INTO Tbl_Users 
                         (Name, Dob, Password, Email, Phone, RoleID, Gender, Status) 
                         VALUES (@Name, @Dob, @Password, @Email, @Phone, @Role, @Gender, @Status)";
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@Dob", user.Dob ?? (object)DBNull.Value);

                    var hashedPassword = hp.HashPassword(user.Password);
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", user.Phone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Role", 2);
                    command.Parameters.AddWithValue("@Gender", user.Gender);
                    command.Parameters.AddWithValue("@Status", 0);
                    con.Open();
                    if (command.ExecuteNonQuery() > 0)
                    {
                        OtpController otpcon = new OtpController();
                        HttpContext.Session.SetString("Email", user.Email);
                        otp = otpcon.SendMail(user.Email, "Verify Email");
                        HttpContext.Session.SetString("otp", otp);



                        string script = "<script>alert('User registered successfully!');window.location='/Customer/Otp';</script>";
                    con.Close();
                        return Content(script, "text/html");
                    }
                    else
                    {
                        con.Close();
                        string script = "<script>alert('User registration Failed!');window.location='/Customer/Registration';</script>";
                        return Content(script, "text/html");
                    }
                }
            }
        }

    
}
