namespace ShoppingCartService.Services.Contracts
{
    public interface ICartRepository
    {
        Task AddUser(string userId);
        Task<bool> UserExists(string userId);
    }
}
