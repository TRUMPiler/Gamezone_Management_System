using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Razorpay.Api;
using Org.BouncyCastle.Utilities;
using GameZoneManagementSystem.Models;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Configuration;

namespace GameZoneManagementSystem.Controllers
{
    public class CreditController : Controller
    {
        private readonly string _key = "rzp_test_YpWMzLzMbgtqFk"; // Replace with your Razorpay key
        private readonly string _secret = "lPo9E0pjKqPoCuJqoTDX7yWs"; // Replace with your Razorpay secret
        public IConfiguration configuration;
        public CreditController(IConfiguration config) { configuration = config; }
        

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
                // Extract payment details
                string paymentId = paymentDetails.razorpay_payment_id;
                int userId;
                decimal paymentAmount = 500; // Replace with actual payment amount logic

                // Validate session for UserId
                if (HttpContext.Session.GetString("Userid") != null)
                {
                    userId = Int32.Parse(HttpContext.Session.GetString("Userid"));
                }
                else
                {
                    Console.WriteLine("Error: User ID not found in session");
                    return Json(new { success = false, error = "User ID not found in session" });
                }

                using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
                {
                    con.Open();
                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // Check if user already has a credit entry
                            int count = 0;
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(ID) FROM Tbl_Credits WHERE UserId = @userid", con, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@userid", userId);
                                count = (int)checkCmd.ExecuteScalar();
                            }

                            if (count == 0)
                            {
                                // Insert a new credit entry
                                using (SqlCommand insertCreditCmd = new SqlCommand("INSERT INTO Tbl_Credits (Credits, UserId) VALUES (0, @userid)", con, transaction))
                                {
                                    insertCreditCmd.Parameters.AddWithValue("@userid", userId);
                                    insertCreditCmd.ExecuteNonQuery();
                                }
                            }

                            // Update the credits for the user
                            using (SqlCommand updateCreditCmd = new SqlCommand("UPDATE Tbl_Credits SET Credits = Credits + @credits WHERE UserId = @userid", con, transaction))
                            {
                                updateCreditCmd.Parameters.AddWithValue("@credits", paymentAmount);
                                updateCreditCmd.Parameters.AddWithValue("@userid", userId);
                                updateCreditCmd.ExecuteNonQuery();
                            }

                            // Insert payment details into Tbl_Payments
                            using (SqlCommand insertPaymentCmd = new SqlCommand(
       "INSERT INTO Tbl_Payments (UserId, TransactionID, Type, Date) VALUES (@userid, @paymentId, @type, @Date)", con, transaction))
                            {
                                insertPaymentCmd.Parameters.AddWithValue("@userid", userId);
                                insertPaymentCmd.Parameters.AddWithValue("@paymentId", paymentId);
                                insertPaymentCmd.Parameters.AddWithValue("@type", 1); 
                                insertPaymentCmd.Parameters.AddWithValue("@Date", DateTime.Now);
                                insertPaymentCmd.ExecuteNonQuery();
                            }


                            transaction.Commit(); // Commit transaction if all commands are successful
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback(); // Rollback transaction in case of any error
                            throw new Exception("Transaction failed: " + ex.Message);
                        }
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
            using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
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
            using (SqlConnection con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]))
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
            SqlConnection _con = new SqlConnection(this.configuration.GetSection("ConnectionStrings")["DefaultConnection"]);
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
