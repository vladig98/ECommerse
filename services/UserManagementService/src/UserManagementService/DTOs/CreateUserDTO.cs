using System.ComponentModel.DataAnnotations;

namespace UserManagementService.DTOs
{
    public class CreateUserDTO
    {
        public CreateUserDTO()
        {
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
            City = string.Empty;
            Country = string.Empty;
            PostalCode = string.Empty;
            Street = string.Empty;
            State = string.Empty;
        }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? DateOfBirth { get; set; }

        public string? PreferredLanguage { get; set; }

        public string? PreferredCurrency { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string State { get; set; }
    }
}
