namespace ShoppingCartService.Services.Contracts
{
    public interface ICartService
    {
        Task<ServiceResult<CartDto>> AddItemToCart(CreateCartItemDto item);
        Task HandleUserCreatedEvent(UserCreatedEvent userCreatedEvent);
    }
}
