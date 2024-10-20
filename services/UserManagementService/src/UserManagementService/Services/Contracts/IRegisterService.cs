namespace UserManagementService.Services.Contracts
{
    public interface IRegisterService
    {
        Task<ServiceResult<RegisterDto>> RegisterUserAsync(CreateUserDTO registerData);
    }
}
