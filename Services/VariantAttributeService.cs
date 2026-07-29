namespace ECommerce.Catalog.Services;

public class VariantAttributeService(MainDbContext dbContext, ILogger logger) : IVariantAttributeService
{
    public ApiResponse<VariantAttributeDto> Create(string username, CreateVariantAttributeDto dto)
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

            return new ApiResponse<VariantAttributeDto>(Data: attribute.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while preparing variant attribute '{AttributeName}'. User: '{Username}'",
                dto.Name, username);

            return new ApiResponse<VariantAttributeDto>(Error: "An unexpected error occurred while processing your request. Please try again later.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<VariantAttributeDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            VariantAttribute? attribute = await dbContext.VariantAttributes.FindAsync(keyValues: [id], cancellationToken: token);
            if (attribute is null)
            {
                logger.Warning(
                    "Delete aborted: Variant attribute '{AttributeId}' was not found. User: '{Username}'",
                    id, username);

                return new ApiResponse<VariantAttributeDto>(Error: "The requested variant attribute could not be found. It may have already been removed.", Code: ErrorCodes.NotFound);
            }

            dbContext.Entry(attribute).Property(p => p.Version).OriginalValue = version;
            dbContext.VariantAttributes.Remove(attribute);

            logger.Information(
                "Successfully prepared variant attribute '{AttributeId}' for deletion. User: '{Username}'.",
                id, username);

            return new ApiResponse<VariantAttributeDto>(Data: attribute.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to delete variant attribute '{AttributeId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<VariantAttributeDto>(Error: "An unexpected error occurred while processing the deletion. Please try again later.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<IEnumerable<VariantAttributeDto>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<VariantAttributeDto> attributes = await dbContext.VariantAttributes.Select(x => x.ToDto()).ToListAsync(token);

            logger.Debug(
                "Retrieved {Count} variant attributes. User: '{Username}'",
                attributes.Count, username);

            return new ApiResponse<IEnumerable<VariantAttributeDto>>(Data: attributes);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving all variant attributes. User: '{Username}'",
                username);

            return new ApiResponse<IEnumerable<VariantAttributeDto>>(Error: "An unexpected error occurred while retrieving the data.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<VariantAttributeDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            VariantAttribute? attribute = await dbContext.VariantAttributes.FindAsync(keyValues: [id], cancellationToken: token);
            if (attribute is null)
            {
                logger.Warning(
                    "Read aborted: Variant attribute '{AttributeId}' was not found. User: '{Username}'.",
                    id, username);

                return new ApiResponse<VariantAttributeDto>(Error: "The requested variant attribute could not be found.", Code: ErrorCodes.NotFound);
            }

            logger.Debug(
                "Retrieved variant attribute '{AttributeId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<VariantAttributeDto>(Data: attribute.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving variant attribute '{AttributeId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<VariantAttributeDto>(Error: "An unexpected error occurred while retrieving the data.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<VariantAttributeDto>> UpdateAsync(string username, Guid id, Guid version, UpdateVariantAttributeDto dto, CancellationToken token)
    {
        try
        {
            VariantAttribute? attribute = await dbContext.VariantAttributes.FindAsync(keyValues: [id], cancellationToken: token);
            if (attribute is null)
            {
                logger.Warning(
                    "Update aborted: Variant attribute '{AttributeId}' was not found. User: '{Username}'.",
                    id, username);

                return new ApiResponse<VariantAttributeDto>(Error: "The requested variant attribute could not be found.", Code: ErrorCodes.NotFound);
            }

            dbContext.Entry(attribute).Property(p => p.Version).OriginalValue = version;

            attribute.Name = dto.Name;
            attribute.Value = dto.Value;
            attribute.UpdatedAt = DateTime.UtcNow;
            attribute.Version = Guid.NewGuid();

            logger.Information(
                "Successfully prepared variant attribute '{AttributeId}' for update. User: '{Username}'.",
                id, username);

            return new ApiResponse<VariantAttributeDto>(Data: attribute.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to update variant attribute '{AttributeId}'. User: '{Username}'.",
                id, username);

            return new ApiResponse<VariantAttributeDto>(Error: "An unexpected error occurred while processing the update. Please try again later.", Code: ErrorCodes.Generic);
        }
    }
}