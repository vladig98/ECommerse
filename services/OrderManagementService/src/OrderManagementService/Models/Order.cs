namespace OrderManagementService.Models
{
    public class Order
    {
        public Order()
        {
            Id = Guid.NewGuid().ToString();
            Products = new List<OrderProduct>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            UserId = string.Empty;
            ShippingAddress = string.Empty;
            OrderNumber = string.Empty;
            Notes = string.Empty;
            PaymentDetails = new PaymentDetails();
        }

        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UserId { get; set; }

        public List<OrderProduct> Products { get; set; }

        public PaymentOption PaymentOption { get; set; }
        public PaymentDetails PaymentDetails { get; set; }

        public OrderStatus Status { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderNumber { get; set; }
        public string Notes { get; set; }
    }
}
