namespace GameZoneManagementSystem.Models
{
    public class User
    {
        
        public int id { get; set; }
        public string Name { get; set; }
        public DateTime? Dob { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int Role { get; set; }
        public char Gender { get; set; }
        public bool Status{ get; set; }
    }
}
