using Microsoft.EntityFrameworkCore;

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
                return ServiceResult<CartDto>.Failure("User does not exist!");
            }

            var productExists = await _cartRepository.ProductExists(item.ProductId);

            if (!productExists)
            {
                return ServiceResult<CartDto>.Failure("Product does not exist!");
            }

            var cart = await _context.Carts.FirstOrDefaultAsync(x => x.UserId == item.UserId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CreatedAt = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString(),
                    UpdatedAt = DateTime.UtcNow,
                    UserId = item.UserId
                };
            }

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
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();

            var cartDto = new CartDto
            {
                
            };

            return ServiceResult<CartDto>.Success(cartDto, "Product added to cart!");
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
