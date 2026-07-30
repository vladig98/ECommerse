namespace ECommerce.Catalog.Extensions;

public static class ApiResponseExtensions
{
    extension<T>(ApiResponse<T> result)
    {
        public static ApiResponse<T> Success(T data)
        {
            return new ApiResponse<T>(Data: data);
        }

        public static ApiResponse<T> Failure(string? error)
        {
            return new ApiResponse<T>(Error: error, Code: ErrorCodes.Generic);
        }

        public static ApiResponse<T> NotFound(string? error)
        {
            return new ApiResponse<T>(Error: error, Code: ErrorCodes.NotFound);
        }

        public static ApiResponse<T> Conflict(string? error)
        {
            return new ApiResponse<T>(Error: error, Code: ErrorCodes.Conflict);
        }

        public static ApiResponse<T> FromResponse<T2>(ApiResponse<T2> response)
        {
            return new ApiResponse<T>(Error: response.Error, Code: response.Code);
        }

        public IResult ToErrorResult()
        {
            return result.Code switch
            {
                ErrorCodes.NotFound => TypedResults.NotFound(result.Error),
                ErrorCodes.Conflict => TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed),
                _ => TypedResults.InternalServerError(result.Error)
            };
        }
    }
}