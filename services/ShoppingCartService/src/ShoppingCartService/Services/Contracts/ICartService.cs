namespace ShoppingCartService.Services.Contracts
{
    public interface ICartService
    {
        Task<ServiceResult<CartDto>> AddItemToCart(CreateCartItemDto item);
        Task<ServiceResult<CartDto>> GetCart(string userId);
        Task HandleUserCreatedEvent(UserCreatedEvent userCreatedEvent);
        Task HandleProductCreatedEvent(ProductCreatedEvent productCreatedEvent);
    }
}
