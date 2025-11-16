using Microsoft.AspNetCore.Identity;

namespace Base_Asp_Core_MVC_with_Identity.Areas.Identity.Data
{
    public class UserSystemIdentity : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //public int UsernameChangeLimit { get; set; } = 10;
        public byte[] ProfilePicture { get; set; }
        public string? CodeUser { get; set; }
        public int? Status { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Position { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
    }
}
