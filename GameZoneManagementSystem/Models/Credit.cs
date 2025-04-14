using System.Data.SqlTypes;

namespace GameZoneManagementSystem.Models
{
    public class Credit
    {

        public int id {  get; set; }
        public SqlMoney credits {  get; set; }
        public int userid { get; set; }

        public User User { get; set; }
        public string UserName { get; set; }
    }
}
