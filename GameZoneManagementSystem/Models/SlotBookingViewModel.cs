using Microsoft.AspNetCore.Mvc.Rendering;

namespace GameZoneManagementSystem.Models
{
    public class SlotBookingViewModel
    {
        public int GameID { get; set; }
        public int DayID { get; set; }  
        public int TimeID { get; set; }
        public List<SelectListItem> Days { get; set; }
        public List<SelectListItem> Times { get; set; }
    }
}
