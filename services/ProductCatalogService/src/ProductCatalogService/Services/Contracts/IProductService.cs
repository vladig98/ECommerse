namespace ProductCatalogService.Services.Contracts
{
    public interface IProductService
    {
        Task<ServiceResult<ProductDto>> CreateProduct(CreateProductDto productData);

        Task<ServiceResult<ProductDto>> GetProduct(string id);

        Task<ServiceResult<AllProductsDto>> GetAllProducts();

        Task<ServiceResult<ProductDto>> UpdateProduct(string id, EditProductDto updatedData);

        Task<ServiceResult<ProductDto>> DeleteProduct(string id);
    }
}
