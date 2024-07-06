namespace ShoppingCartService.Models
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
