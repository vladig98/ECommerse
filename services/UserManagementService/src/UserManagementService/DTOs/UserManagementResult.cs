namespace UserManagementService.DTOs
{
    public class UserManagementResult
    {
        public UserManagementResult()
        {
            ErrorMessage = string.Empty;
            User = null;
            Succeeded = false;
        }

        public string ErrorMessage { get; set; }
        public User? User { get; set; }
        public bool Succeeded { get; set; }
    }
}
