namespace UserManagementService.Helpers
{
    public class ServiceResult<T>
    {
        public bool Succeeded { get; private set; }
        public T Data { get; private set; }
        public string Message { get; private set; }

        public static ServiceResult<T> Success(T data, string successMessage)
        {
            return new ServiceResult<T> { Succeeded = true, Data = data, Message = successMessage };
        }

        public static ServiceResult<T> Failure(string errorMessage)
        {
            return new ServiceResult<T> { Succeeded = false, Message = errorMessage };
        }
    }
}
