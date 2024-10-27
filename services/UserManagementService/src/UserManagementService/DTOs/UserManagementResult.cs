namespace UserManagementService.DTOs
{
    public class UserManagementResult
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public User? User { get; set; }
        public bool Succeeded { get; set; } = false;
    }
}
