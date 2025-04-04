namespace GameZoneManagementSystem.Models
{
    public class AddSlotViewModel
    {
        public List<Games> Games { get; set; }
        public List<DayModel> Days { get; set; }
        public List<TimeSlotModel> TimeSlots { get; set; }
    }
    public class DayModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TimeSlotModel
    {
        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

}
