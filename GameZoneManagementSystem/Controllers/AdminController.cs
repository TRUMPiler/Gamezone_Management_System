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

        [HttpPost]
        public IActionResult UpdateUser(User user)
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            if (user.id == 0)
            {
                return Content("<script>alert('User ID is missing'); window.location.href='/Admin/Users';</script>", "text/html");
            }

            string query = "UPDATE Tbl_Users SET Name=@Name, Email=@Email, Phone=@Phone, Gender=@Gender, Dob=@DOB, RoleID=@Role, Status=@Status WHERE id=@ID";

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

                if (rowsAffected >= 1)
                {
                    return Content("<script>alert('Update done'); window.location.href='/Admin/Users';</script>", "text/html");
                }
                else
                {
                    return Content("<script>alert('Failed to update user'); window.location.href='/Admin/Users';</script>", "text/html");
                }
            }
        }







        //    public IActionResult EditGame(int id)
        //    {
        //        Games game = new Games();
        //        string query = "SELECT * FROM Tbl_Game WHERE ID = @id";

        //        SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
        //        using (SqlCommand command = new SqlCommand(query, con))
        //        {
        //            command.Parameters.AddWithValue("@id", id);
        //            con.Open();
        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {
        //                if (reader.Read())
        //                {
        //                    game.Id = (int)reader["ID"];
        //                    game.Name = reader["Game"].ToString();
        //                    game.Game_Description = reader["Game_Description"].ToString();
        //                    game.SubCatID = (int)reader["SubCatID"];
        //                    game.image = reader["Image"].ToString();
        //                }
        //            }
        //            con.Close();
        //        }


        //        string subCatQuery = "SELECT ID, Sub_Category_Name FROM Tbl_Games_Sub_Category";
        //        using (SqlCommand cmd = new SqlCommand(subCatQuery, con))
        //    {
        //        con.Open();
        //        using (SqlDataReader rdr = cmd.ExecuteReader())
        //        {
        //            while (rdr.Read())
        //            {
        //                subCategories.Add(new SubCategory
        //                {
        //                    ID = (int) rdr["ID"],
        //                    Sub_Category_Name = rdr["Sub_Category_Name"].ToString()
        //                });
        //            }
        //        }
        //        con.Close();
        //    }
        //}

        //var viewModel = new GameEditViewModel
        //{
        //    Game = game,
        //    SubCategories = subCategories
        //};
        //        return View(game);
        //    }




        public IActionResult DeleteGame(int id)
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            string query = "DELETE FROM Tbl_Game WHERE ID = @id";
            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.Parameters.AddWithValue("@id", id);
                con.Open();
                command.ExecuteNonQuery();
                con.Close();
            }

            return RedirectToAction("Games");
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



        
        public IActionResult EditGame(int id = 0)
        {
            Games game = null;

            if (id > 0)
            {
                using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
                {
                    con.Open();
                    string query = @"
                        SELECT
                            g.ID AS GameID,
                            g.Game AS Name,
                            g.Game_Description,
                            g.SubCatID,
                            sc.ID AS SubCategoryID,
                            sc.Sub_Category_Name,
                            gc.ID AS CategoryID,
                            gc.CategoryName AS CategoryName,
                            g.Image,
                            p.Credits AS Price,
                            g.Status
                        FROM Tbl_Game g
                        INNER JOIN Tbl_Price p ON g.ID = p.GameID
                        INNER JOIN Tbl_Games_Sub_Category sc ON g.SubCatID = sc.ID
                        INNER JOIN Tbl_Games_Category gc ON sc.CategoryID = gc.ID
                        WHERE g.ID = @ID;";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                game = new Games
                                {
                                    Id = (int)reader["GameID"],
                                    Name = reader["Name"].ToString(),
                                    Game_Description = reader["Game_Description"].ToString(),
                                    SubCatID = (int)reader["SubCatID"],
                                    SubCategory = new Games_Sub_Category
                                    {
                                        ID = (int)reader["SubCategoryID"],
                                        Sub_Category_Name = reader["Sub_Category_Name"].ToString(),
                                        CategoryID = (int)reader["CategoryID"],
                                        CategoryName = reader["CategoryName"].ToString()
                                    },
                                    image = reader["Image"].ToString(),
                                    price = Convert.ToDecimal(reader["Price"]),
                                    Status = (bool)reader["Status"]
                                };
                            }
                        }
                    }
                }
            }

            if (game == null)
            {
                string script = "<script>alert('Game not found.');window.location='/Admin/Games'</script>";
                return Content(script, "text/html");
                
            }

            return View(game);
        }

        [HttpPost]
        public IActionResult EditGame1(Games game, IFormFile imageFile)
        {
            // Validate the uploaded file (optional for edit, handle if no new file)
            string filePath = game.image; // Default to existing image path

            if (imageFile != null && imageFile.Length > 0)
            {
                // File upload logic (similar to AddGames)
                try
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    game.image = "/uploads/" + uniqueFileName;

                    // Optionally delete the old image file if it was updated
                    if (!string.IsNullOrEmpty(game.image) && game.image != "/uploads/" + imageFile.FileName) // Basic check
                    {
                        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", game.image.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                            catch (Exception ex)
                            {
                                // Log error
                                Console.WriteLine($"Error deleting old image: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string script = $"<script>alert('File upload failed: {ex.Message}');window.location='/Admin/EditGame/{game.Id}';</script>";
                    return Content(script, "text/html");
                }
            }

            if (game == null || string.IsNullOrEmpty(game.Name) || string.IsNullOrEmpty(game.Game_Description) || game.SubCatID == 0 || game.price == 0)
            {
                TempData["Error"] = "All fields are required.";
                return RedirectToAction("EditGame", new { id = game.Id });
            }

            // Update data in the database
            using (SqlConnection connection = new SqlConnection(this.configuration.GetRequiredSection("ConnectionStrings")["DefaultConnection"]))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Update data in Tbl_Gamet
                    string gameQuery = @"
                        UPDATE Tbl_Game
                        SET Game = @Game,
                            Game_Description = @Game_Description,
                            SubCatID = @SubCatID,
                            Image = @Image,
                            Status = @Status
                        WHERE ID = @ID;";
                    using (SqlCommand gameCommand = new SqlCommand(gameQuery, connection, transaction))
                    {
                        gameCommand.Parameters.AddWithValue("@ID", game.Id);
                        gameCommand.Parameters.AddWithValue("@Game", game.Name);
                        gameCommand.Parameters.AddWithValue("@Game_Description", game.Game_Description);
                        gameCommand.Parameters.AddWithValue("@SubCatID", game.SubCatID);
                        gameCommand.Parameters.AddWithValue("@Image", game.image);
                        gameCommand.Parameters.AddWithValue("@Status", game.Status);
                        gameCommand.ExecuteNonQuery();
                    }

                    // Update price in Tbl_Price
                    string priceQuery = @"
                        UPDATE Tbl_Price
                        SET Credits = @Price
                        WHERE GameID = @GameID;";
                    using (SqlCommand priceCommand = new SqlCommand(priceQuery, connection, transaction))
                    {
                        priceCommand.Parameters.AddWithValue("@GameID", game.Id);
                        priceCommand.Parameters.AddWithValue("@Price", game.price);
                        priceCommand.ExecuteNonQuery();
                    }

                    // Commit the transaction
                    transaction.Commit();

                    string script = "<script>alert('Game updated successfully!');window.location='/Admin/Games';</script>";
                    return Content(script, "text/html");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    string script = $"<script>alert('Failed to update game: {ex.Message}');window.location='/Admin/EditGame/{game.Id}';</script>";
                    return Content(script, "text/html");
                }
            }
        }



        ////---------------------------------------------------------------------------------------

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
            string query = "SELECT * FROM Tbl_Users WHERE RoleID = 2";
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


        //---------------------------------harsh-----------------------------



        public IActionResult GameSlots()
        {
            var connString = configuration.GetConnectionString("DefaultConnection");
            var list = new List<GameSlotViewModel>();

            string sql = @"
                SELECT 
                    gs.ID,
                    g.Game         AS GameName,
                    d.Day          AS DayName,
                    t.Start_Time   AS StartTime,
                    t.End_Time     AS EndTime
                FROM Tbl_Game_Slot gs
                INNER JOIN Tbl_Game g 
                    ON gs.GameID = g.ID
                INNER JOIN Tbl_Slot s 
                    ON gs.SlotID = s.ID
                INNER JOIN Tbl_Day d 
                    ON s.DayID = d.ID
                INNER JOIN Tbl_Time t 
                    ON s.TimeID = t.ID
                ORDER BY gs.ID;
            ";

            using (var con = new SqlConnection(connString))
            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new GameSlotViewModel
                        {
                            Id = (int)rdr["ID"],
                            GameName = (string)rdr["GameName"],
                            Day = (string)rdr["DayName"],
                            StartTime = (TimeSpan)rdr["StartTime"],
                            EndTime = (TimeSpan)rdr["EndTime"]
                        });
                    }
                }
            }

           

            // explicitly specify the view name to match your file:
            return View("GameSlotssss", list);
        }

    

        //---------------------------varun------------------------------------------
        public IActionResult Empployees()
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            List<User> users = new List<User>();
            string query = "SELECT * FROM Tbl_Users WHERE RoleID = 3";
            using (SqlCommand command = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    ;
                    while (reader.Read())
                    {
                        User user = new User();
                        user.id = (int)reader["ID"];
                        user.Name = (string)reader["Name"];
                        user.Email = (string)reader["Email"];
                        user.Phone = (string)reader["Phone"];
                        user.Gender = Char.Parse(reader["Gender"].ToString());
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
        //---------------------------------------------------------------------------

        public IActionResult Credit()
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            List<Credit> credit = new List<Credit>();
            string query = @"SELECT c.ID, c.Credits, c.UserID, u.Name 
                 FROM Tbl_Credits c 
                 INNER JOIN Tbl_Users u ON c.UserID = u.ID";

            using (SqlCommand command = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Credit credits = new Credit();
                        credits.id = (int)reader["ID"];
                        credits.credits = Convert.ToDecimal(reader["Credits"]);
                        credits.userid = (int)reader["UserID"];
                        credits.UserName = reader["Name"].ToString(); // <-- Add this
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

        //-------------------------------Add credit-----------------------------------------

        public IActionResult AddCredit()
        {
            List<User> users = new List<User>();

            using (SqlConnection con = new SqlConnection(this.configuration.GetConnectionString("DefaultConnection")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT ID, Name, Email, Phone FROM Tbl_Users WHERE Status = 1 and RoleID=2", con);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new User
                    {
                        id = (int)reader["ID"],
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"].ToString()
                    });
                }
            }

            return View(users); 
        }

        [HttpPost]
        public IActionResult AddCreditToUser(int userId, decimal amount)
        {
            using (SqlConnection con = new SqlConnection(this.configuration.GetConnectionString("DefaultConnection")))
            {
                con.Open();

                // Check if the user already has a credit record
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Tbl_Credits WHERE UserID = @userId", con);
                checkCmd.Parameters.AddWithValue("@userId", userId);

                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    // Update existing credit
                    SqlCommand updateCmd = new SqlCommand("UPDATE Tbl_Credits SET Credits = @credits+Credits WHERE UserID = @userId", con);
                    updateCmd.Parameters.AddWithValue("@credits", amount);
                    updateCmd.Parameters.AddWithValue("@userId", userId);
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    // Insert new credit
                    SqlCommand insertCmd = new SqlCommand("INSERT INTO Tbl_Credits (Credits, UserID) VALUES (@credits, @userId)", con);
                    insertCmd.Parameters.AddWithValue("@credits", amount);
                    insertCmd.Parameters.AddWithValue("@userId", userId);
                    insertCmd.ExecuteNonQuery();
                }
            }

            TempData["Success"] = "Credits added or updated successfully!";
            return RedirectToAction("AddCredit");
        }



        //------------------------------------------------------------------------------------




        public IActionResult Games()
        {
            SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
            List<Games> games = new List<Games>();
            string query = @"
  SELECT 
    g.ID, 
    g.Game, 
    g.Game_Description, 
    g.SubCatID,
    s.Sub_Category_Name,
    g.Image, 
    p.Credits,
g.Status
  FROM Tbl_Game g
  INNER JOIN Tbl_Games_Sub_Category s 
    ON g.SubCatID = s.ID
  LEFT JOIN Tbl_Price p 
    ON g.ID = p.GameID";

            using (SqlCommand command = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Games game = new Games();
                        game.Id = (int)reader["ID"];
                        game.Name = (string)reader["Game"];
                        game.Game_Description = (string)reader["Game_Description"];
                        game.SubCatID = (int)reader["SubCatID"];
                        game.SubCategory = new Games_Sub_Category
                        {
                            ID = (int)reader["SubCatID"],
                            Sub_Category_Name = (string)reader["Sub_Category_Name"],
                        };
                        //game.SubCategory.Sub_Category_Name = reader["Sub_Category_Name"].ToString();    // ← here
                        game.image = (string)reader["Image"];
                        game.price = reader["Credits"] != DBNull.Value
                                                   ? (decimal)reader["Credits"]
                                                   : 0m;
                        game.Status = (bool)reader["Status"];
                        games.Add(game);
                    }
                }
            }
            con.Close();
            return View(games);
        }



        //-------------------varun--------------------
        public ActionResult ActiveGame(int gameid)
        {
            
            using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
            {
                string query = "UPDATE Tbl_Game SET Status = 1 WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", gameid);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            return RedirectToAction("Games"); // Redirect to your listing page
        }

        public ActionResult DeactiveGame(int gameid)
        {

            using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
            {
                string query = "UPDATE Tbl_Game SET Status = 0 WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", gameid);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            return RedirectToAction("Games"); // Redirect to your listing page
        }
