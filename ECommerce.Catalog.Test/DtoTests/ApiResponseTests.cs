using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ECommerce.Catalog.Test.DtoTests;

public class ApiResponseTests
{
    #region Factory Method Tests

    [Fact]
    public void Success_Creates_Valid_Response_With_Data()
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
    public void Failure_Creates_Generic_Error_Response()
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
    public void NotFound_Creates_NotFound_Error_Response()
    {
        // Act
        ApiResponse<Guid> response = ApiResponse<Guid>.NotFound("Entity not found");

        // Assert
        Assert.Equal("Entity not found", response.Error);
        Assert.Equal(ErrorCodes.NotFound, response.Code);
    }

    [Fact]
    public void Conflict_Creates_Conflict_Error_Response()
    {
        // Act
        ApiResponse<bool> response = ApiResponse<bool>.Conflict("Resource version conflict");

        // Assert
        Assert.Equal("Resource version conflict", response.Error);
        Assert.Equal(ErrorCodes.Conflict, response.Code);
    }

    [Fact]
    public void FromResponse_Transforms_Generic_Type_Preserving_Error_And_Code()
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
    public void ToErrorResult_Maps_NotFound_To_NotFoundResult()
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
    public void ToErrorResult_Maps_Conflict_To_PreconditionFailedResult()
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
    public void ToErrorResult_Maps_Generic_Error_To_InternalServerError()
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
    public void ApiResponse_Serializes_And_Deserializes_Correctly()
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