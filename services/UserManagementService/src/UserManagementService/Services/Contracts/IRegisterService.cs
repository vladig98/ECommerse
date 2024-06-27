namespace UserManagementService.Services.Contracts
{
    public interface IRegisterService
    {
        Task<ServiceResult<RegisterDto>> RegisterUser(CreateUserDTO registerData);
    }
}