//--------------------------------------------------------------------------------------------------------------


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
        public IActionResult addGames(Games game, IFormFile imageFile)
        {
            // Validate the uploaded file
            if (imageFile == null || imageFile.Length == 0)
            {
                string script = "<script>alert('Please upload a valid image file.'+" + imageFile.FileName + ");window.location='/Admin/AddGames';</script>";
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
                    addGames(game, imageFile);
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
        [HttpPost]
        public IActionResult AddSlot(int GameID, int DayID, int TimeSlotID)
        {
            int slotID = -1; // Initialize to an invalid value

            using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
            {
                con.Open();

                // 1. Check if the slot record already exists in Tbl_Slot for the given DayID and TimeSlotID.
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
                            object newSlotIdResult = cmdInsertSlot.ExecuteScalar();
                            if (newSlotIdResult != null)
                            {
                                slotID = Convert.ToInt32(newSlotIdResult);
                            }
                        }
                    }
                }

                // 3. Check if the link between the Game and the Slot already exists in Tbl_Game_Slot.
                if (slotID != -1) // Only proceed if we have a valid slotID
                {
                    string selectGameSlotQuery = "SELECT ID FROM Tbl_Game_Slot WHERE GameID = @GameID AND SlotID = @SlotID";
                    using (SqlCommand cmdSelectGameSlot = new SqlCommand(selectGameSlotQuery, con))
                    {
                        cmdSelectGameSlot.Parameters.AddWithValue("@GameID", GameID);
                        cmdSelectGameSlot.Parameters.AddWithValue("@SlotID", slotID);
                        object existingGameSlot = cmdSelectGameSlot.ExecuteScalar();

                        if (existingGameSlot == null)
                        {
                            // 4. Insert into Tbl_Game_Slot to link the game with the slot if the link doesn't exist.
                            string insertGameSlotQuery = "INSERT INTO Tbl_Game_Slot (GameID, SlotID) VALUES (@GameID, @SlotID)";
                            using (SqlCommand cmdInsertGameSlot = new SqlCommand(insertGameSlotQuery, con))
                            {
                                cmdInsertGameSlot.Parameters.AddWithValue("@GameID", GameID);
                                cmdInsertGameSlot.Parameters.AddWithValue("@SlotID", slotID);
                                cmdInsertGameSlot.ExecuteNonQuery();
                                con.Close();
                                string script = "<script>alert('Slot added successfully!');window.location='/Admin/AddSlot';</script>";
                                return Content(script, "text/html");
                            }
                        }
                        else
                        {
                            con.Close();
                            string script = "<script>alert('This slot is already added for this game.');window.location='/Admin/AddSlot';</script>";
                            return Content(script, "text/html");
                        }
                    }
                }
                else
                {
                    con.Close();
                    string script = "<script>alert('Error adding slot.');window.location='/Admin/AddSlot';</script>";
                    return Content(script, "text/html");
                }
            }
        }

    }
}
