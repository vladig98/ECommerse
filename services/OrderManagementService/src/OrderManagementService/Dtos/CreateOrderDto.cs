namespace OrderManagementService.Dtos
{
    public class CreateOrderDto
    {
        public CreateOrderDto()
        {
            UserId = string.Empty;
            Products = new List<CreateOrderProductDto>();
            PaymentOption = string.Empty;
            ShippinAddress = string.Empty;
            Notes = string.Empty;
            PaymentDetails = new CreatePaymentDto();
        }

        public string UserId { get; set; }
        public List<CreateOrderProductDto> Products { get; set; }
        public string PaymentOption { get; set; }
        public CreatePaymentDto PaymentDetails { get; set; }
        public string ShippinAddress { get; set; }
        public string Notes { get; set; }
    }
}
