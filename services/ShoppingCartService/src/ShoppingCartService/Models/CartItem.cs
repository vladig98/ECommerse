namespace ShoppingCartService.Models
{
    public class CartItem
    {
        public CartItem()
        {
            Id = Guid.NewGuid().ToString();
            ProductId = string.Empty;
            Quantity = 0;
        }

        public string Id { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
