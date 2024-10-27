namespace UserManagementService.Services.Interfacces
{
    public interface ILoginService
    {
        Task<APIResponse<TokenDto>> LoginUser(LoginDto loginData);
    }
}
