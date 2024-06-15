namespace SharedModels
{
    public class OrderItem
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        public string OrderId { get; set; }
        public Order Order { get; set; }
    }
}
