namespace SharedModels
{
    public class Order
    {
        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        public string Id { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }

        public string ClientId { get; set; }
        public User Client { get; set; }

        public DateTime OrderDate { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public string ShippingAddressId { get; set; }
        public Address ShippingAddress { get; set; }

        public string BillingAddressId { get; set; }
        public Address BillingAddress { get; set; }
    }
}
