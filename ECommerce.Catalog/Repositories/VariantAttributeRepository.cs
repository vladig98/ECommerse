namespace ECommerce.Catalog.Repositories;

public class VariantAttributeRepository(MainDbContext dbContext, ILogger logger) : IVariantAttributeRepository
{
    public ApiResponse<VariantAttribute> Create(string username, CreateVariantAttributeDto dto)
    {
        try
        {
            VariantAttribute attribute = new()
            {
                Name = dto.Name,
                Value = dto.Value
            };

            dbContext.VariantAttributes.Add(attribute);

            logger.Information(
                "Successfully prepared variant attribute '{AttributeName}' for creation. User: '{Username}'",
                attribute.Name, username);

            return ApiResponse<VariantAttribute>.Success(attribute);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while preparing variant attribute '{AttributeName}'. User: '{Username}'",
                dto.Name, username);

            return ApiResponse<VariantAttribute>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ApiResponse<VariantAttribute>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            VariantAttribute? attribute = await dbContext.VariantAttributes.FindAsync(keyValues: [id], cancellationToken: token);
            if (attribute is null)
            {
                logger.Warning(
                    "Delete aborted: Variant attribute '{AttributeId}' was not found. User: '{Username}'",
                    id, username);

                return ApiResponse<VariantAttribute>.NotFound("The requested variant attribute could not be found. It may have already been removed.");
            }

            dbContext.Entry(attribute).Property(p => p.Version).OriginalValue = version;
            dbContext.VariantAttributes.Remove(attribute);

            logger.Information(
                "Successfully prepared variant attribute '{AttributeId}' for deletion. User: '{Username}'.",
                id, username);

            return ApiResponse<VariantAttribute>.Success(attribute);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to delete variant attribute '{AttributeId}'. User: '{Username}'",
                id, username);

            return ApiResponse<VariantAttribute>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }
    }

    public async Task<ApiResponse<List<VariantAttribute>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<VariantAttribute> attributes = await dbContext.VariantAttributes.ToListAsync(token);

            logger.Debug(
                "Retrieved {Count} variant attributes. User: '{Username}'",
                attributes.Count, username);

            return ApiResponse<List<VariantAttribute>>.Success(attributes);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving all variant attributes. User: '{Username}'",
                username);

            return ApiResponse<List<VariantAttribute>>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<VariantAttribute>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            VariantAttribute? attribute = await dbContext.VariantAttributes.FindAsync(keyValues: [id], cancellationToken: token);
            if (attribute is null)
            {
                logger.Warning(
                    "Read aborted: Variant attribute '{AttributeId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<VariantAttribute>.NotFound("The requested variant attribute could not be found.");
            }

            logger.Debug(
                "Retrieved variant attribute '{AttributeId}'. User: '{Username}'",
                id, username);

            return ApiResponse<VariantAttribute>.Success(attribute);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving variant attribute '{AttributeId}'. User: '{Username}'",
                id, username);

            return ApiResponse<VariantAttribute>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<VariantAttribute>> UpdateAsync(string username, Guid id, Guid version, UpdateVariantAttributeDto dto, CancellationToken token)
    {
        try
        {
            VariantAttribute? attribute = await dbContext.VariantAttributes.FindAsync(keyValues: [id], cancellationToken: token);
            if (attribute is null)
            {
                logger.Warning(
                    "Update aborted: Variant attribute '{AttributeId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<VariantAttribute>.NotFound("The requested variant attribute could not be found.");
            }

            dbContext.Entry(attribute).Property(p => p.Version).OriginalValue = version;

            attribute.Name = dto.Name;
            attribute.Value = dto.Value;
            attribute.UpdatedAt = DateTime.UtcNow;
            attribute.Version = Guid.NewGuid();

            logger.Information(
                "Successfully prepared variant attribute '{AttributeId}' for update. User: '{Username}'.",
                id, username);

            return ApiResponse<VariantAttribute>.Success(attribute);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to update variant attribute '{AttributeId}'. User: '{Username}'.",
                id, username);

            return ApiResponse<VariantAttribute>.Failure("An unexpected error occurred while processing the update. Please try again later.");
        }
    }
}