namespace UserManagementService.Services.Contracts
{
    public interface ILoginService
    {
        Task<ServiceResult<TokenDto>> LoginUser(LoginDto loginData);
    }
}
