namespace UserManagementService.Services.Contracts
{
    public interface IRoleManagement
    {
        Task ManageRoleAsync(string roleName);
    }
}
