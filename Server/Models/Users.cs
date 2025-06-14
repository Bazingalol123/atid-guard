namespace AuthWithAdmin.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        public bool isAdmin { get; set; } 
    }
}
