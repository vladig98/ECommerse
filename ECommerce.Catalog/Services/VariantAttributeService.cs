namespace ECommerce.Catalog.Services;

public class VariantAttributeService(
    IVariantAttributeRepository variantAttributeRepository,
    MainDbContext dbContext,
    ILogger logger) : IVariantAttributeService
{
    public async Task<ApiResponse<VariantAttributeDto>> CreateAsync(string username, CreateVariantAttributeDto dto, CancellationToken token)
    {
        ApiResponse<VariantAttribute> variantAttributeResponse = variantAttributeRepository.Create(username, dto);
        if (!string.IsNullOrWhiteSpace(variantAttributeResponse.Error))
        {
            logger.Warning("Aborting variant attribute creation: Failed to prepare attribute '{Name}: {Value}'. User: '{Username}'", dto.Name, dto.Value, username);
            return ApiResponse<VariantAttributeDto>.FromResponse(variantAttributeResponse);
        }

        VariantAttribute variantAttribute = variantAttributeResponse.Data!;

        try
        {
            await dbContext.SaveChangesAsync(token);
            logger.Information("Successfully committed variant attribute '{Name}: {Value}' (ID: {AttributeId}). User: '{Username}'", variantAttribute.Name, variantAttribute.Value, variantAttribute.Id, username);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency update error while saving attribute '{Name}: {Value}'. User: '{Username}'", dto.Name, dto.Value, username);
            return ApiResponse<VariantAttributeDto>.Conflict("The entity has been modified by another process. Please try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving attribute '{Name}: {Value}'. User: '{Username}'", dto.Name, dto.Value, username);
            return ApiResponse<VariantAttributeDto>.Failure("A database constraint was violated. This attribute name and value combination might already exist.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing attribute '{Name}: {Value}' to the database. User: '{Username}'", dto.Name, dto.Value, username);
            return ApiResponse<VariantAttributeDto>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }

        return ApiResponse<VariantAttributeDto>.Success(variantAttribute.ToDto());
    }

    public async Task<ApiResponse<VariantAttributeDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        ApiResponse<VariantAttribute> attributeResponse = await variantAttributeRepository.DeleteAsync(username, id, version, token);
        if (!string.IsNullOrWhiteSpace(attributeResponse.Error))
        {
            logger.Warning("Aborting variant attribute deletion: Failed to prepare attribute '{AttributeId}' for deletion. User: '{Username}'. Reason: {Error}", id, username, attributeResponse.Error);
            return ApiResponse<VariantAttributeDto>.FromResponse(attributeResponse);
        }

        VariantAttribute attribute = attributeResponse.Data!;

        try
        {
            await dbContext.SaveChangesAsync(token);
            logger.Information("Successfully committed deletion for variant attribute '{Name}: {Value}' (ID: {AttributeId}). User: '{Username}'", attribute.Name, attribute.Value, attribute.Id, username);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency error while deleting attribute '{Name}: {Value}' (ID: {AttributeId}). User: '{Username}'", attribute.Name, attribute.Value, id, username);
            return ApiResponse<VariantAttributeDto>.Conflict("The attribute has been modified by another process. Please refresh and try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while deleting attribute '{Name}: {Value}' (ID: {AttributeId}). User: '{Username}'", attribute.Name, attribute.Value, id, username);
            return ApiResponse<VariantAttributeDto>.Failure("This attribute cannot be deleted because it is currently assigned to one or more product variants.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing deletion of attribute '{Name}: {Value}' (ID: {AttributeId}) to the database. User: '{Username}'", attribute.Name, attribute.Value, id, username);
            return ApiResponse<VariantAttributeDto>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }

        return ApiResponse<VariantAttributeDto>.Success(attribute.ToDto());
    }

    public async Task<ApiResponse<List<VariantAttributeDto>>> GetAllAsync(string username, CancellationToken token)
    {
        ApiResponse<List<VariantAttribute>> attributeResponse = await variantAttributeRepository.GetAllAsync(username, token);
        if (!string.IsNullOrWhiteSpace(attributeResponse.Error))
        {
            logger.Warning("Failed to retrieve variant attributes. User: '{Username}'. Reason: {Error}", username, attributeResponse.Error);
            return ApiResponse<List<VariantAttributeDto>>.FromResponse(attributeResponse);
        }

        List<VariantAttribute> attributes = attributeResponse.Data!;
        return ApiResponse<List<VariantAttributeDto>>.Success([.. attributes.Select(x => x.ToDto())]);
    }

    public async Task<ApiResponse<VariantAttributeDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        ApiResponse<VariantAttribute> attributeResponse = await variantAttributeRepository.GetAsync(username, id, token);
        if (!string.IsNullOrWhiteSpace(attributeResponse.Error))
        {
            logger.Warning("Failed to retrieve variant attribute '{AttributeId}'. User: '{Username}'. Reason: {Error}", id, username, attributeResponse.Error);
            return ApiResponse<VariantAttributeDto>.FromResponse(attributeResponse);
        }

        VariantAttribute attribute = attributeResponse.Data!;
        return ApiResponse<VariantAttributeDto>.Success(attribute.ToDto());
    }

    public async Task<ApiResponse<VariantAttributeDto>> UpdateAsync(string username, Guid id, Guid version, UpdateVariantAttributeDto dto, CancellationToken token)
    {
        ApiResponse<VariantAttribute> attributeResponse = await variantAttributeRepository.UpdateAsync(username, id, version, dto, token);
        if (!string.IsNullOrWhiteSpace(attributeResponse.Error))
        {
            logger.Warning("Aborting variant attribute update: Failed to update attribute '{AttributeId}'. User: '{Username}'", id, username);
            return ApiResponse<VariantAttributeDto>.FromResponse(attributeResponse);
        }

        VariantAttribute attribute = attributeResponse.Data!;

        try
        {
            await dbContext.SaveChangesAsync(token);
            logger.Information("Successfully committed update for variant attribute '{Name}: {Value}' (ID: {AttributeId}). User: '{Username}'", attribute.Name, attribute.Value, attribute.Id, username);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency update error while saving attribute '{Name}: {Value}'. User: '{Username}'", dto.Name, dto.Value, username);
            return ApiResponse<VariantAttributeDto>.Conflict("The entity has been modified by another process. Please try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving attribute '{Name}: {Value}'. User: '{Username}'", dto.Name, dto.Value, username);
            return ApiResponse<VariantAttributeDto>.Failure("A database constraint was violated. This attribute name and value combination might already exist.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing attribute '{Name}: {Value}' to the database. User: '{Username}'", dto.Name, dto.Value, username);
            return ApiResponse<VariantAttributeDto>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }

        return ApiResponse<VariantAttributeDto>.Success(attribute.ToDto());
    }
}