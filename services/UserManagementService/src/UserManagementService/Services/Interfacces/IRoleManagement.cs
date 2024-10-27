namespace UserManagementService.Services.Interfacces
{
    public interface IRoleManagement
    {
        Task EnsureRoleExistsAsync(string roleName);
    }
}
