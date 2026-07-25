using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net;

namespace ShoppingCartService.Helpers
{
    public class APIResponse<T>
    {
        public int StatusCode { get; private set; }
        public string Status { get; private set; }
        public T Data { get; private set; }
        public string Message { get; private set; }

        public APIResponse()
        {
            Status = string.Empty;
            Message = string.Empty;
            Data = default(T)!;
        }

        public APIResponse(T data, string message)
        {
            Status = string.Empty;
            Message = message;
            Data = data;
        }

        public APIResponse(T data, string message, IActionResult status)
        {
            Message = message;
            Data = data;
            var statusCodeResult = (IStatusCodeActionResult)status;

            Status = ((HttpStatusCode)statusCodeResult.StatusCode!.Value).ToString();
            StatusCode = statusCodeResult.StatusCode!.Value;
        }

        public APIResponse(IActionResult status)
        {
            var statusCodeResult = (IStatusCodeActionResult)status;

            Status = ((HttpStatusCode)statusCodeResult.StatusCode!.Value).ToString();
            StatusCode = statusCodeResult.StatusCode.Value;

            Message = string.Empty;
            Data = default(T)!;
        }

        public void SetDataAndMessage(T data, string message)
        {
            Data = data;
            Message = message;
        }

        public void SetStatus(IActionResult status)
        {
            var statusCodeResult = (IStatusCodeActionResult)status;

            Status = ((HttpStatusCode)statusCodeResult.StatusCode!.Value).ToString();
            StatusCode = statusCodeResult.StatusCode.Value;
        }
    }
}
