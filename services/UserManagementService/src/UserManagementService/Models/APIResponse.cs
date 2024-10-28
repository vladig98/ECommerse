using System.Net;

namespace UserManagementService.Models
{
    public class APIResponse<T>(HttpStatusCode status, string message, T? data = default, params string[] messagePlaceholders)
    {
        public int StatusCode { get; private set; } = (int)status;
        public string Status { get; private set; } = status.ToString();
        public T? Data { get; private set; } = data;
        public string Message { get; private set; } = string.Format(message, messagePlaceholders);
    }
}
