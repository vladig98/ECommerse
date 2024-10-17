using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Models
{
    public class Role : IdentityRole<string>
    {
        public Role()
        {
            Users = new List<UserRole>();
            Id = Guid.NewGuid().ToString();
        }

        public ICollection<UserRole> Users { get; set; }
    }
}
