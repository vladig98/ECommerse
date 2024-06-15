namespace SharedModels
{
    public class Address
    {
        public Address()
        {
            Orders = new List<Order>();
        }

        public string Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public string City { get; set; }
        public string Country { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
        public bool IsDefault { get; set; }

        public List<Order> Orders { get; set; }
    }
}
