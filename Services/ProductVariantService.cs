namespace ECommerce.Catalog.Services;

public class ProductVariantService(MainDbContext dbContext, ILogger logger) : IProductVariantService
{
    public async Task AttachAttributeAsync(string username, Guid variantId, Guid attributeId, CancellationToken token)
    {
        try
        {
            ProductVariant? variant = await dbContext.ProductVariants.FindAsync([variantId], cancellationToken: token);
            VariantAttribute? attribute = await dbContext.VariantAttributes.FindAsync([attributeId], cancellationToken: token);

            if (variant is null || attribute is null)
            {
                logger.Warning(
                    "Attach aborted: Could not link Attribute '{AttributeId}' to Variant '{VariantId}'. One or both entities were not found in the tracker. User: '{Username}'",
                    attributeId, variantId, username);
                return;
            }

            variant.VariantAttributes.Add(new ProductVariantAttribute()
            {
                VariantId = variantId,
                AttributeId = attributeId
            });

            logger.Information(
                "Successfully prepared to attach Attribute '{AttributeId}' to Variant '{VariantId}'. User: '{Username}'",
                attributeId, variantId, username);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while trying to attach Attribute '{AttributeId}' to Variant '{VariantId}'. User: '{Username}'",
                attributeId, variantId, username);
        }
    }

    public async Task AttachMediaAsync(string username, Guid variantId, Guid mediaId, CancellationToken token)
    {
        try
        {
            ProductVariant? variant = await dbContext.ProductVariants.FindAsync([variantId], cancellationToken: token);
            ProductMedia? media = await dbContext.ProductMedia.FindAsync([mediaId], cancellationToken: token);

            if (variant is null || media is null)
            {
                logger.Warning(
                    "Attach aborted: Could not link Media '{MediaId}' to Variant '{VariantId}'. One or both entities were not found in the tracker. User: '{Username}'",
                    mediaId, variantId, username);
                return;
            }

            variant.Media.Add(media);

            logger.Information(
                "Successfully prepared to attach Media '{MediaId}' to Variant '{VariantId}'. User: '{Username}'",
                mediaId, variantId, username);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while trying to attach Media '{MediaId}' to Variant '{VariantId}'. User: '{Username}'",
                mediaId, variantId, username);
        }
    }

    public ApiResponse<ProductVariantDto> Create(string username, CreateProductVariantDto dto)
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

            return new ApiResponse<ProductVariantDto>(Data: variant.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while preparing product variant '{Sku}'. User: '{Username}'",
                dto.Sku, username);

            return new ApiResponse<ProductVariantDto>(Error: "An unexpected error occurred while processing your request. Please try again later.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductVariantDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
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

                return new ApiResponse<ProductVariantDto>(Error: "The requested product variant could not be found. It may have already been removed.", Code: ErrorCodes.NotFound);
            }

            dbContext.Entry(variant).Property(p => p.Version).OriginalValue = version;
            dbContext.ProductVariants.Remove(variant);

            logger.Information(
                "Successfully prepared product variant '{VariantId}' (SKU: {Sku}) for deletion. User: '{Username}'.",
                id, variant.Sku, username);

            return new ApiResponse<ProductVariantDto>(Data: variant.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to delete product variant '{VariantId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<ProductVariantDto>(Error: "An unexpected error occurred while processing the deletion. Please try again later.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<IEnumerable<ProductVariantDto>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<ProductVariantDto> variants = await dbContext.ProductVariants
                .Include(v => v.VariantAttributes)
                .Include(v => v.Media)
                .Select(x => x.ToDto())
                .ToListAsync(token);

            logger.Debug(
                "Retrieved {Count} product variants. User: '{Username}'",
                variants.Count, username);

            return new ApiResponse<IEnumerable<ProductVariantDto>>(Data: variants);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving all product variants. User: '{Username}'",
                username);

            return new ApiResponse<IEnumerable<ProductVariantDto>>(Error: "An unexpected error occurred while retrieving the data.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductVariantDto>> GetAsync(string username, Guid id, CancellationToken token)
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

                return new ApiResponse<ProductVariantDto>(Error: "The requested product variant could not be found.", Code: ErrorCodes.NotFound);
            }

            logger.Debug(
                "Retrieved product variant '{VariantId}' (SKU: {Sku}). User: '{Username}'",
                id, variant.Sku, username);

            return new ApiResponse<ProductVariantDto>(Data: variant.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving product variant '{VariantId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<ProductVariantDto>(Error: "An unexpected error occurred while retrieving the data.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductVariantDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductVariantDto dto, CancellationToken token)
    {
        try
        {
            ProductVariant? variant = await dbContext.ProductVariants.FindAsync(keyValues: [id], cancellationToken: token);
            if (variant is null)
            {
                logger.Warning(
                    "Update aborted: Product variant '{VariantId}' was not found. User: '{Username}'.",
                    id, username);

                return new ApiResponse<ProductVariantDto>(Error: "The requested product variant could not be found.", Code: ErrorCodes.NotFound);
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

            return new ApiResponse<ProductVariantDto>(Data: variant.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to update product variant '{VariantId}'. User: '{Username}'.",
                id, username);

            return new ApiResponse<ProductVariantDto>(Error: "An unexpected error occurred while processing the update. Please try again later.", Code: ErrorCodes.Generic);
        }
    }
}