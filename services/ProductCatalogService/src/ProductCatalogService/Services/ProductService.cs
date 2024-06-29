using ProductCatalogService.Services.Contracts;

namespace ProductCatalogService.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductsDbContext _dbContext;

        public ProductService(ProductsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<ProductDto>> CreateProduct(CreateProductDto productData)
        {
            throw new NotImplementedException();
        }
    }
}
