namespace ShoppingCartService.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public Task<ServiceResult<CartDto>> AddItemToCart(CreateCartItemDto item)
        {
            throw new NotImplementedException();
        }

        public async Task HandleUserCreatedEvent(UserCreatedEvent userCreatedEvent)
        {
            // Store user ID logic
            await _cartRepository.AddUser(userCreatedEvent.UserId);
        }
    }
}
