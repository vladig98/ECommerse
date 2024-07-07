using Microsoft.EntityFrameworkCore;

namespace OrderManagementService.Data
{
    public class OrderManagementDbContext : DbContext
    {
        public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options) : base(options)
        {
        }
    }
}
