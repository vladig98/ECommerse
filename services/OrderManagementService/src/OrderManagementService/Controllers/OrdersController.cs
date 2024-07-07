using Microsoft.AspNetCore.Mvc;

namespace OrderManagementService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            throw new NotImplementedException();
        }
    }
}
