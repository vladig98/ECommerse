namespace ECommerce.Identity.Dtos;

public sealed class RegisterDto
{
    public required string Email { get; set; }

    public required string Username { get; set; }

    public required string Password { get; set; }

    public required string ConfirmPassword { get; set; }

    public required string PhoneNumber { get; set; } 
}
