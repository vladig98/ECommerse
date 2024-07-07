namespace OrderManagementService.Models
{
    public class PaymentDetails
    {
        public PaymentDetails()
        {
            Id = Guid.NewGuid().ToString();
            CardNumber = string.Empty;
            CardHolder = string.Empty;
            CVC = string.Empty;
        }

        public string Id { get; set; }
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public string CVC { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
