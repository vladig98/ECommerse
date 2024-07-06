using Microsoft.EntityFrameworkCore;

namespace ShoppingCartService.Services
{
    public class CartRepository : ICartRepository
    {
        private readonly ShoppingCartDbContext _dbContext;

        public CartRepository(ShoppingCartDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddUser(User user)
        {
            if (!await UserExists(user.UserId))
            {
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> UserExists(string userId)
        {
            return await _dbContext.Users.AnyAsync(u => u.UserId == userId);
        }

        public async Task AddProduct(Product product)
        {
            if (!await ProductExists(product.Id))
            {
                _dbContext.Products.Add(product);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> ProductExists(string productId)
        {
            return await _dbContext.Products.AnyAsync(p => p.ProductId == productId);
        }
    }
}
