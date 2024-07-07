namespace ShoppingCartService.Services.Contracts
{
    public interface ICartService
    {
        Task<ServiceResult<CartDto>> AddItemToCart(CreateCartItemDto item);
        Task<ServiceResult<CartDto>> GetCart(string userId);
        Task<ServiceResult<CartDto>> DeleteItemFromCart(string userId, string productId);
        Task HandleUserCreatedEvent(UserCreatedEvent userCreatedEvent);
        Task HandleProductCreatedEvent(ProductCreatedEvent productCreatedEvent);
    }
}
