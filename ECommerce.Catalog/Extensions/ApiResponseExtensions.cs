namespace ECommerce.Catalog.Extensions;

internal static class ApiResponseExtensions
{
    extension<T>(ApiResponse<T> result)
    {
        public static ApiResponse<T> Success(T data) =>
            new(Data: data);

        public static ApiResponse<T> Failure(string? error) =>
            new(Error: error, Code: ErrorCodes.Generic);

        public static ApiResponse<T> NotFound(string? error) =>
            new(Error: error, Code: ErrorCodes.NotFound);

        public static ApiResponse<T> Conflict(string? error) =>
            new(Error: error, Code: ErrorCodes.Conflict);

        public static ApiResponse<T> FromResponse<T2>(ApiResponse<T2> response) =>
            new(Error: response.Error, Code: response.Code);

        public IResult ToErrorResult() 
        {
            return result.Code switch
            {
                ErrorCodes.NotFound => TypedResults.NotFound(result.Error),
                ErrorCodes.Conflict => TypedResults.Json(result.Error, statusCode: StatusCodes.Status412PreconditionFailed),
                _ => TypedResults.InternalServerError(result.Error)
            };
        }
    }
}