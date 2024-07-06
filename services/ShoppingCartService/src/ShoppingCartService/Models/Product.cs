namespace ShoppingCartService.Models
{
    public class Product
    {
        public Product()
        {
            Id = Guid.NewGuid().ToString();
            ProductId = string.Empty;
        }

        public string Id { get; set; }
        public string ProductId { get; set; }
    }
}
