namespace ECommerce.Catalog.Services;

public class ProductMediaService(MainDbContext dbContext, ILogger logger) : IProductMediaService
{
    public ApiResponse<ProductMediaDto> Create(string username, CreateProductMediaDto dto)
    {
        try
        {
            ProductMedia media = new()
            {
                Url = dto.Url,
                AltText = dto.AltText,
                Type = dto.Type,
                DisplayOrder = dto.DisplayOrder,
                IsPrimary = dto.IsPrimary
            };

            dbContext.ProductMedia.Add(media);

            logger.Information(
                "Successfully prepared product media '{MediaUrl}' for creation. User: '{Username}'",
                media.Url, username);

            return new ApiResponse<ProductMediaDto>(Data: media.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while preparing product media '{MediaUrl}'. User: '{Username}'",
                dto.Url, username);

            return new ApiResponse<ProductMediaDto>(Error: "An unexpected error occurred while processing your request. Please try again later.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductMediaDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            ProductMedia? media = await dbContext.ProductMedia.FindAsync(keyValues: [id], cancellationToken: token);
            if (media is null)
            {
                logger.Warning(
                    "Delete aborted: Product media '{MediaId}' was not found. User: '{Username}'",
                    id, username);

                return new ApiResponse<ProductMediaDto>(Error: "The requested product media could not be found. It may have already been removed.", Code: ErrorCodes.NotFound);
            }

            dbContext.Entry(media).Property(p => p.Version).OriginalValue = version;
            dbContext.ProductMedia.Remove(media);

            logger.Information(
                "Successfully prepared product media '{MediaId}' for deletion. User: '{Username}'.",
                id, username);

            return new ApiResponse<ProductMediaDto>(Data: media.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to delete product media '{MediaId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<ProductMediaDto>(Error: "An unexpected error occurred while processing the deletion. Please try again later.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<IEnumerable<ProductMediaDto>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<ProductMediaDto> media = await dbContext.ProductMedia.Select(x => x.ToDto()).ToListAsync(token);

            logger.Debug(
                "Retrieved {Count} product media items. User: '{Username}'",
                media.Count, username);

            return new ApiResponse<IEnumerable<ProductMediaDto>>(Data: media);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving all product media. User: '{Username}'",
                username);

            return new ApiResponse<IEnumerable<ProductMediaDto>>(Error: "An unexpected error occurred while retrieving the data.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductMediaDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            ProductMedia? media = await dbContext.ProductMedia.FindAsync(keyValues: [id], cancellationToken: token);
            if (media is null)
            {
                logger.Warning(
                    "Read aborted: Product media '{MediaId}' was not found. User: '{Username}'.",
                    id, username);

                return new ApiResponse<ProductMediaDto>(Error: "The requested product media could not be found.", Code: ErrorCodes.NotFound);
            }

            logger.Debug(
                "Retrieved product media '{MediaId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<ProductMediaDto>(Data: media.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving product media '{MediaId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<ProductMediaDto>(Error: "An unexpected error occurred while retrieving the data.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductMediaDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductMediaDto dto, CancellationToken token)
    {
        try
        {
            ProductMedia? media = await dbContext.ProductMedia.FindAsync(keyValues: [id], cancellationToken: token);
            if (media is null)
            {
                logger.Warning(
                    "Update aborted: Product media '{MediaId}' was not found. User: '{Username}'.",
                    id, username);

                return new ApiResponse<ProductMediaDto>(Error: "The requested product media could not be found.", Code: ErrorCodes.NotFound);
            }

            dbContext.Entry(media).Property(p => p.Version).OriginalValue = version;

            media.Url = dto.Url;
            media.AltText = dto.AltText;
            media.Type = dto.Type;
            media.DisplayOrder = dto.DisplayOrder;
            media.IsPrimary = dto.IsPrimary;
            media.UpdatedAt = DateTime.UtcNow;
            media.Version = Guid.NewGuid();

            logger.Information(
                "Successfully prepared product media '{MediaId}' for update. User: '{Username}'.",
                id, username);

            return new ApiResponse<ProductMediaDto>(Data: media.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to update product media '{MediaId}'. User: '{Username}'.",
                id, username);

            return new ApiResponse<ProductMediaDto>(Error: "An unexpected error occurred while processing the update. Please try again later.", Code: ErrorCodes.Generic);
        }
    }
}