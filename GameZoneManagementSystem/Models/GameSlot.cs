using System;
using System.ComponentModel.DataAnnotations;

namespace SlotBookingApp.Models
{
    public class GameSlot
    {
        public int Id { get; set; }

        [Required]
        public string GameName { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SlotTime { get; set; }

        public bool IsBooked { get; set; }
    }
}
