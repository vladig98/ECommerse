namespace ECommerce.Catalog.Dtos;

internal record class ApiResponse<T>(
    string? Error = null,
    T? Data = default,
    ErrorCodes Code = ErrorCodes.None
);