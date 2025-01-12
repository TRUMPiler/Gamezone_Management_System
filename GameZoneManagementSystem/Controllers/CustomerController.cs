using Microsoft.AspNetCore.Mvc;
using GameZoneManagementSystem.Models;
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
            return View();
        }
        [HttpPost]
        public IActionResult register()
        {
            return View();
        }
    }
}
