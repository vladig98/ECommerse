namespace OrderManagementService.Services.Contracts
{
    public interface IOrderService
    {
        Task<ServiceResult<OrderDto>> Create(CreateOrderDto orderDetails, string username);
        Task<ServiceResult<OrderDto>> GetOrderById(string id, string username);
        Task<ServiceResult<List<OrderDto>>> GetOrders(string username);
        Task HandleUserCreatedEvent(UserCreatedEvent userCreatedEvent);
        Task HandleProductCreatedEvent(ProductCreatedEvent productCreatedEvent);
    }
}
