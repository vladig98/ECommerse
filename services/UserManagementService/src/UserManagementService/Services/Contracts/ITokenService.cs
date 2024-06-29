namespace UserManagementService.Services.Contracts
{
    public interface ITokenService
    {
        Task<string> GenerateJWTToken(User user);
    }
}
