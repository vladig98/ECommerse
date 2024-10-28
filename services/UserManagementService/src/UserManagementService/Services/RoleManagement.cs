using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Services
{
    public class RoleManagement(RoleManager<Role> roleManager, IDataFactory dataFactory) : IRoleManagement
    {
        private readonly RoleManager<Role> _roleManager = roleManager;
        private readonly IDataFactory _dataFactory = dataFactory;

        public async Task EnsureRoleExistsAsync(string roleName)
        {
            if (await DoesRoleExistAsync(roleName))
            {
                return;
            }

            await CreateRoleAsync(roleName);
        }

        private async Task<bool> DoesRoleExistAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        private async Task CreateRoleAsync(string roleName)
        {
            Role role = _dataFactory.CreateRoleInstance(roleName);
            await _roleManager.CreateAsync(role);
        }
    }
}
