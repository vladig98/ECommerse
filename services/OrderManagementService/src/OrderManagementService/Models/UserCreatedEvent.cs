namespace OrderManagementService.Models
{
    public class UserCreatedEvent
    {
        public UserCreatedEvent()
        {
            UserId = string.Empty;
            Username = string.Empty;
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            City = string.Empty;
            Country = string.Empty;
            PhoneNumber = string.Empty;
            PostalCode = string.Empty;
            Street = string.Empty;
            State = string.Empty;
            Role = string.Empty;
        }

        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? PreferredCurrency { get; set; }
        public int? LoyaltyPoints { get; set; }
        public string? MembershipLevel { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string Role { get; set; }
    }
}
