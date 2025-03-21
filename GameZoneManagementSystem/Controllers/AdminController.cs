using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
