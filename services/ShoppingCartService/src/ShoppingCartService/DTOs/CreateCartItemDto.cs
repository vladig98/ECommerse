namespace ShoppingCartService.DTOs
{
    public class CreateCartItemDto
    {
        public CreateCartItemDto()
        {
            UserId = string.Empty;
            ProductId = string.Empty;
            Quantity = 0;
        }

        public string UserId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
