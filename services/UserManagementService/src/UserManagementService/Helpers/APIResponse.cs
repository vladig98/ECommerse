using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net;

namespace UserManagementService.Helpers
{
    public class APIResponse<T>
    {
        public int StatusCode { get; private set; }
        public string Status { get; private set; }
        public T Data { get; private set; }
        public string Message { get; private set; }

        public APIResponse()
        {
        }

        public APIResponse(T data, string message)
        {
            SetDataAndMessage(data, message);
        }

        public APIResponse(T data, string message, IActionResult status)
        {
            SetDataAndMessage(data, message);
            SetStatus(status);
        }

        public APIResponse(IActionResult status)
        {
            SetStatus(status);
        }

        public void SetDataAndMessage(T data, string message)
        {
            Data = data;
            Message = message;
        }

        public void SetStatus(IActionResult status)
        {
            var statusCodeResult = (IStatusCodeActionResult)status;

            Status = ((HttpStatusCode)statusCodeResult.StatusCode.Value).ToString();
            StatusCode = statusCodeResult.StatusCode.Value;
        }
    }
}
