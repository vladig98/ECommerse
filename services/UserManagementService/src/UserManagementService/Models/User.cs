using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Models
{
    public class User : IdentityUser<string>
    {
        public User()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            City = string.Empty;
            Country = string.Empty;
            Street = string.Empty;
            PostalCode = string.Empty;
            State = string.Empty;
        }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public string State { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? PreferredCurrency { get; set; }
        public int? LoyaltyPoints { get; set; }
        public string? MembershipLevel { get; set; }
    }
}
