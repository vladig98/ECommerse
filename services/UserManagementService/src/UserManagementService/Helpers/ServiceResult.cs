namespace UserManagementService.Helpers
{
    public class ServiceResult<T>
    {
        public bool Succeeded { get; set; }
        public T Data { get; set; }
        public Error Error { get; set; }

        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T> { Succeeded = true, Data = data };
        }

        public static ServiceResult<T> Failure(string errorMessage, int statusCode = 400, string status = "error")
        {
            return new ServiceResult<T>
            {
                Succeeded = false,
                Error = new Error
                {
                    ErrorMessage = errorMessage,
                    StatusCode = statusCode,
                    Status = status
                }
            };
        }
    }
}
