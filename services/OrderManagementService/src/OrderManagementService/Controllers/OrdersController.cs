using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

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

        [HttpPost("{orderId}")]
        [Authorize(Roles = GlobalConstants.Admin)]
        public async Task<IActionResult> ChangeOrderStatus(string orderId, string status)
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

            response.SetStatus(new StatusCodeResult(303));

            List<SessionLineItemOptions> products = new List<SessionLineItemOptions>();

            foreach (OrderProductDto product in result.Data.Products)
            {
                products.Add(new SessionLineItemOptions()
                {
                    Price = product.Price.ToString(),
                    Quantity = product.Quantity
                });
            }

            SessionCreateOptions options = new SessionCreateOptions()
            {
                LineItems = products,
                Mode = "payment",
                SuccessUrl = "?success=true",
                CancelUrl = "?canceled=true"
            };

            SessionService sessionService = new SessionService();
            Session session = sessionService.Create(options);

            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);
        }
    }
}
