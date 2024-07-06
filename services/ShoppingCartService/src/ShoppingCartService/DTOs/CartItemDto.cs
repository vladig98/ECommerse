namespace ShoppingCartService.DTOs
{
    public class CartItemDto
    {
        public CartItemDto()
        {
            Id = string.Empty;
            ProductId = string.Empty;
            Quantity = 0;
            Price = 0m;
            Name = string.Empty;
            Description = string.Empty;
            SKU = string.Empty;
            Discount = 0;
        }

        public string Id { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public double? Discount { get; set; }
    }
}
