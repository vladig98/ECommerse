namespace ShoppingCartService.DTOs
{
    public class CreateCartItemDto
    {
        public CreateCartItemDto()
        {
            UserId = string.Empty;
            ProductId = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            SKU = string.Empty;
        }

        public string UserId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public double? Discount { get; set; }
    }
}
