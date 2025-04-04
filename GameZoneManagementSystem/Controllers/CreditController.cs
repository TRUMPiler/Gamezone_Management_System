using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Razorpay.Api;
using Org.BouncyCastle.Utilities;
using GameZoneManagementSystem.Models;
using System.ComponentModel;
using System.Data.SqlTypes;

namespace GameZoneManagementSystem.Controllers
{
    public class CreditController : Controller
    {
        private readonly string _key = "rzp_test_YpWMzLzMbgtqFk"; // Replace with your Razorpay key
        private readonly string _secret = "lPo9E0pjKqPoCuJqoTDX7yWs"; // Replace with your Razorpay secret
        private readonly SqlConnection _con = new SqlConnection("Data Source=DESKTOP-TN71EG6\\SQLEXPRESS;Initial Catalog=GZMS;Integrated Security=True;");

        public IActionResult Index()
        {
            
            return View();
        }

        public IActionResult PaymentPage()
        {
            if(HttpContext.Session.GetString("Userid")==null)
            {
                string script = "<script>alert('Ohno you need to Login First');window.location='/Home/Login'</script>";
                return Content(script, "text/html");
            }
            String userid=HttpContext.Session.GetString("Userid");
            return View(FetchDetails(userid));
        }

        public IActionResult CreateOrder(decimal amount = 1000)
        {
            // Initialize Razorpay client
            var client = new RazorpayClient(_key, _secret);

            // Create Razorpay order
            var options = new Dictionary<string, object>
            {
                { "amount", (amount * 100) }, // Convert amount to paise
                { "currency", "INR" },
                { "receipt", "receipt#1" },
                { "payment_capture", 1 }
            };

            var order = client.Order.Create(options);
            ViewBag.OrderId = order["id"];

            return RedirectToAction("PaymentPage");
        }

        [HttpPost]
        public IActionResult PaymentSuccessful([FromBody] RazorpayPaymentDetails paymentDetails)
        {
            try
            {
                // Extract payment ID and other details
                string paymentId = paymentDetails.razorpay_payment_id;
                int userId = 1; // Replace with your actual logic for user ID
                decimal paymentAmount = 500; // Replace with the actual payment amount logic

                // Insert payment details into the database
                using (SqlConnection con = new SqlConnection("Data Source=DESKTOP-TN71EG6\\SQLEXPRESS;Initial Catalog=GZMS;Integrated Security=True;"))
                {
                    con.Open();

                    // Check if user already has a credit entry
                    using (SqlCommand checkCmd = new SqlCommand("SELECT count(id) FROM Tbl_Credits WHERE userid = @userid", con))
                    {
                        checkCmd.Parameters.AddWithValue("@userid", userId);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count == 0)
                        {
                            // Insert a new credit entry if none exists
                            using (SqlCommand insertCreditCmd = new SqlCommand("INSERT INTO Tbl_Credits (Credits, UserId) VALUES (@credits, @userid)", con))
                            {
                                if(HttpContext.Session.GetString("Userid")!=null)
                                {
                                    string v = HttpContext.Session.GetString("Userid").ToString();
                                    userId = Int32.Parse(v);

                                    insertCreditCmd.Parameters.AddWithValue("@credits", 0);
                                    insertCreditCmd.Parameters.AddWithValue("@userid", userId);
                                    insertCreditCmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    userId = 1;
                                    Console.WriteLine("Error: Details not found");
                                    return Json(new { success = false,error="Details Not Found" });
                                }
                                
                            }
                        }
                    }

                    // Update the credits for the user
                    using (SqlCommand updateCreditCmd = new SqlCommand("UPDATE Tbl_Credits SET Credits = Credits + @credits WHERE UserId = @userid", con))
                    {
                        updateCreditCmd.Parameters.AddWithValue("@credits", paymentAmount);
                        updateCreditCmd.Parameters.AddWithValue("@userid", userId);
                        updateCreditCmd.ExecuteNonQuery();
                    }

                    // Insert payment details into tbl_payment
                    using (SqlCommand insertPaymentCmd = new SqlCommand(
                        "INSERT INTO Tbl_Payments (userid, TransactionID, Type,Date) VALUES (@userid, @paymentId, @amount,@Date)", con))
                    {
                        insertPaymentCmd.Parameters.AddWithValue("@userid", userId);
                        insertPaymentCmd.Parameters.AddWithValue("@paymentId", paymentId);
                        insertPaymentCmd.Parameters.AddWithValue("@amount", 1);
                        insertPaymentCmd.Parameters.AddWithValue("@Date", DateTime.Now.Date);
                        insertPaymentCmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        public bool AddAmout(int credit,int userid=0)
        {

            if(userid==0)
            {
                return false;
            }
            Credit CurrentDetails = FetchDetails(userid.ToString());
            //if (CurrentDetails.credits >= 999999)
            //{
            //    return false;
            //}
            CurrentDetails.credits = CurrentDetails.credits + credit;
            using (SqlConnection con = new SqlConnection("Data Source=DESKTOP-TN71EG6\\SQLEXPRESS;Initial Catalog=GZMS;Integrated Security=True;"))
            {
                con.Open();
                string query = @"Update Tbl_Credits set Credits=@Credits where UserID=@Userid";
                using (SqlCommand com = new SqlCommand(query, con))
                {
                    com.Parameters.AddWithValue("@Credits", CurrentDetails.credits);
                    com.Parameters.AddWithValue("@Userid", userid);
                    com.ExecuteNonQuery();
                }
            }
            return true;
        }
        public bool ReduceAmout(int credit,int userid=0)
        {
            if (userid==0)
            {
                return false;
            }
            Credit CurrentDetails=FetchDetails(userid.ToString());
            if(CurrentDetails.credits<=credit)
            {
                return false;
            }
            CurrentDetails.credits = CurrentDetails.credits-credit;
            using (SqlConnection con = new SqlConnection("Data Source=DESKTOP-TN71EG6\\SQLEXPRESS;Initial Catalog=GZMS;Integrated Security=True;"))
            {
                con.Open();
                string query = @"Update Tbl_Credits set Credits=@Credits where UserID=@Userid";
                using(SqlCommand com=new SqlCommand(query,con))
                {
                    com.Parameters.AddWithValue("@Credits",CurrentDetails.credits);
                    com.Parameters.AddWithValue("@Userid",userid);
                    com.ExecuteNonQuery();
                }
            }
                return true;
        }
        public Credit FetchDetails(String userid="1")
        {

            Credit g = new Credit();
            SqlCommand com = new SqlCommand("Select * from Tbl_Credits where UserId=@userid",_con);
            com.Parameters.AddWithValue("@userid",userid);
            _con.Open();
            using (SqlDataReader r= com.ExecuteReader())
            {
                if (r.Read())
                {
                    g.userid = (int)r["userid"];
                    g.credits = r["Credits"] != DBNull.Value ? new System.Data.SqlTypes.SqlMoney(Convert.ToDecimal(r["Credits"])) : System.Data.SqlTypes.SqlMoney.Null;

                    g.id = (int)r["id"];
                }
            }
            _con.Close();
            return g;
        }
        public class RazorpayPaymentDetails
        {
            public string razorpay_payment_id { get; set; }
            public string razorpay_order_id { get; set; }
            public string razorpay_signature { get; set; }
        }
    }
}
