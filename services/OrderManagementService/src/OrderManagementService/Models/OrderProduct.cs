namespace OrderManagementService.Models
{
    public class OrderProduct
    {
        public OrderProduct()
        {
            Id = Guid.NewGuid().ToString();
            ProductId = string.Empty;
            Name = string.Empty;
        }

        public string Id { get; set; }
        public string ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
