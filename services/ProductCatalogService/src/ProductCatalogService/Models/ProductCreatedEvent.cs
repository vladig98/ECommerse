namespace ProductCatalogService.Models
{
    public class ProductCreatedEvent
    {
        public ProductCreatedEvent()
        {
            ProductId = string.Empty;
        }

        public string ProductId { get; set; }
    }
}
