using GameZoneManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using Org.BouncyCastle.Bcpg;
using System.Data.SqlClient;

namespace GameZoneManagementSystem.Controllers
{
    public class AdminController : Controller
    {
       
        SqlConnection con = new SqlConnection("Data Source=NAISHALTUF;Initial Catalog=GZMS;Integrated Security=True;");
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

        public ActionResult Index1()
        {
            return View();
        }
        public ActionResult Deactive(int userid = 0)
        {
            if(userid==0)
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
            using (SqlConnection connection =con)
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


       
        public IActionResult Users()
        {
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
    }
}
