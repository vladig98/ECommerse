namespace OrderManagementService.Dtos
{
    public class OrderProductDto
    {
        public OrderProductDto()
        {
            Id = string.Empty;
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
