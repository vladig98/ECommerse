using Microsoft.AspNetCore.Mvc;

namespace ShoppingCartService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart(string userId)
        {
            var result = await _cartService.GetCart(userId);

            var response = new APIResponse<CartDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            response.SetStatus(Ok());
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToCart(CreateCartItemDto item)
        {
            var result = await _cartService.AddItemToCart(item);

            var response = new APIResponse<CartDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            var status = CreatedAtAction(nameof(AddProductToCart), new { id = result.Data.Id }, response);
            response.SetStatus(status);

            return status;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmelentFromCart(string id)
        {
            return Ok();
        }
    }
}
