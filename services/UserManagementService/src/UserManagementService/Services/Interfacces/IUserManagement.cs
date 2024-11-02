using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace UserManagementService.Services.Interfacces
{
    public interface IUserManagement
    {
        Task<UserManagementResult> CreateUserAsync(CreateUserDTO registerData, string roleName);
        Task<User?> FindByNameAsync(string name);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<User?> FindByIdAsync(string id);
        Task<User?> FindByEmailAsync(string email);
        Task<IdentityResult> UpdateAsync(User user);
        Task<IdentityResult> AddClaimsAsync(User user, Claim[] claims);
        Task<IdentityResult> SetAuthenticationTokenAsync(User user, string loginProvider, string tokenName, string tokenValue);
    }
}
