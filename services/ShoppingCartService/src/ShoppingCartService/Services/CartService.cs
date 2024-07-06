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

            var cart = cartExists ? await _context.Carts.Include(x => x.CartItems).FirstAsync(x => x.UserId == item.UserId) : new Cart
            {
                CreatedAt = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                UpdatedAt = DateTime.UtcNow,
                UserId = item.UserId
            };

            Func<CartItem, bool> cartItemFunc = x => x.ProductId == item.ProductId;

            var itemExists = cart.CartItems.Any(cartItemFunc);
            var cartItem = new CartItem();

            var product = await _context.Products.FirstAsync(x => x.ProductId == item.ProductId);

            if (itemExists)
            {
                cartItem = cart.CartItems.First(cartItemFunc);
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new CartItem
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                cart.CartItems.Add(cartItem);
            }

            if (product.Quantity < cartItem.Quantity)
            {
                return ServiceResult<CartDto>.Failure(GlobalConstants.ProductNotEnough);
            }

            if (!itemExists)
            {
                await _context.CartItems.AddAsync(cartItem);
            }

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
                    Id = cartItemProduct.Id,
                    ProductId = cartItemProduct.ProductId,
                    Quantity = cartItemProduct.Quantity
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

        public async Task<ServiceResult<CartDto>> GetCart(string userId)
        {
            var userExists = await _cartRepository.UserExists(userId);

            if (!userExists)
            {
                return ServiceResult<CartDto>.Failure(GlobalConstants.UserDoesNotExist);
            }

            var cart = await _context.Carts.Include(x => x.CartItems).FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null) 
            {
                return ServiceResult<CartDto>.Failure(GlobalConstants.CartNotExist);
            }

            if (!cart.CartItems.Any())
            {
                return ServiceResult<CartDto>.Failure(GlobalConstants.CartNotExist);
            }

            var items = new List<CartItemDto>();

            foreach (var cartItemProduct in cart.CartItems)
            {
                items.Add(new CartItemDto
                {
                    Id = cartItemProduct.Id,
                    ProductId = cartItemProduct.ProductId,
                    Quantity = cartItemProduct.Quantity
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

            return ServiceResult<CartDto>.Success(cartDto, GlobalConstants.CartFound);
        }
    }
}
