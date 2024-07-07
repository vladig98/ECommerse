namespace OrderManagementService.Dtos
{
    public class PaymentDetailsDto
    {
        public PaymentDetailsDto()
        {
            Id = string.Empty;
            CardNumber = string.Empty;
            CardHolder = string.Empty;
            CVC = string.Empty;
            ExpiryDate = string.Empty;
        }

        public string Id { get; set; }
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public string CVC { get; set; }
        public string ExpiryDate { get; set; }
    }
}
