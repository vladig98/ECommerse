namespace ShoppingCartService.Services.Contracts
{
    public interface ICartRepository
    {
        Task AddUser(User user);
        Task<bool> UserExists(string userId);
        Task AddProduct(Product product);
        Task<bool> ProductExists(string productId);
    }
}
