using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using GameZoneManagementSystem.Models;

namespace GameZoneManagementSystem.Controllers
{
    public class SlotBookingController : Controller
    {
        private readonly string _connectionString = "Data Source=NAISHALTUF;Initial Catalog=GZMS;Integrated Security=True;";

        // Display available slots for booking
        public IActionResult Index(int gameId)
        {
            List<SlotViewModel> availableSlots = GetAvailableSlots(gameId);
            ViewBag.GameId = gameId;
            return View(availableSlots);
        }

        // Fetch available slots for a given game
        private List<SlotViewModel> GetAvailableSlots(int gameId)
        {
            var slots = new List<SlotViewModel>();

            string query = @"
                SELECT s.ID, d.DayName, t.TimeSlot 
                FROM Tbl_Slot s
                INNER JOIN Tbl_Day d ON s.DayID = d.ID
                INNER JOIN Tbl_Time t ON s.TimeID = t.ID
                LEFT JOIN Tbl_Game_Slot gs ON gs.SlotID = s.ID AND gs.GameID = @GameID
                WHERE gs.ID IS NULL"; // Only unbooked slots

            using (var con = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@GameID", gameId);
                    con.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            slots.Add(new SlotViewModel
                            {
                                SlotID = (int)reader["ID"],
                                DayName = reader["DayName"].ToString(),
                                TimeSlot = reader["TimeSlot"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return slots;
        }

        // Handle booking logic
        [HttpPost]
        public IActionResult BookSlot(int gameId, int slotId)
        {
            int userId = GetUserId(); // Method to get the logged-in user's ID
            int creditsRequired = 50; // Example: 50 credits per booking

            // Check if user has enough credits
            if (HasEnoughCredits(userId, creditsRequired))
            {
                // Deduct credits and book the slot
                DeductCredits(userId, creditsRequired);
                BookGameSlot(gameId, slotId);

                TempData["Message"] = "Slot booked successfully!";
            }
            else
            {
                TempData["Error"] = "Not enough credits!";
            }

            return RedirectToAction("Index", new { gameId });
        }

        // Method to fetch user ID (Example, adapt based on your authentication system)
        private int GetUserId()
        {
            // Assume that the user ID is hardcoded or fetched from session or authentication.
            return 1; // Replace with actual user authentication logic.
        }

        // Check if the user has sufficient credits
        private bool HasEnoughCredits(int userId, int creditsRequired)
        {
            int userCredits = 0;

            using (var con = new SqlConnection(_connectionString))
            {
                string query = "SELECT Credits FROM Tbl_User WHERE ID = @UserID";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();
                    userCredits = (int)cmd.ExecuteScalar();
                    con.Close();
                }
            }

            return userCredits >= creditsRequired;
        }

        // Deduct credits after booking a slot
        private void DeductCredits(int userId, int creditsRequired)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Tbl_User SET Credits = Credits - @CreditsRequired WHERE ID = @UserID";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CreditsRequired", creditsRequired);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        // Book the game slot in Tbl_Game_Slot
        private void BookGameSlot(int gameId, int slotId)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Tbl_Game_Slot (GameID, SlotID) VALUES (@GameID, @SlotID)";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@GameID", gameId);
                    cmd.Parameters.AddWithValue("@SlotID", slotId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }
    }
}
