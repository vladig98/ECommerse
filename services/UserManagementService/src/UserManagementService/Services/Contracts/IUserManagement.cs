namespace UserManagementService.Services.Contracts
{
    public interface IUserManagement
    {
        Task<UserManagementResult> CreateUserAsync(CreateUserDTO registerData, string roleName);
    }
}
