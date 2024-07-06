namespace ShoppingCartService.Models
{
    public class User
    {
        public User()
        {
            Id = Guid.NewGuid().ToString();
            UserId = string.Empty;
        }

        public string Id { get; set; }
        public string UserId { get; set; }
    }
}
