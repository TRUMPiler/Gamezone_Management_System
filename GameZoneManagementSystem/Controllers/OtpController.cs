using Microsoft.AspNetCore.Mvc;
using GameZoneManagementSystem.Controllers;
using GameZoneManagementSystem.Models;
using System.Net.Mail;
using System.Net;
using System.Data.SqlClient;
namespace GameZoneManagementSystem.Controllers
{
    public class OtpController : Controller
    {
        
        EmailSending es = new EmailSending();
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        string otp = "";
        public string SendMail(String To, String subject, bool verification=true)
        {
            es.Email = "22bmiit031@gmail.com";
            es.To = To;
            string randomNum = GenerateRandomNumber(6);
            //HttpContext.Session.SetString("Otp",randomNum);
            using (MailMessage mm = new MailMessage(es.Email, es.To))
            {


                if (verification)
                {
                    string emailBody = $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: 'Arial', sans-serif;
                        
                        background-size: cover;
                        background-position: center;
                        text-align: center;
                        padding: 20px;
                        color: #ffffff;
                    }}
                    .otp-box {{
                        background: rgba(0, 0, 0, 0.8);
                        padding: 20px;
                        border-radius: 10px;
                        box-shadow: 0px 0px 10px rgba(255, 0, 255, 0.5);
                        display: inline-block;
                        margin: 20px;
                    }}
                    .otp-code {{
                        font-size: 30px;
                        font-weight: bold;
                        color: #0f0;
                        background: rgba(255, 255, 255, 0.2);
                        padding: 15px;
                        border-radius: 5px;
                        display: inline-block;
                    }}
                    .btn {{
                        background-color: #ff00ff;
                        color: white;
                        padding: 10px 20px;
                        border-radius: 5px;
                        text-decoration: none;
                        font-weight: bold;
                        display: inline-block;
                        margin-top: 10px;
                    }}
                </style>
            </head>
            <body>
                <div class='otp-box'>
                    <h2>🎮 Welcome to Game Zone 🔥</h2>
                    <p>Enter the *magic OTP code* below:</p>
                    <div class='otp-code'>{randomNum}</div>
                    <p>Enter this OTP in your app before time runs out! ⏳</p>
                    <a href='#' class='btn'>Verify OTP</a>
                </div>
            </body>
            </html>";
                    es.Body = emailBody;
                    es.Subject = "verification Email";
                }
                else
                {
                    es.Body = "Your Password for User ID: is " + subject;
                    es.Subject = "Your Password ";
                }
                //es.Password = "cgqgsvvqvjwswyyq";
                es.Password = "yzqsstjgglsdhbzv";



                mm.Subject = es.Subject;
                mm.Body = es.Body;
                mm.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false; // Set to false
                    NetworkCredential cred = new NetworkCredential(es.Email, es.Password);
                    smtp.Credentials = cred;
                    smtp.Port = 587;
                    smtp.Send(mm);

                }
            }
            return randomNum;


        }
        public IActionResult Index()
        {
            String OTP = SendMail("22bmiit031@gmail.com","OTP");
            return View();
        }
        

        [HttpPost]
        public IActionResult GenerateOtp()
        {
            var otp = GenerateRandomOtp();
            var model = new OtpGen { Otp = otp };
            return View("Index", model);
        }

        private string GenerateRandomOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); 
        }
        public string GenerateRandomNumber(int numberOfDigits)
        {
            Random rand = new Random();
            int minValue = (int)Math.Pow(10, numberOfDigits - 1);
            int maxValue = (int)Math.Pow(10, numberOfDigits) - 1;
            return rand.Next(minValue, maxValue).ToString();
        }
    }
}
