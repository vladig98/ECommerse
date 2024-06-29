using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto productData)
        {
            var result = await _productService.CreateProduct(productData);

            var response = new APIResponse<ProductDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            var status = CreatedAtAction(nameof(Create), new { id = result.Data.Id }, response);
            response.SetStatus(status);

            return status;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var result = await _productService.GetAllProducts();

            var response = new APIResponse<AllProductsDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            response.SetStatus(Ok());
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            var result = await _productService.GetProduct(id);

            var response = new APIResponse<ProductDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            response.SetStatus(Ok());
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, EditProductDto updatedData)
        {
            var result = await _productService.UpdateProduct(id, updatedData);

            var response = new APIResponse<ProductDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            response.SetStatus(Ok());
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var result = await _productService.DeleteProduct(id);

            var response = new APIResponse<ProductDto>(result.Data, result.Message);

            if (!result.Succeeded)
            {
                response.SetStatus(BadRequest());
                return BadRequest(response);
            }

            return NoContent();
        }
    }
}
