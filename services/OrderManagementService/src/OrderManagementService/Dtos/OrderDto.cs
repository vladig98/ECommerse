namespace OrderManagementService.Dtos
{
    public class OrderDto
    {
        public OrderDto()
        {
            Id = string.Empty;
            Products = new List<OrderProductDto>();
            CreatedAt = string.Empty;
            UpdatedAt = string.Empty;
            UserId = string.Empty;
            ShippingAddress = string.Empty;
            OrderNumber = string.Empty;
            Notes = string.Empty;
            PaymentDetails = new PaymentDetailsDto();
            PaymentOption = string.Empty;
            Status = string.Empty;
        }

        public string Id { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string UserId { get; set; }

        public List<OrderProductDto> Products { get; set; }

        public string PaymentOption { get; set; }
        public PaymentDetailsDto PaymentDetails { get; set; }

        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderNumber { get; set; }
        public string Notes { get; set; }
    }
}
