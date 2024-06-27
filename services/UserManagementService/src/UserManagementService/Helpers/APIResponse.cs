using Microsoft.AspNetCore.Mvc;
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

        public APIResponse(T data, string message, StatusCodeResult status)
        {
            SetDataAndMessage(data, message);
            SetStatus(status);
        }

        public APIResponse(StatusCodeResult status)
        {
            SetStatus(status);
        }

        public void SetDataAndMessage(T data, string message)
        {
            Data = data;
            Message = message;
        }

        public void SetStatus(StatusCodeResult status)
        {
            Status = ((HttpStatusCode)status.StatusCode).ToString();
            StatusCode = status.StatusCode;
        }
    }
}
