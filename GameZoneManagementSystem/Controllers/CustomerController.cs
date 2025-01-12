using Microsoft.AspNetCore.Mvc;
using GameZoneManagementSystem.Models;
using System.Data.SqlClient;
using GameZoneManagementSystem.Controllers;
namespace GameZoneManagementSystem.Controllers
{
    
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            HomeController home = new HomeController();
            if(home.isLoggedIn())
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
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
        [HttpPost]
            {
        }
        
    }
}
