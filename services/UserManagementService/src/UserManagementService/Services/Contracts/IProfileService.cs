namespace UserManagementService.Services.Contracts
{
    public interface IProfileService
    {
        Task<ServiceResult<UserDTO>> GetUser(string userId);
        Task<ServiceResult<UserDTO>> UpdateUser(CreateUserDTO updatedData);
    }
}
