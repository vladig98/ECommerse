namespace UserManagementService.Services.Contracts
{
    public interface IUserService
    {
        Task<ServiceResult<UserDTO>> RegisterUser(CreateUserDTO registerData);
    }
}
