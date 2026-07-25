namespace UserManagementService.DTOs
{
    public class RegisterDto
    {
        public RegisterDto()
        {
            TokenData = new TokenDto();
            UserData = new UserDTO();
        }

        public TokenDto TokenData { get; set; }
        public UserDTO UserData { get; set; }
    }
}
