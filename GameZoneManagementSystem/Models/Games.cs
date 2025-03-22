using System.Data.SqlTypes;

namespace GameZoneManagementSystem.Models
{
    public class Games
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Game_Description { get; set; }
        public int SubCatID { get;set; }
        public string image { get; set; }
        public decimal price { get; set; }
    }
}
