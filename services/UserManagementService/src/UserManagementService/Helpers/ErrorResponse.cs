namespace UserManagementService.Helpers
{
    public class ErrorResponse
    {
        public string Status { get; set; } = "error";
        public ErrorDetails Error { get; set; }
    }
}
