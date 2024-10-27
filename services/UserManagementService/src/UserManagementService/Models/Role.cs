using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Models
{
    public class Role : IdentityRole<string>
    {
        public virtual ICollection<UserRole> Users { get; } = new List<UserRole>();
        public override string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
