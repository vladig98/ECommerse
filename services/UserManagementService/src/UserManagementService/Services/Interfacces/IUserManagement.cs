namespace UserManagementService.Services.Interfacces
{
    public interface IUserManagement
    {
        Task<UserManagementResult> CreateUserAsync(CreateUserDTO registerData, string roleName);
    }
}
