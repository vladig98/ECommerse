namespace ShoppingCartService.DTOs
{
    public class CartItemDto
    {
        public CartItemDto()
        {
            Id = string.Empty;
            ProductId = string.Empty;
            Quantity = 0;
        }

        public string Id { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
