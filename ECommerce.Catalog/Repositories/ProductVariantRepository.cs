namespace ECommerce.Catalog.Repositories;

public class ProductVariantRepository(MainDbContext dbContext, ILogger logger) : IProductVariantRepository
{
    public ApiResponse<ProductVariant> Create(string username, CreateProductVariantDto dto)
    {
        try
        {
            ProductVariant variant = new()
            {
                BasePrice = dto.BasePrice,
                Gtin = dto.Gtin,
                Sku = dto.Sku
            };

            dbContext.ProductVariants.Add(variant);

            logger.Information(
                "Successfully prepared product variant '{Sku}' for creation. User: '{Username}'",
                variant.Sku, username);

            return ApiResponse<ProductVariant>.Success(variant);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while preparing product variant '{Sku}'. User: '{Username}'",
                dto.Sku, username);

            return ApiResponse<ProductVariant>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ApiResponse<ProductVariant>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            ProductVariant? variant = await dbContext.ProductVariants
                .Include(v => v.VariantAttributes)
                .Include(v => v.Media)
                .FirstOrDefaultAsync(v => v.Id == id, token);

            if (variant is null)
            {
                logger.Warning(
                    "Delete aborted: Product variant '{VariantId}' was not found. User: '{Username}'",
                    id, username);

                return ApiResponse<ProductVariant>.NotFound("The requested product variant could not be found. It may have already been removed.");
            }

            dbContext.Entry(variant).Property(p => p.Version).OriginalValue = version;
            dbContext.ProductVariants.Remove(variant);

            logger.Information(
                "Successfully prepared product variant '{VariantId}' (SKU: {Sku}) for deletion. User: '{Username}'.",
                id, variant.Sku, username);

            return ApiResponse<ProductVariant>.Success(variant);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to delete product variant '{VariantId}'. User: '{Username}'",
                id, username);

            return ApiResponse<ProductVariant>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }
    }

    public async Task<ApiResponse<List<ProductVariant>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<ProductVariant> variants = await dbContext.ProductVariants
                .Include(v => v.VariantAttributes)
                .Include(v => v.Media)
                .ToListAsync(token);

            logger.Debug(
                "Retrieved {Count} product variants. User: '{Username}'",
                variants.Count, username);

            return ApiResponse<List<ProductVariant>>.Success(variants);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving all product variants. User: '{Username}'",
                username);

            return ApiResponse<List<ProductVariant>>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<ProductVariant>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            ProductVariant? variant = await dbContext.ProductVariants
                .Include(v => v.VariantAttributes)
                .Include(v => v.Media)
                .FirstOrDefaultAsync(v => v.Id == id, token);

            if (variant is null)
            {
                logger.Warning(
                    "Read aborted: Product variant '{VariantId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<ProductVariant>.NotFound("The requested product variant could not be found.");
            }

            logger.Debug(
                "Retrieved product variant '{VariantId}' (SKU: {Sku}). User: '{Username}'",
                id, variant.Sku, username);

            return ApiResponse<ProductVariant>.Success(variant);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving product variant '{VariantId}'. User: '{Username}'",
                id, username);

            return ApiResponse<ProductVariant>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<ProductVariant>> UpdateAsync(string username, Guid id, Guid version, UpdateProductVariantDto dto, CancellationToken token)
    {
        try
        {
            ProductVariant? variant = await dbContext.ProductVariants.FindAsync(keyValues: [id], cancellationToken: token);
            if (variant is null)
            {
                logger.Warning(
                    "Update aborted: Product variant '{VariantId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<ProductVariant>.NotFound("The requested product variant could not be found.");
            }

            dbContext.Entry(variant).Property(p => p.Version).OriginalValue = version;

            variant.Sku = dto.Sku;
            variant.BasePrice = dto.BasePrice;
            variant.Gtin = dto.Gtin;
            variant.UpdatedAt = DateTime.UtcNow;
            variant.Version = Guid.NewGuid();

            logger.Information(
                "Successfully prepared product variant '{VariantId}' (SKU: {Sku}) for update. User: '{Username}'.",
                id, variant.Sku, username);

            return ApiResponse<ProductVariant>.Success(variant);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to update product variant '{VariantId}'. User: '{Username}'.",
                id, username);

            return ApiResponse<ProductVariant>.Failure("An unexpected error occurred while processing the update. Please try again later.");
        }
    }
}