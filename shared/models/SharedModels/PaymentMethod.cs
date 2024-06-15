namespace SharedModels
{
    public class PaymentMethod
    {
        public string Id { get; set; }
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public string ExpiryDate { get; set; }
        public int CVC { get; set; }
        public string PaymentType { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
