namespace OrderManagementService.Dtos
{
    public class CreatePaymentDto
    {
        public CreatePaymentDto()
        {
            CardHolder = string.Empty;
            CardNumber = string.Empty;
            CVC = string.Empty;
            ExpiryDate = string.Empty;
        }

        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public string CVC { get; set; }
        public string ExpiryDate { get; set; }
    }
}
