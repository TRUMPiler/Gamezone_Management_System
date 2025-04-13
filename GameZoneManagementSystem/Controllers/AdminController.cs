using GameZoneManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using Org.BouncyCastle.Bcpg;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using static System.Net.WebRequestMethods;

namespace GameZoneManagementSystem.Controllers
{
    public class AdminController : Controller
    {

        
        public bool CheckRole()
        {
            int role = 0;

            if(HttpContext.Session.GetString("Role")!=null)
            {
                role = Int32.Parse(HttpContext.Session.GetString("Role"));
                if(role!=1)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        IConfiguration configuration;
        public AdminController(IConfiguration config)
        {
            configuration = config;
        }
        
        // GET: AdminController
        public ActionResult Index()
        {
            if(!CheckRole())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public List<Games_Category> GetGameCategories()
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            List<Games_Category> categories = new List<Games_Category>();
            using (SqlCommand com = new SqlCommand("Select * from Tbl_Games_Category", con))
            {
                con.Open();
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Games_Category category = new Games_Category();
                        category.Id = (int)reader["ID"];
                        category.CategoryName = (string)reader["CategoryName"];
                        categories.Add(category);
                    }
                }
                con.Close();
            }
            return categories;
        }

        public List<Games_Sub_Category> games_Sub_Categories()
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            List<Games_Sub_Category> games=new List<Games_Sub_Category>();
            using (SqlCommand com = new SqlCommand("Select * from Tbl_Games_Sub_Category", con))
            {
                con.Open();
                using (SqlDataReader reader = com.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        
                        Games_Sub_Category sub_Category = new Games_Sub_Category();
                        sub_Category.ID =(int) reader["ID"];
                        sub_Category.Sub_Category_Name =(string) reader["Sub_Category_Name"];
                        sub_Category.CategoryID = (int)reader["CategoryID"];
                        games.Add(sub_Category);
                    }

                }
            }
                con.Close();
                return games;
        }
        public List<Games_Sub_Category> GetSubCategoriesWithCategoryName()
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            List<Games_Sub_Category> subCategories = new List<Games_Sub_Category>();
            using (SqlCommand com = new SqlCommand(
                "SELECT sub.ID, sub.Sub_Category_Name, sub.CategoryID, cat.CategoryName " +
                "FROM Tbl_Games_Sub_Category sub " +
                "INNER JOIN Tbl_Games_Category cat ON sub.CategoryID = cat.ID", con))
            {
                con.Open();
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Games_Sub_Category subCategory = new Games_Sub_Category();
                        subCategory.ID = (int)reader["ID"];
                        subCategory.Sub_Category_Name = (string)reader["Sub_Category_Name"];
                        subCategory.CategoryID = (int)reader["CategoryID"];

                        // Assuming you add a property in Games_Sub_Category to store the category name
                        subCategory.CategoryName = (string)reader["CategoryName"];

                        subCategories.Add(subCategory);
                    }
                }
                con.Close();
            }
            return subCategories;
        }
        [HttpPost]
        public IActionResult UpdateUser(User user)
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            if (user.id == 0)
            {
                return Content("<script>alert('User ID is missing'); window.location.href='/Admin/Users';</script>", "text/html");
            }

            string query = "UPDATE Tbl_Users SET Name=@Name, Email=@Email, Phone=@Phone, Gender=@Gender, DOB=@DOB, RoleID=@Role, Status=@Status WHERE ID=@ID";

            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Phone", user.Phone);
                command.Parameters.AddWithValue("@Gender", user.Gender);
                command.Parameters.AddWithValue("@DOB", user.Dob);
                command.Parameters.AddWithValue("@Role", user.Role);
                command.Parameters.AddWithValue("@Status", user.Status);
                command.Parameters.AddWithValue("@ID", user.id);

                con.Open();
                int rowsAffected = command.ExecuteNonQuery();
                con.Close();

                if (rowsAffected > 0)
                {
                    return RedirectToAction("Users");
                }
                else
                {
                    return Content("<script>alert('Failed to update user'); window.location.href='/Admin/Users';</script>", "text/html");
                }
            }
        }

        public ActionResult Index1()
        {
            return View();
        }
        public ActionResult Deactive(int userid = 0)
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            if (userid==0)
            {

                return View("Index");
            }
            SqlCommand com = new SqlCommand("update Tbl_Users set Status=0 where ID=" + userid,con);
            con.Open();
            if(com.ExecuteNonQuery()>0)
            {
                string scripts = "<script>alert('Status Deactivated of the user');window.location='/Admin/Index'</script>";
                return Content(scripts, "text/html");
            }
            string script = "<script>alert('Status Deactivation failed');window.location='/Admin/Index'</script>";
            return Content(script, "text/html");
        }
        public ActionResult Active(int userid = 0)
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            if (userid == 0)
            {

                return View("Index");
            }
            SqlCommand com = new SqlCommand("update Tbl_Users set Status=1 where ID=" + userid, con);
            con.Open();
            if (com.ExecuteNonQuery() > 0)
            {
                string scripts = "<script>alert('Status Activated of the user');window.location='/Admin/Index'</script>";
                return Content(scripts, "text/html");
            }
            string script = "<script>alert('Status Activation failed');window.location='/Admin/Index'</script>";
            return Content(script, "text/html");
        }
        public ActionResult Index2()
        {
            return View();
        }
        public ActionResult Index3()
        {
            return View();
        }
        public ActionResult AddGames()
        {
            List<Games_Sub_Category> games_Sub_Categories = GetSubCategoriesWithCategoryName();

            return View(games_Sub_Categories);
        }
        [HttpPost]
        public IActionResult addGames(Games game, IFormFile imageFile)
        {
            // Validate the uploaded file
            if (imageFile == null || imageFile.Length == 0)
            {
                string script = "<script>alert('Please upload a valid image file.'+"+imageFile.FileName+");window.location='/Admin/AddGames';</script>";
                return Content(script, "text/html");
            }
            if (game == null || string.IsNullOrEmpty(game.Name) || string.IsNullOrEmpty(game.Game_Description) || game.SubCatID == 0 || game.price == 0)
            {
                TempData["Error"] = "All fields are required.";
                return RedirectToAction("AddGames");
            }
            // File upload logic
            string filePath = string.Empty;
            try
            {
                // Define the folder to save uploaded files
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    addGames(game,imageFile);
                }

                // Generate a unique file name to avoid conflicts
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                // Save relative path for the database
                game.image = "/uploads/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                string script = $"<script>alert('File upload failed: {ex.Message}');window.location='/Admin/AddGames';</script>";
                return Content(script, "text/html");
            }

            // Insert data into the database
            using (SqlConnection connection = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Insert data into Tbl_Games
                    string gameQuery = @"INSERT INTO Tbl_Game (Game, Game_Description, SubCatID, Image) 
                                 VALUES (@Game, @Game_Description, @SubCatID, @Image);
                                 SELECT SCOPE_IDENTITY();";
                    int gameId;
                    using (SqlCommand gameCommand = new SqlCommand(gameQuery, connection, transaction))
                    {
                        gameCommand.Parameters.AddWithValue("@Game", game.Name);
                        gameCommand.Parameters.AddWithValue("@Game_Description", game.Game_Description);
                        gameCommand.Parameters.AddWithValue("@SubCatID", game.SubCatID);
                        gameCommand.Parameters.AddWithValue("@Image", game.image);

                        gameId = Convert.ToInt32(gameCommand.ExecuteScalar());
                    }

                    // Insert price into Tbl_Price
                    string priceQuery = @"INSERT INTO Tbl_Price (GameID, Credits) VALUES (@GameID, @Price)";
                    using (SqlCommand priceCommand = new SqlCommand(priceQuery, connection, transaction))
                    {
                        priceCommand.Parameters.AddWithValue("@GameID", gameId);
                        priceCommand.Parameters.AddWithValue("@Price", game.price);

                        priceCommand.ExecuteNonQuery();
                    }

                    // Commit the transaction
                    transaction.Commit();

                    string script = "<script>alert('Game added successfully!');window.location='/Admin/AddGames';</script>";
                    return Content(script, "text/html");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    string script = $"<script>alert('Failed to add game: {ex.Message}');window.location='/Admin/AddGames';</script>";
                    return Content(script, "text/html");
                }
            }
        }
        public IActionResult EditUser(int id = 0)
        {
            if (id == 0)
            {
                string script = $"<script>alert('User ID not Found');window.location='/Admin/Users';</script>";
                return Content(script, "text/html");
            }

            User user = null;
            string query = "SELECT * FROM Tbl_Users WHERE ID = @id";
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.Parameters.AddWithValue("@id", id);
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            id = (int)reader["ID"],
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            Gender = char.Parse(reader["Gender"].ToString()),
                            Dob = Convert.ToDateTime(reader["DOB"]),
                            Status = (bool)reader["Status"],
                            Role = Convert.ToInt32(reader["RoleID"])
                        };
                    }
                }
                con.Close();
            }

            if (user == null)
            {
                string script = $"<script>alert('User not found');window.location='/Admin/Users';</script>";
                return Content(script, "text/html");
            }

            return View(user); // Pass user to the view
        }

        //---------------------------------------------------------------------------------------

        //public IActionResult register()
        //{

        //    if (string.IsNullOrEmpty(User.Email) || string.IsNullOrEmpty(User.Phone))
        //    {
        //        TempData["Error"] = "Email or Phone cannot be empty.";
        //        return RedirectToAction("Register");
        //    }

        //    using (SqlConnection con = new SqlConnection(this.configuration.GetConnectionString("DefaultConnection")))
        //    {
        //        con.Open();

        //        string checkQuery = "SELECT COUNT(*) FROM Tbl_Users WHERE Email = @Email OR Phone = @Phone";
        //        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
        //        {
        //            checkCmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = User.Email;
        //            checkCmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 15).Value = user.Phone;

        //            int count = (int)checkCmd.ExecuteScalar();
        //            if (count > 0)
        //            {
        //                TempData["Error"] = "User already exists with this Email or Phone.";
        //                return RedirectToAction("Register");
        //            }
        //        }

        //        string insertQuery = @"INSERT INTO Tbl_Users 
        //(Name, DOB, Password, Email, Phone, RoleID, Gender, Status)
        //VALUES (@Name, @Dob, @Password, @Email, @Phone, @RoleID, @Gender, @Status)";

        //        using (SqlCommand cmd = new SqlCommand(insertQuery, con))
        //        {
        //            cmd.Parameters.AddWithValue("@Name", User.Name);
        //            cmd.Parameters.AddWithValue("@Dob", user.Dob ?? (object)DBNull.Value);
        //            cmd.Parameters.AddWithValue("@Password", hp.HashPassword(user.Password));
        //            cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
        //            cmd.Parameters.AddWithValue("@Phone", user.Phone ?? (object)DBNull.Value);
        //            cmd.Parameters.AddWithValue("@RoleID", user.Role);
        //            cmd.Parameters.AddWithValue("@Gender", user.Gender);
        //            cmd.Parameters.AddWithValue("@Status", user.Status);

        //            int rows = cmd.ExecuteNonQuery();
        //            if (rows > 0)
        //            {
        //                TempData["Success"] = "User registered successfully.";
        //                return RedirectToAction("Users");
        //            }
        //            else
        //            {
        //                TempData["Error"] = "Registration failed.";
        //                return RedirectToAction("Register");
        //            }
        //        }
        //    }


        //}

        //------------------------------------------varun---------------------------------
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(User user)
{
    // Check if the email or phone already exists in the database
    using (SqlConnection connection = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
    {
        connection.Open();

        string checkUserQuery = @"SELECT COUNT(*) FROM Tbl_Users WHERE Email = @Email OR Phone = @Phone";
        using (SqlCommand checkCommand = new SqlCommand(checkUserQuery, connection))
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Phone))
            {
                // Handle the case where email or phone is null or empty
                return BadRequest("Email or Phone cannot be empty.");
            }

            checkCommand.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = user.Email;
            checkCommand.Parameters.Add("@Phone", SqlDbType.NVarChar, 15).Value = user.Phone;

            int userCount = (int)checkCommand.ExecuteScalar();
            if (userCount > 0)
            {
                // If the user already exists, redirect with a message
                string script = "<script>alert('You are already registered with this email or phone.');window.location='/Admin/Register';</script>";
                return Content(script, "text/html");
            }
        }
    }

    // Proceed with registration if email/phone are unique
    string query = @"INSERT INTO Tbl_Users 
                     (Name, Dob, Password, Email, Phone, RoleID, Gender, Status) 
                     VALUES (@Name, @Dob, @Password, @Email, @Phone, @Role, @Gender, @Status)";

    using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
    using (SqlCommand command = new SqlCommand(query, con))
    {
        // Add parameters to the SQL command
        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@Dob", user.Dob ?? (object)DBNull.Value);

        var hashedPassword = new HashPasswordController().HashPassword(user.Password);
        command.Parameters.AddWithValue("@Password", hashedPassword);
        command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Phone", user.Phone ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Role", user.Role); // 1 for Admin, 3 for Staff
        command.Parameters.AddWithValue("@Gender", user.Gender);
        command.Parameters.AddWithValue("@Status", true); // Active by default

        // Open the connection and execute the insert
        con.Open();
        int rowsAffected = command.ExecuteNonQuery();
        
        if (rowsAffected > 0)
        {
            // Redirect to the users list or show success message
            string script = "<script>alert('User registered successfully!');window.location='/Admin/Users';</script>";
            return Content(script, "text/html");
        }
        else
        {
            // Registration failed
            string script = "<script>alert('User registration failed!');window.location='/Admin/Register';</script>";
            return Content(script, "text/html");
        }
    }
}




        //------------------------------------------------------------------------------------------

        public IActionResult Users()
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            List<User> users = new List<User>();
            string query = "SELECT * FROM Tbl_Users";
            using (SqlCommand command = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    ;
                    while(reader.Read())
                    {
                        User user = new User();
                        user.id = (int)reader["ID"];
                        user.Name = (string)reader["Name"];
                        user.Email = (string)reader["Email"];
                        user.Phone = (string)reader["Phone"];
                        user.Gender=Char.Parse( reader["Gender"].ToString());
                        user.Dob = (System.DateTime)reader["DOB"];
                        user.Status = (bool)reader["Status"];
                        user.Role = (int)reader["RoleID"];
                        users.Add(user);
                    }

                }
            }
            if (!CheckRole())
            {
                return RedirectToAction("Index", "Home");
            }
            con.Close();
            return View(users);
        }



        public IActionResult Credit()
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            List<Credit> credit = new List<Credit>();
            string query = "SELECT * FROM Tbl_Credits ";
            using (SqlCommand command = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Credit credits = new Credit();
                        credits.id = (int)reader["ID"];
                        credits.credits = Convert.ToDecimal(reader["Credits"]); // Safe conversion
                        credits.userid = (int)reader["UserID"];
                        credit.Add(credits);
                    }
                }
                con.Close();
            }

            if (!CheckRole())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(credit);



        }




        public IActionResult Games()
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            List<Games> games = new List<Games>();
            string query = "SELECT g.ID, g.Game, g.Game_Description, g.SubCatID, g.Image, p.Credits " +
                           "FROM Tbl_Game g " +
                           "LEFT JOIN Tbl_Price p ON g.ID = p.GameID";

            using (SqlCommand command = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Games game = new Games();
                        game.Id = (int)reader["ID"];
                        game.Name= (string)reader["Game"];
                        game.Game_Description = (string)reader["Game_Description"];
                        game.SubCatID = (int)reader["SubCatID"];
                        game.image = (string)reader["Image"];
                        game.price = reader["Credits"] != DBNull.Value ? (decimal)reader["Credits"] : 0;

                        games.Add(game);
                    }
                }
            }
            con.Close();
            return View(games);
        }

        public IActionResult Payments()
        {
            List<Payment> payments = new List<Payment>();

            string query = @"
SELECT 
    p.ID, 
    p.TransactionID, 
    p.Type, 
    p.UserID, 
    p.Date, 
    u.Name AS UserName, 
    ISNULL(c.Credits, 0) AS Credits
FROM Tbl_Payments p
INNER JOIN Tbl_Users u ON p.UserID = u.ID
LEFT JOIN Tbl_Credits c ON p.UserID = c.UserID";
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            using (SqlCommand command = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Payment payment = new Payment
                        {
                            Id = (int)reader["ID"],
                            TransactionId = reader["TransactionID"].ToString(),
                            Type = (bool)reader["Type"],
                            UserId = (int)reader["UserID"],
                            Date = reader["Date"] != DBNull.Value ? (DateTime?)reader["Date"] : null,
                            User = new User
                            {
                                Name = reader["UserName"].ToString()
                            },
                            Credits = Convert.ToDecimal(reader["Credits"])
                        };

                        payments.Add(payment);
                    }
                }
                con.Close();
            }

            if (!CheckRole())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(payments);
        }

        //Add Slot

        public IActionResult AddSlot()
        {;
            AddSlotViewModel viewModel = new AddSlotViewModel
            {
                Games = new List<Games>(),
                Days = new List<DayModel>(),
                TimeSlots = new List<TimeSlotModel>()
            };

            using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
            {
                con.Open();

                // Games
                string gameQuery = "SELECT ID, Game FROM Tbl_Game";
                using (SqlCommand cmd = new SqlCommand(gameQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        viewModel.Games.Add(new Games
                        {
                            Id = Convert.ToInt32(reader["ID"]),
                            Name = reader["Game"].ToString()
                        });
                    }
                }

                // Days
                string dayQuery = "SELECT ID, Day FROM Tbl_Day";
                using (SqlCommand cmd = new SqlCommand(dayQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        viewModel.Days.Add(new DayModel
                        {
                            Id = Convert.ToInt32(reader["ID"]),
                            Name = reader["Day"].ToString()
                        });
                    }
                }

                // Time Slots
                string timeSlotQuery = "SELECT ID, Start_Time, End_Time FROM Tbl_Time";
                using (SqlCommand cmd = new SqlCommand(timeSlotQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        viewModel.TimeSlots.Add(new TimeSlotModel
                        {
                            Id = Convert.ToInt32(reader["ID"]),
                            StartTime = (TimeSpan)reader["Start_Time"],
                            EndTime = (TimeSpan)reader["End_Time"]
                        });
                    }
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddSlot(int GameID, int DayID, int TimeSlotID)
        {

            int slotID;

            using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
            {
                con.Open();

                // 1. Check if the slot record already exists in Tbl_Slot using DayID and TimeSlotID.
                string selectSlotQuery = "SELECT ID FROM Tbl_Slot WHERE DayID = @DayID AND TimeID = @TimeSlotID";
                using (SqlCommand cmdSelectSlot = new SqlCommand(selectSlotQuery, con))
                {
                    cmdSelectSlot.Parameters.AddWithValue("@DayID", DayID);
                    cmdSelectSlot.Parameters.AddWithValue("@TimeSlotID", TimeSlotID);
                    object result = cmdSelectSlot.ExecuteScalar();
                    if (result != null)
                    {
                        slotID = Convert.ToInt32(result);
                    }
                    else
                    {
                        // 2. Insert a new record into Tbl_Slot if not found.
                        string insertSlotQuery = "INSERT INTO Tbl_Slot (DayID, TimeID) VALUES (@DayID, @TimeSlotID); SELECT SCOPE_IDENTITY();";
                        using (SqlCommand cmdInsertSlot = new SqlCommand(insertSlotQuery, con))
                        {
                            cmdInsertSlot.Parameters.AddWithValue("@DayID", DayID);
                            cmdInsertSlot.Parameters.AddWithValue("@TimeSlotID", TimeSlotID);
                            slotID = Convert.ToInt32(cmdInsertSlot.ExecuteScalar());
                        }
                    }
                }

                // 3. Insert into Tbl_Game_Slot to link the game with the slot.
                string insertGameSlotQuery = "INSERT INTO Tbl_Game_Slot (GameID, SlotID) VALUES (@GameID, @SlotID)";
                using (SqlCommand cmdGameSlot = new SqlCommand(insertGameSlotQuery, con))
                {
                    cmdGameSlot.Parameters.AddWithValue("@GameID", GameID);
                    cmdGameSlot.Parameters.AddWithValue("@SlotID", slotID);
                    cmdGameSlot.ExecuteNonQuery();
                }

                con.Close();
            }

            string script = "<script>alert('Slot added successfully!');window.location='/Admin/AddSlot';</script>";
            return Content(script, "text/html");
        }

    }
}
