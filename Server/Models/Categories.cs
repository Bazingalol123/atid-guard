using AuthWithAdmin.Models;

namespace AuthWithAdmin.Server.Models
{
    public class Categories
    {
        public int ID { get; set; }
        public string categoryName { get; set; }
        public string categoryNameEN { get; set; }
        public string categoryNameAR { get; set; }

        public string? image { get; set; }
        public List<Questions>? questions { get; set; }
    }
}
