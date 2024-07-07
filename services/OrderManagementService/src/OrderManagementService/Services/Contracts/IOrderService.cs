namespace OrderManagementService.Services.Contracts
{
    public interface IOrderService
    {
        Task<ServiceResult<OrderDto>> Create();
        Task HandleUserCreatedEvent(UserCreatedEvent userCreatedEvent);
        Task HandleProductCreatedEvent(ProductCreatedEvent productCreatedEvent);
    }
}
