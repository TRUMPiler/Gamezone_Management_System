namespace GameZoneManagementSystem.Models
{
    public class User
    {
        
        public int id { get; set; }

       
        public string FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? LastName { get; set; }
        public DateTime? Dob { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public char Gender { get; set; }
    }
}
