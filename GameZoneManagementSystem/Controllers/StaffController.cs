using Microsoft.AspNetCore.Mvc;
using GameZoneManagementSystem.Models;
using System.Data.SqlClient;
using GameZoneManagementSystem.Controllers;
using System.Data;
namespace GameZoneManagementSystem.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult index()
        {
            return View();
        }
    }
}