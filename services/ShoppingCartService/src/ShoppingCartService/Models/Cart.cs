namespace ShoppingCartService.Models
{
    public class Cart
    {
        public Cart()
        {
            Id = Guid.NewGuid().ToString();
            UserId = string.Empty;
            CartItems = new List<CartItem>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public string Id { get; set; }
        public List<CartItem> CartItems { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
