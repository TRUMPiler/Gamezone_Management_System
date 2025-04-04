using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
namespace GameZoneManagementSystem.Controllers
{
    public class GameslotController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}