using Microsoft.AspNetCore.Identity;

namespace SharedModels
{
    public class User : IdentityUser<string>
    {
        public User()
        {
            Orders = new List<Order>();
            PaymentMethods = new List<PaymentMethod>();
            Addresses = new List<Address>();
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string PreferedLanguage { get; set; }
        public string PreferredCurrency { get; set; }

        public int LoyaltyPoints { get; set; }
        public string MembershipLevel { get; set; }

        public List<Address> Addresses { get; set; }
        public List<Order> Orders { get; set; }
        public List<PaymentMethod> PaymentMethods { get; set; }
    }
}
