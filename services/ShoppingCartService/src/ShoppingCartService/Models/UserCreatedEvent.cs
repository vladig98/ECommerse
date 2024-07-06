namespace ShoppingCartService.Models
{
    public class UserCreatedEvent
    {
        public UserCreatedEvent()
        {
            UserId = string.Empty;
        }

        public string UserId { get; set; }
    }
}
