using Microsoft.AspNetCore.Mvc;

namespace ShoppingCartService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToCart(CreateCartItemDto item)
        {
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmelentFromCart(string id)
        {
            return Ok();
        }
    }
}
