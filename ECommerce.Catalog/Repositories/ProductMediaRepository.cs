namespace ECommerce.Catalog.Repositories;

public class ProductMediaRepository(MainDbContext dbContext, ILogger logger) : IProductMediaRepository
{
    public ApiResponse<ProductMedia> Create(string username, CreateProductMediaDto dto)
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

            return ApiResponse<ProductMedia>.Success(media);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while preparing product media '{MediaUrl}'. User: '{Username}'",
                dto.Url, username);

            return ApiResponse<ProductMedia>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ApiResponse<ProductMedia>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            ProductMedia? media = await dbContext.ProductMedia.FindAsync(keyValues: [id], cancellationToken: token);
            if (media is null)
            {
                logger.Warning(
                    "Delete aborted: Product media '{MediaId}' was not found. User: '{Username}'",
                    id, username);

                return ApiResponse<ProductMedia>.NotFound("The requested product media could not be found. It may have already been removed.");
            }

            dbContext.Entry(media).Property(p => p.Version).OriginalValue = version;
            dbContext.ProductMedia.Remove(media);

            logger.Information(
                "Successfully prepared product media '{MediaId}' for deletion. User: '{Username}'.",
                id, username);

            return ApiResponse<ProductMedia>.Success(media);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to delete product media '{MediaId}'. User: '{Username}'",
                id, username);

            return ApiResponse<ProductMedia>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }
    }

    public async Task<ApiResponse<List<ProductMedia>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<ProductMedia> media = await dbContext.ProductMedia.ToListAsync(token);

            logger.Debug(
                "Retrieved {Count} product media items. User: '{Username}'",
                media.Count, username);

            return ApiResponse<List<ProductMedia>>.Success(media);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving all product media. User: '{Username}'",
                username);

            return ApiResponse<List<ProductMedia>>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<ProductMedia>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            ProductMedia? media = await dbContext.ProductMedia.FindAsync(keyValues: [id], cancellationToken: token);
            if (media is null)
            {
                logger.Warning(
                    "Read aborted: Product media '{MediaId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<ProductMedia>.NotFound("The requested product media could not be found.");
            }

            logger.Debug(
                "Retrieved product media '{MediaId}'. User: '{Username}'",
                id, username);

            return ApiResponse<ProductMedia>.Success(media);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving product media '{MediaId}'. User: '{Username}'",
                id, username);

            return ApiResponse<ProductMedia>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<ProductMedia>> UpdateAsync(string username, Guid id, Guid version, UpdateProductMediaDto dto, CancellationToken token)
    {
        try
        {
            ProductMedia? media = await dbContext.ProductMedia.FindAsync(keyValues: [id], cancellationToken: token);
            if (media is null)
            {
                logger.Warning(
                    "Update aborted: Product media '{MediaId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<ProductMedia>.NotFound("The requested product media could not be found.");
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

            return ApiResponse<ProductMedia>.Success(media);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to update product media '{MediaId}'. User: '{Username}'.",
                id, username);

            return ApiResponse<ProductMedia>.Failure("An unexpected error occurred while processing the update. Please try again later.");
        }
    }
}