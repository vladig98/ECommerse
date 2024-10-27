namespace UserManagementService.DTOs
{
    public class RegisterDto
    {
        public TokenDto TokenData { get; set; } = new();
        public UserDTO UserData { get; set; } = new();
    }
}
