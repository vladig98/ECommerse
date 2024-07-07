using OrderManagementService.Services.Contracts;

namespace OrderManagementService.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderManagementDbContext _context;

        public OrderService(OrderManagementDbContext context)
        {
            _context = context;
        }

        public Task<ServiceResult<OrderDto>> Create()
        {
            throw new NotImplementedException();
        }

        public async Task HandleProductCreatedEvent(ProductCreatedEvent productCreatedEvent)
        {
            var product = new Product
            {
                Category = productCreatedEvent.Category,
                CreationDate = productCreatedEvent.CreationDate,
                Description = productCreatedEvent.Description,
                Discount = productCreatedEvent.Discount,
                ImageURLs = productCreatedEvent.ImageURLs,
                IsActive = productCreatedEvent.IsActive,
                Name = productCreatedEvent.Name,
                Price = productCreatedEvent.Price,
                ProductId = productCreatedEvent.ProductId,
                Quantity = productCreatedEvent.Quantity,
                Rating = productCreatedEvent.Rating,
                SKU = productCreatedEvent.SKU,
                Tags = productCreatedEvent.Tags,
                UpdatedDate = productCreatedEvent.UpdatedDate
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task HandleUserCreatedEvent(UserCreatedEvent userCreatedEvent)
        {
            var user = new User
            {
                City = userCreatedEvent.City,
                Country = userCreatedEvent.Country,
                DateOfBirth = userCreatedEvent.DateOfBirth,
                Email = userCreatedEvent.Email,
                FirstName = userCreatedEvent.FirstName,
                LastName = userCreatedEvent.LastName,
                LoyaltyPoints = userCreatedEvent.LoyaltyPoints,
                MembershipLevel = userCreatedEvent.MembershipLevel,
                PhoneNumber = userCreatedEvent.PhoneNumber,
                PostalCode = userCreatedEvent.PostalCode,
                PreferredCurrency = userCreatedEvent.PreferredCurrency,
                PreferredLanguage = userCreatedEvent.PreferredLanguage,
                Role = userCreatedEvent.Role,
                State = userCreatedEvent.State,
                Street = userCreatedEvent.Street,
                UserId = userCreatedEvent.UserId,
                Username = userCreatedEvent.Username
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
