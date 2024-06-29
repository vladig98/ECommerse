namespace ProductCatalogService.Services.Contracts
{
    public interface IProductService
    {
        Task<ServiceResult<ProductDto>> CreateProduct(CreateProductDto productData);
    }
}
