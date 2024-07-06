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

        public async Task AddUser(string userId)
        {
            if (!await UserExists(userId))
            {
                _dbContext.Users.Add(new User { Id = userId });
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> UserExists(string userId)
        {
            return await _dbContext.Users.AnyAsync(u => u.Id == userId);
        }
    }
}
