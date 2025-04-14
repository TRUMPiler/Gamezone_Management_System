namespace GameZoneManagementSystem.Models
{
    public class GameSlotViewModel
    {
        public int Id { get; set; }              // Tbl_Game_Slot.ID
        public string GameName { get; set; }     // Tbl_Game.Game
        public string Day { get; set; }          // Tbl_Day.Day
        public TimeSpan StartTime { get; set; }  // Tbl_Time.Start_Time
        public TimeSpan EndTime { get; set; }    // Tbl_Time.End_Time
    }
}
