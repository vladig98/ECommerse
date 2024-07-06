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
            await _cartRepository.AddUser(userCreatedEvent.UserId);
        }

        public async Task HandleProductCreatedEvent(ProductCreatedEvent productCreatedEvent)
        {
            await _cartRepository.AddProduct(productCreatedEvent.ProductId);
        }
    }
}
