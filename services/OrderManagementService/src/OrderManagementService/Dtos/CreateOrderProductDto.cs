namespace OrderManagementService.Dtos
{
    public class CreateOrderProductDto
    {
        public CreateOrderProductDto()
        {
            ProductId = string.Empty;
        }

        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
