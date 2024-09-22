namespace OrderManagementService.Services.Contracts
{
    public interface ITokenManager
    {
        string ExtractUserNameFromJWT(string jwt);
    }
}
