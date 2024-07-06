namespace ShoppingCartService.DTOs
{
    public class CartDto
    {
        public CartDto()
        {
            Id = string.Empty;
            UserId = string.Empty;
            CreatedAt = string.Empty;
            UpdatedAt = string.Empty;
            Items = new List<CartItemDto>();
        }

        public string Id { get; set; }
        public string UserId { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public List<CartItemDto> Items { get; set; }
    }
}
