namespace UserManagementService.Services.Interfacces
{
    public interface IRegisterService
    {
        Task<APIResponse<RegisterDto>> RegisterUserAsync(CreateUserDTO registerData);
    }
}
