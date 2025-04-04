using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameZoneManagementSystem.Models
{
    public class Payment
    {
       
        public int Id { get; set; } 
        public string TransactionId { get; set; } 
        public bool Type { get; set; } 
        public int UserId { get; set; } 
        public DateTime? Date { get; set; } 
        public User User { get; set; } 
    }
}
