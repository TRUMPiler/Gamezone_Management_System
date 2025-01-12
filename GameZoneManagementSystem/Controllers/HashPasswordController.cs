using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;

namespace GameZoneManagementSystem.Controllers
{
    public class HashPasswordController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index","Home");
        }
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
        // Function to verify if a hashed password matches the user-entered password
        public bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            string hashedEnteredPassword = HashPassword(enteredPassword);
            return string.Equals(hashedEnteredPassword, storedHashedPassword);
        }
    }
}
