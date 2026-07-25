namespace ProductCatalogService.DTOs
{
    public class AllProductsDto
    {
        public AllProductsDto()
        {
            Products = new List<ProductDto>();
        }

        public List<ProductDto> Products { get; set; }
    }
}
