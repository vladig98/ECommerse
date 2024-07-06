namespace ShoppingCartService.Models
{
    public class CartItem
    {
        public CartItem()
        {
            Id = Guid.NewGuid().ToString();
            ProductId = string.Empty;
        }

        public string Id { get; set; }
        public string ProductId { get; set; }
    }
}
