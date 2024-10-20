namespace UserManagementService.Services.Contracts
{
    public interface IRoleManagement
    {
        Task EnsureRoleExistsAsync(string roleName);
    }
}
