using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ShoppingCartService.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ShoppingCartDbContext _context;

        public CartService(ICartRepository cartRepository, ShoppingCartDbContext context)
        {
            _cartRepository = cartRepository;
            _context = context;
        }

        public async Task<ServiceResult<CartDto>> AddItemToCart(CreateCartItemDto item)
        {
            var userExists = await _cartRepository.UserExists(item.UserId);

            if (!userExists)
            {
                return ServiceResult<CartDto>.Failure(GlobalConstants.UserDoesNotExist);
            }

            var productExists = await _cartRepository.ProductExists(item.ProductId);

            if (!productExists)
            {
                return ServiceResult<CartDto>.Failure(GlobalConstants.ProductDoesNotExist);
            }

            var cartExists = await _context.Carts.AnyAsync(x => x.UserId == item.UserId);

            var cart = cartExists ? await _context.Carts.FirstAsync(x => x.UserId == item.UserId) : new Cart
            {
                CreatedAt = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                UpdatedAt = DateTime.UtcNow,
                UserId = item.UserId
            };

            var cartItem = new CartItem
            {
                Description = item.Description,
                Discount = item.Discount,
                Id = Guid.NewGuid().ToString(),
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity,
                SKU = item.SKU,
                ProductId = item.ProductId
            };

            cart.CartItems.Add(cartItem);

            await _context.CartItems.AddAsync(cartItem);

            if (!cartExists)
            {
                await _context.Carts.AddAsync(cart);
            }

            await _context.SaveChangesAsync();

            var items = new List<CartItemDto>();

            foreach (var cartItemProduct in cart.CartItems)
            {
                items.Add(new CartItemDto
                {
                    Description = cartItemProduct.Description,
                    Discount = cartItemProduct.Discount,
                    Id = cartItemProduct.Id,
                    Name = cartItemProduct.Name,
                    Price = cartItemProduct.Price,
                    ProductId = cartItemProduct.ProductId,
                    Quantity = cartItemProduct.Quantity,
                    SKU = cartItemProduct.SKU
                });
            }

            var cartDto = new CartDto
            {
                CreatedAt = cart.CreatedAt.ToString(GlobalConstants.DateFormat, CultureInfo.InvariantCulture),
                Id = cart.Id,
                UpdatedAt = cart.UpdatedAt.ToString(GlobalConstants.DateFormat, CultureInfo.InvariantCulture),
                UserId = cart.UserId,
                Items = items
            };

            return ServiceResult<CartDto>.Success(cartDto, GlobalConstants.ProductAddedSuccessfully);
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

            await _cartRepository.AddUser(user);
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

            await _cartRepository.AddProduct(product);
        }
    }
}
