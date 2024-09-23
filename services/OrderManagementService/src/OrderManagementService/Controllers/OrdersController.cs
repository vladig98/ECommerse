using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrderManagementService.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize(Roles = GlobalConstants.Admin)]
        public async Task<IActionResult> ChangeOrdderStatus(string orderId, string status)
        {
            var result = await _orderService.ChangeOrderStatus(orderId, status);

            var response = new APIResponse<OrderDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            response.SetStatus(Ok());
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrder(string id)
        {
            string username = User.FindFirst(GlobalConstants.SubClaim)?.Value;

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
            string username = User.FindFirst(GlobalConstants.SubClaim)?.Value;

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
            string username = User.FindFirst(GlobalConstants.SubClaim)?.Value;

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
