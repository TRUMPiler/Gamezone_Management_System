using Microsoft.AspNetCore.Mvc;
using GameZoneManagementSystem.Models;
using System.Data.SqlClient;
using GameZoneManagementSystem.Controllers;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using SlotBookingApp.Models;
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

        //public IActionResult BookSlot(int gameId)
        //{
        //    var model = new SlotBookingViewModel();
        //    model.GameID = gameId;
        //    model.Days = new List<SelectListItem>();
        //    model.Times = new List<SelectListItem>();

        //    // Load days
        //    con.Open();
        //    SqlCommand dayCmd = new SqlCommand("SELECT * FROM Tbl_Day", con);
        //    SqlDataReader dayReader = dayCmd.ExecuteReader();
        //    while (dayReader.Read())
        //    {
        //        model.Days.Add(new SelectListItem
        //        {
        //            Value = dayReader["ID"].ToString(),
        //            Text = dayReader["Day"].ToString()
        //        });
        //    }
        //    dayReader.Close();

        //    // Load times
        //    SqlCommand timeCmd = new SqlCommand("SELECT * FROM Tbl_Time", con);
        //    SqlDataReader timeReader = timeCmd.ExecuteReader();
        //    while (timeReader.Read())
        //    {
        //        model.Times.Add(new SelectListItem
        //        {
        //            Value = timeReader["ID"].ToString(),
        //            Text = timeReader["Time"].ToString()
        //        });
        //    }
        //    con.Close();

        //    return View(model);
        //}

        //[HttpPost]
        //public IActionResult BookSlot(SlotBookingViewModel model)
        //{
        //    int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));

        //    // Check credits
        //    int userCredits = 0;
        //    con.Open();
        //    SqlCommand creditCmd = new SqlCommand("SELECT Credit FROM Tbl_Users WHERE ID=@id", con);
        //    creditCmd.Parameters.AddWithValue("@id", userId);
        //    userCredits = Convert.ToInt32(creditCmd.ExecuteScalar());

        //    // Assume each game costs 200 credits
        //    if (userCredits < 200)
        //    {
        //        ViewBag.Message = "Not enough credits to book this slot.";
        //        con.Close();
        //        return View(model);
        //    }

        //    // Create/Find slot in Tbl_Slot
        //    int slotId = 0;
        //    SqlCommand checkSlot = new SqlCommand("SELECT ID FROM Tbl_Slot WHERE DayID=@day AND TimeID=@time", con);
        //    checkSlot.Parameters.AddWithValue("@day", model.DayID);
        //    checkSlot.Parameters.AddWithValue("@time", model.TimeID);
        //    var slotObj = checkSlot.ExecuteScalar();
        //    if (slotObj != null)
        //    {
        //        slotId = Convert.ToInt32(slotObj);
        //    }
        //    else
        //    {
        //        SqlCommand insertSlot = new SqlCommand("INSERT INTO Tbl_Slot(DayID, TimeID) OUTPUT INSERTED.ID VALUES(@day, @time)", con);
        //        insertSlot.Parameters.AddWithValue("@day", model.DayID);
        //        insertSlot.Parameters.AddWithValue("@time", model.TimeID);
        //        slotId = (int)insertSlot.ExecuteScalar();
        //    }

        //    // Insert into Tbl_Game_Slot
        //    SqlCommand insertGameSlot = new SqlCommand("INSERT INTO Tbl_Game_Slot(GameID, SlotID) VALUES(@game, @slot)", con);
        //    insertGameSlot.Parameters.AddWithValue("@game", model.GameID);
        //    insertGameSlot.Parameters.AddWithValue("@slot", slotId);
        //    insertGameSlot.ExecuteNonQuery();

        //    // Insert booking (assuming Tbl_Booking exists)
        //    SqlCommand insertBooking = new SqlCommand("INSERT INTO Tbl_Booking(UserID, SlotID) VALUES(@user, @slot)", con);
        //    insertBooking.Parameters.AddWithValue("@user", userId);
        //    insertBooking.Parameters.AddWithValue("@slot", slotId);
        //    insertBooking.ExecuteNonQuery();

        //    // Deduct credits
        //    SqlCommand updateCredit = new SqlCommand("UPDATE Tbl_Users SET Credit = Credit - 200 WHERE ID = @id", con);
        //    updateCredit.Parameters.AddWithValue("@id", userId);
        //    updateCredit.ExecuteNonQuery();

        //    con.Close();
        //    TempData["Success"] = "Slot booked successfully!";
        //    return RedirectToAction("Games");
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
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            List<Games> games = new List<Games>();

            string query = @"
        SELECT 
    g.ID AS GameID, 
    g.Game, 
    g.Game_Description, 
    g.SubCatID, 
    g.Image, 
    p.Credits, 
    sc.Sub_Category_Name, 
    sc.CategoryID, -- Ensure this column is included
    c.CategoryName, 
    gs.ID AS GameSlotID, 
    gs.GameID, 
    s.DayID, 
    s.TimeID
FROM Tbl_Game g
LEFT JOIN Tbl_Price p ON g.ID = p.GameID
LEFT JOIN Tbl_Games_Sub_Category sc ON g.SubCatID = sc.ID
LEFT JOIN Tbl_Games_Category c ON sc.CategoryID = c.ID -- Ensure proper join
LEFT JOIN Tbl_Game_Slot gs ON g.ID = gs.GameID
LEFT JOIN Tbl_Slot s ON gs.SlotID = s.ID
where g.Status=1;
";

            using (SqlCommand command = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Create the game object
                        Games game = new Games
                        {
                            Id = (int)reader["GameID"],
                            Name = (string)reader["Game"],
                            Game_Description = (string)reader["Game_Description"],
                            image = (string)reader["Image"],
                            price = reader["Credits"] != DBNull.Value ? (decimal)reader["Credits"] : 0,

                            // Create and assign SubCategory object
                            SubCategory = new Games_Sub_Category
                            {
                                ID = (int)reader["SubCatID"],
                                Sub_Category_Name = (string)reader["Sub_Category_Name"],
                                CategoryID = (int)reader["CategoryID"],
                                CategoryName = (string)reader["CategoryName"]
                            },

                            // Create and assign Slot object if slot details exist
                            Slot = reader["GameSlotID"] != DBNull.Value ? new GameSlot
                            {
                                Id = (int)reader["GameSlotID"],
                                GameName = (string)reader["Game"], // Using game name from parent
                                SlotTime = new DateTime((int)reader["DayID"], 1, 1) // Placeholder
                                                                                    // Map `DayID` and `TimeID` to a proper `DateTime` if needed
                            } : null
                        };

                        games.Add(game);
                    }
                }
            }
            con.Close();
            return View(games);
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
