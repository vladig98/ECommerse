namespace UserManagementService.Services.Interfacces
{
    public interface ITokenService
    {
        Task<string> GenerateJWTToken(User user);
    }
}
