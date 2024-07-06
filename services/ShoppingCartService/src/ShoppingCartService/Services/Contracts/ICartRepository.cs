namespace ShoppingCartService.Services.Contracts
{
    public interface ICartRepository
    {
        Task AddUser(string userId);
        Task<bool> UserExists(string userId);
        Task AddProduct(string productId);
        Task<bool> ProductExists(string productId);
    }
}
