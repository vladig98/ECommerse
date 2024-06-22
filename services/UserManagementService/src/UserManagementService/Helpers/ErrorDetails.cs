namespace UserManagementService.Helpers
{
    public class ErrorDetails
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<string> Details { get; set; } = new List<string>();
    }
}
