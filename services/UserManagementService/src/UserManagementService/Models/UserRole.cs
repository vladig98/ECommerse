using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        public virtual Role Role { get; set; } = new();
        public virtual User User { get; set; } = new();
    }
}
