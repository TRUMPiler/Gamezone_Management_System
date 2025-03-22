using GameZoneManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
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
        public ActionResult Index1()
        {
            return View();
        }
        public ActionResult Index2()
        {
            return View();
        }
        public ActionResult Index3()
        {
            return View();
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
                    User user = new User();
                    while(reader.Read())
                    {

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
