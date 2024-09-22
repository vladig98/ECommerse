using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagementService.Services.Contracts;

namespace OrderManagementService.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ITokenManager _tokenManager;

        public OrdersController(IOrderService orderService, ITokenManager tokenManager)
        {
            _orderService = orderService;
            _tokenManager = tokenManager;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrder(string id)
        {
            var token = Request.Headers.Authorization;
            string username = _tokenManager.ExtractUserNameFromJWT(token!);

            var result = await _orderService.GetOrderById(id, username);

            var response = new APIResponse<OrderDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            response.SetStatus(Ok());
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrders()
        {
            var token = Request.Headers.Authorization;
            string username = _tokenManager.ExtractUserNameFromJWT(token!);

            var result = await _orderService.GetOrders(username);

            var response = new APIResponse<List<OrderDto>>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            response.SetStatus(Ok());
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder(CreateOrderDto orderDetails)
        {
            var token = Request.Headers.Authorization;
            string username = _tokenManager.ExtractUserNameFromJWT(token!);

            var result = await _orderService.Create(orderDetails, username);

            var response = new APIResponse<OrderDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            var status = CreatedAtAction(nameof(CreateOrder), new { id = result.Data.Id }, response);
            response.SetStatus(status);

            return status;
        }
    }
}
