namespace ECommerce.Catalog.Dtos;

public record class ApiResponse<T>(
    string? Error = null,
    T? Data = default,
    ErrorCodes Code = ErrorCodes.None
);
