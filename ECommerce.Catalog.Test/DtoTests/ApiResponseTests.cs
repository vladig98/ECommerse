namespace ECommerce.Catalog.Test.DtoTests;

public class ApiResponseTests
{
    #region Factory Method Tests

    [Fact]
    public void SuccessCreatesValidResponseWithData()
    {
        // Act
        ApiResponse<string> response = ApiResponse<string>.Success("Sample Data");

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Sample Data", response.Data);
        Assert.Null(response.Error);
        Assert.Equal(ErrorCodes.None, response.Code);
    }

    [Fact]
    public void FailureCreatesGenericErrorResponse()
    {
        // Act
        ApiResponse<int> response = ApiResponse<int>.Failure("Something went wrong");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(default, response.Data);
        Assert.Equal("Something went wrong", response.Error);
        Assert.Equal(ErrorCodes.Generic, response.Code);
    }

    [Fact]
    public void NotFoundCreatesNotFoundErrorResponse()
    {
        // Act
        ApiResponse<Guid> response = ApiResponse<Guid>.NotFound("Entity not found");

        // Assert
        Assert.Equal("Entity not found", response.Error);
        Assert.Equal(ErrorCodes.NotFound, response.Code);
    }

    [Fact]
    public void ConflictCreatesConflictErrorResponse()
    {
        // Act
        ApiResponse<bool> response = ApiResponse<bool>.Conflict("Resource version conflict");

        // Assert
        Assert.Equal("Resource version conflict", response.Error);
        Assert.Equal(ErrorCodes.Conflict, response.Code);
    }

    [Fact]
    public void FromResponseTransformsGenericTypePreservingErrorAndCode()
    {
        // Arrange
        ApiResponse<string> sourceResponse = ApiResponse<string>.NotFound("Category missing");

        // Act
        ApiResponse<int> targetResponse = ApiResponse<int>.FromResponse(sourceResponse);

        // Assert
        Assert.Equal(default, targetResponse.Data);
        Assert.Equal(sourceResponse.Error, targetResponse.Error);
        Assert.Equal(sourceResponse.Code, targetResponse.Code);
    }

    #endregion

    #region IResult Mapping Tests (ToErrorResult)

    [Fact]
    public void ToErrorResultMapsNotFoundToNotFoundResult()
    {
        // Arrange
        ApiResponse<string> response = ApiResponse<string>.NotFound("Product not found");

        // Act
        IResult result = response.ToErrorResult();

        // Assert
        NotFound<string?> typedResult = Assert.IsType<NotFound<string?>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, typedResult.StatusCode);
        Assert.Equal("Product not found", typedResult.Value);
    }

    [Fact]
    public void ToErrorResultMapsConflictToPreconditionFailedResult()
    {
        // Arrange
        ApiResponse<string> response = ApiResponse<string>.Conflict("Version mismatch");

        // Act
        IResult result = response.ToErrorResult();

        // Assert
        JsonHttpResult<string?> typedResult = Assert.IsType<JsonHttpResult<string?>>(result);
        Assert.Equal(StatusCodes.Status412PreconditionFailed, typedResult.StatusCode);
        Assert.Equal("Version mismatch", typedResult.Value);
    }

    [Fact]
    public void ToErrorResultMapsGenericErrorToInternalServerError()
    {
        // Arrange
        ApiResponse<string> response = ApiResponse<string>.Failure("Database timeout");

        // Act
        IResult result = response.ToErrorResult();

        // Assert
        InternalServerError<string?> typedResult = Assert.IsType<InternalServerError<string?>>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, typedResult.StatusCode);
        Assert.Equal("Database timeout", typedResult.Value);
    }

    #endregion

    #region Serialization Tests

    [Fact]
    public void ApiResponseSerializesAndDeserializesCorrectly()
    {
        // Arrange
        ApiResponse<string> original = ApiResponse<string>.Success("Payload");

        // Act
        string json = JsonSerializer.Serialize(original);
        ApiResponse<string>? deserialized = JsonSerializer.Deserialize<ApiResponse<string>>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Data, deserialized.Data);
        Assert.Equal(original.Code, deserialized.Code);
        Assert.Null(deserialized.Error);
    }

    #endregion
}