using System.ComponentModel.DataAnnotations;

namespace UserManagementService.DTOs
{
    public class EditUserDto
    {
        [EmailAddress]
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? PreferredCurrency { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? Street { get; set; }
        public string? State { get; set; }
    }
}
