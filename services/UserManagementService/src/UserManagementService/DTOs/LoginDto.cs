namespace UserManagementService.DTOs
{
    public class LoginDto
    {
        public LoginDto()
        {
            Username = string.Empty;
            Password = string.Empty;
        }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}
