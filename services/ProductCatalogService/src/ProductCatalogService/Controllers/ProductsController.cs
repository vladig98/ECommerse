using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class ProductsController : Controller
    {
        [HttpPost]
        public IActionResult Create(CreateProductDto productData)
        {
            return Ok();
        }

        [HttpGet]
        public IActionResult ListAll()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(string id)
        {
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(string id)
        {
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(string id)
        {
            return Ok();
        }
    }
}
