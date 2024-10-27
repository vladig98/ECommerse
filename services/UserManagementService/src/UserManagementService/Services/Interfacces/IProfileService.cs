namespace UserManagementService.Services.Interfacces
{
    public interface IProfileService
    {
        Task<APIResponse<UserDTO>> GetUser(string userId);
        Task<APIResponse<UserDTO>> UpdateUser(string userId, EditUserDto updatedData);
    }
}
