
namespace AuthWithAdmin.Shared.AuthSharedModels
{
    public class UserForAdmin
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime RegisterDate { get; set; }

        public List<string> Roles{ get; set; }
    }


}
