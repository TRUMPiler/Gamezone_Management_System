using Microsoft.AspNetCore.Mvc;
using GameZoneManagementSystem.Models;
using System.Data.SqlClient;
using GameZoneManagementSystem.Controllers;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using SlotBookingApp.Models;
using System.Security.Claims;
namespace GameZoneManagementSystem.Controllersd
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
        public IActionResult BookSlotConfirmation(int GameID, string Day, string TimeRange, DateTime? bookingDate)
        {
            if (!isLoggedin())
            {
                string script = "<script>alert('You are not logged in.')</script>" +
                                "<script>window.location='/Home/Login'</script>";
                return Content(script, "text/html");
            }

            if (!bookingDate.HasValue)
            {
                TempData["ErrorMessage"] = "Please select a booking date.";
                return RedirectToAction("Games");
            }

            HttpContext.Session.SetInt32("BookingGameID", GameID);
            HttpContext.Session.SetString("BookingDay", Day);
            HttpContext.Session.SetString("BookingTimeRange", TimeRange);
            HttpContext.Session.SetString("BookingDate", bookingDate.Value.ToString("yyyy-MM-dd")); // Store date as string

            // Fetch game details for confirmation
            using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
            {
                con.Open();
                string gameQuery = "SELECT ID, Game FROM Tbl_Game WHERE ID = @GameID";
                using (SqlCommand cmd = new SqlCommand(gameQuery, con))
                {
                    cmd.Parameters.AddWithValue("@GameID", GameID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.GameName = reader["Game"].ToString();
                            ViewBag.Day = Day;
                            ViewBag.TimeRange = TimeRange;
                            ViewBag.BookingDate = bookingDate.Value.ToShortDateString();
                            return View();
                        }
                    }
                }
                return RedirectToAction("Games"); // If game not found
            }
        }

        [HttpPost]
        public IActionResult BookSlot()
        {
            if (!isLoggedin())
            {
                string script = "<script>alert('You are not logged in.')</script>" +
                                "<script>window.location='/Home/Login'</script>";
                return Content(script, "text/html");
            }

            int userId = Int32.Parse(HttpContext.Session.GetString("Userid"));
            int gameId = HttpContext.Session.GetInt32("BookingGameID").Value;
            string day = HttpContext.Session.GetString("BookingDay");
            string timeRange = HttpContext.Session.GetString("BookingTimeRange");
            DateTime bookingDate = DateTime.Parse(HttpContext.Session.GetString("BookingDate"));

            int? gameSlotIdToBook = null;

            using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
            {
                con.Open();

                // 1. Find the corresponding GameSlotID
                string findGameSlotIdQuery = @"
                SELECT gs.ID
                FROM Tbl_Game g
                INNER JOIN Tbl_Game_Slot gs ON g.ID = gs.GameID
                INNER JOIN Tbl_Slot s ON gs.SlotID = s.ID
                INNER JOIN Tbl_Day d ON s.DayID = d.ID
                INNER JOIN Tbl_Time t ON s.TimeID = t.ID
                WHERE g.ID = @GameID
                  AND d.Day = @Day
                  AND FORMAT(t.Start_Time, 'hh\:mm') + '-' + FORMAT(t.End_Time, 'hh\:mm') = @TimeRange;";

                using (SqlCommand cmdFindGameSlotId = new SqlCommand(findGameSlotIdQuery, con))
                {
                    cmdFindGameSlotId.Parameters.AddWithValue("@GameID", gameId);
                    cmdFindGameSlotId.Parameters.AddWithValue("@Day", day);
                    cmdFindGameSlotId.Parameters.AddWithValue("@TimeRange", timeRange);

                    object result = cmdFindGameSlotId.ExecuteScalar();
                    if (result != null)
                    {
                        gameSlotIdToBook = Convert.ToInt32(result);
                    }
                    else
                    {
                        string script = "<script>alert('Selected slot is not valid')</script>" +
                                "<script>window.location='/Customer/Games'</script>";
                        return Content(script, "text/html");
                       
                    }
                }

                if (gameSlotIdToBook.HasValue)
                {
                    // 2. Check if the slot is already booked by the current user on the selected date
                    string checkBookingQuery = @"
                    SELECT COUNT(*)
                    FROM Tbl_Game_Slot_Booking
                    WHERE Game_SlotID = @GameSlotID
                      AND UserID = @UserID
                      AND Date = @BookingDate;";
                    using (SqlCommand cmdCheckBooking = new SqlCommand(checkBookingQuery, con))
                    {
                        cmdCheckBooking.Parameters.AddWithValue("@GameSlotID", gameSlotIdToBook.Value);
                        cmdCheckBooking.Parameters.AddWithValue("@UserID", userId);
                        cmdCheckBooking.Parameters.AddWithValue("@BookingDate", bookingDate);
                        int bookingCount = (int)cmdCheckBooking.ExecuteScalar();
                        if (bookingCount > 0)
                        {
                            string script = "<script>alert('You have already booked this slot for the selected date.');window.location='/Customer/Games'</script>";
                            return Content(script, "text/html");
                            
                        }
                    }

                    // 3. Insert the booking record with the date
                    string bookSlotQuery = "INSERT INTO Tbl_Game_Slot_Booking (Game_SlotID, UserID, Date) VALUES (@GameSlotID, @UserID, @BookingDate)";
                    using (SqlCommand cmdBookSlot = new SqlCommand(bookSlotQuery, con))
                    {
                        cmdBookSlot.Parameters.AddWithValue("@GameSlotID", gameSlotIdToBook.Value);
                        cmdBookSlot.Parameters.AddWithValue("@UserID", userId);
                        cmdBookSlot.Parameters.AddWithValue("@BookingDate", bookingDate);
                        cmdBookSlot.ExecuteNonQuery();
                        string message= "Slot booked successfully for " + bookingDate.ToShortDateString() + "!";
                        string script = "<script>alert('"+message+"');window.location='/Customer/Games'</script>";
                        return Content(script, "text/html");
                        //TempData["SuccessMessage"] = 
                        //return RedirectToAction("Games");
                    }
                }
            }

            return RedirectToAction("Games"); // General error
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
        Dictionary<int, List<string>> gameSlots = new Dictionary<int, List<string>>();

        // Query 1: Get basic game information
        string gameQuery = @"
            SELECT
                g.ID AS GameID,
                g.Game,
                g.Game_Description,
                g.SubCatID,
                g.Image,
                p.Credits,
                sc.Sub_Category_Name,
                sc.CategoryID,
                c.CategoryName
            FROM Tbl_Game g
            LEFT JOIN Tbl_Price p ON g.ID = p.GameID
            LEFT JOIN Tbl_Games_Sub_Category sc ON g.SubCatID = sc.ID
            LEFT JOIN Tbl_Games_Category c ON sc.CategoryID = c.ID
            WHERE g.Status = 1;
        ";

        // Query 2: Get slot information for each game
        string slotQuery = @"
            SELECT
                g.ID AS GameID,
                dy.Day,
                FORMAT(ti.Start_Time, 'hh\:mm') AS StartTime,
                FORMAT(ti.End_Time, 'hh\:mm') AS EndTime
            FROM Tbl_Game g
            INNER JOIN Tbl_Game_Slot gs ON g.ID = gs.GameID
            INNER JOIN Tbl_Slot s ON gs.SlotID = s.ID
            INNER JOIN Tbl_Day dy ON s.DayID = dy.ID
            INNER JOIN Tbl_Time ti ON s.TimeID = ti.ID
            WHERE g.Status = 1;
        ";

        try
        {
            con.Open();

            // Execute the first query to get game details
            using (SqlCommand gameCommand = new SqlCommand(gameQuery, con))
            using (SqlDataReader gameReader = gameCommand.ExecuteReader())
            {
                while (gameReader.Read())
                {
                    Games game = new Games
                    {
                        Id = (int)gameReader["GameID"],
                        Name = (string)gameReader["Game"],
                        Game_Description = (string)gameReader["Game_Description"],
                        image = (string)gameReader["Image"],
                        price = gameReader["Credits"] != DBNull.Value ? (decimal)gameReader["Credits"] : 0,
                        SubCategory = new Games_Sub_Category
                        {
                            ID = (int)gameReader["SubCatID"],
                            Sub_Category_Name = (string)gameReader["Sub_Category_Name"],
                            CategoryID = (int)gameReader["CategoryID"],
                            CategoryName = (string)gameReader["CategoryName"]
                        },
                        Slots = new List<string>() // Initialize the Slots list here
                    };
                    games.Add(game);
                    gameSlots[game.Id] = new List<string>(); // Initialize slot list for each game ID
                }
            }

            // Execute the second query to get slot details
            using (SqlCommand slotCommand = new SqlCommand(slotQuery, con))
            using (SqlDataReader slotReader = slotCommand.ExecuteReader())
            {
                while (slotReader.Read())
                {
                    int gameId = (int)slotReader["GameID"];
                    string slot = $"{slotReader["Day"]} {slotReader["StartTime"]}-{slotReader["EndTime"]}";
                    if (gameSlots.ContainsKey(gameId))
                    {
                        gameSlots[gameId].Add(slot);
                    }
                }
            }

            // Assign the slots to the respective games
            foreach (var game in games)
            {
                if (gameSlots.ContainsKey(game.Id))
                {
                    game.Slots.AddRange(gameSlots[game.Id]);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle the exception appropriately
            Console.WriteLine($"Error: {ex.Message}");
            // Optionally return an error view
            return View("Error");
        }
        finally
        {
            if (con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
            }
        }

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
