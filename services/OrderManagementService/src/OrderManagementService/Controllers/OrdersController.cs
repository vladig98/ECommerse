using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrderManagementService.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class OrdersController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetOrder(string id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetOrders()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public IActionResult CreateOrder()
        {
            throw new NotImplementedException();
        }
    }
}
