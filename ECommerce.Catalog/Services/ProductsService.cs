namespace ECommerce.Catalog.Services;

public class ProductsService(MainDbContext dbContext, ILogger logger) : IProductsService
{
    public async Task AttachCategoryAsync(string username, Guid productId, Guid categoryId, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products.FindAsync([productId], cancellationToken: token);
            Category? category = await dbContext.Categories.FindAsync([categoryId], cancellationToken: token);

            if (product is null || category is null)
            {
                logger.Warning(
                    "Attach aborted: Could not link Category '{CategoryId}' to Product '{ProductId}'. One or both entities were not found in the tracker. User: '{Username}'",
                    categoryId, productId, username);
                return;
            }

            product.CategoryId = categoryId;

            logger.Information(
                "Successfully prepared to attach Category '{CategoryId}' to Product '{ProductId}'. User: '{Username}'",
                categoryId, productId, username);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while trying to attach Category '{CategoryId}' to Product '{ProductId}'. User: '{Username}'",
                categoryId, productId, username);
        }
    }

    public async Task AttachMediaAsync(string username, Guid productId, Guid mediaId, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products.FindAsync([productId], cancellationToken: token);
            ProductMedia? media = await dbContext.ProductMedia.FindAsync([mediaId], cancellationToken: token);

            if (product is null || media is null)
            {
                logger.Warning(
                    "Attach aborted: Could not link Media '{MediaId}' to Product '{ProductId}'. One or both entities were not found in the tracker. User: '{Username}'",
                    mediaId, productId, username);
                return;
            }

            product.Media.Add(media);

            logger.Information(
                "Successfully prepared to attach Media '{MediaId}' to Product '{ProductId}'. User: '{Username}'",
                mediaId, productId, username);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while trying to attach Media '{MediaId}' to Product '{ProductId}'. User: '{Username}'",
                mediaId, productId, username);
        }
    }

    public async Task AttachVariantAsync(string username, Guid productId, Guid variantId, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products.FindAsync([productId], cancellationToken: token);
            ProductVariant? variant = await dbContext.ProductVariants.FindAsync([variantId], cancellationToken: token);

            if (product is null || variant is null)
            {
                logger.Warning(
                    "Attach aborted: Could not link Variant '{VariantId}' to Product '{ProductId}'. One or both entities were not found in the tracker. User: '{Username}'",
                    variantId, productId, username);
                return;
            }

            product.Variants.Add(variant);

            logger.Information(
                "Successfully prepared to attach Variant '{VariantId}' to Product '{ProductId}'. User: '{Username}'",
                variantId, productId, username);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while trying to attach Variant '{VariantId}' to Product '{ProductId}'. User: '{Username}'",
                variantId, productId, username);
        }
    }

    public ApiResponse<ProductDto> Create(string username, CreateProductDto dto)
    {
        try
        {
            Product product = new()
            {
                Title = dto.Title,
                Slug = dto.Slug,
                Description = dto.Description,
                Brand = dto.Brand,
                IsActive = true
            };

            dbContext.Products.Add(product);

            logger.Information(
                "Successfully prepared product '{ProductTitle}' for creation. User: '{Username}'",
                product.Title, username);

            return new ApiResponse<ProductDto>(Data: product.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while preparing product '{ProductTitle}'. User: '{Username}'",
                dto.Title, username);

            return new ApiResponse<ProductDto>(Error: "An unexpected error occurred while processing your request. Please try again later.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products
                .Include(x => x.Variants)
                    .ThenInclude(x => x.Media)
                .Include(x => x.Variants)
                    .ThenInclude(x => x.VariantAttributes)
                .Include(x => x.Category)
                    .ThenInclude(x => x.SubCategories)
                .Include(x => x.Media)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (product is null)
            {
                logger.Warning(
                    "Delete aborted: Product '{ProductId}' was not found. User: '{Username}'.",
                    id, username);

                return new ApiResponse<ProductDto>(Error: "The requested product could not be found. It may have already been removed.", Code: ErrorCodes.NotFound);
            }

            dbContext.Entry(product).Property(p => p.Version).OriginalValue = version;
            dbContext.Products.Remove(product);

            logger.Information(
                "Successfully prepared product '{ProductId}' for deletion. User: '{Username}'.",
                id, username);

            return new ApiResponse<ProductDto>(Data: product.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to delete product '{ProductId}'. User: '{Username}'.",
                id, username);

            return new ApiResponse<ProductDto>(Error: "An unexpected error occurred while processing the deletion. Please try again later.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<ProductDto> products = await dbContext.Products.Select(x => x.ToDto()).ToListAsync(token);

            logger.Debug(
                "Retrieved {Count} products. User: '{Username}'",
                products.Count, username);

            return new ApiResponse<IEnumerable<ProductDto>>(Data: products);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving all products. User: '{Username}'",
                username);

            return new ApiResponse<IEnumerable<ProductDto>>(Error: "An unexpected error occurred while retrieving the data.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products
                .Include(x => x.Variants)
                    .ThenInclude(x => x.Media)
                .Include(x => x.Variants)
                    .ThenInclude(x => x.VariantAttributes)
                .Include(x => x.Category)
                    .ThenInclude(x => x.SubCategories)
                .Include(x => x.Media)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (product is null)
            {
                logger.Warning(
                    "Read aborted: Product '{ProductId}' was not found. User: '{Username}'.",
                    id, username);

                return new ApiResponse<ProductDto>(Error: "The requested product could not be found.", Code: ErrorCodes.NotFound);
            }

            logger.Debug(
                "Retrieved product '{ProductId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<ProductDto>(Data: product.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving product '{ProductId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<ProductDto>(Error: "An unexpected error occurred while retrieving the data.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products.FindAsync(keyValues: [id], cancellationToken: token);

            if (product is null)
            {
                logger.Warning(
                    "Update aborted: Product '{ProductId}' was not found. User: '{Username}'.",
                    id, username);

                return new ApiResponse<ProductDto>(Error: "The requested product could not be found.", Code: ErrorCodes.NotFound);
            }

            dbContext.Entry(product).Property(p => p.Version).OriginalValue = version;

            product.Title = dto.Title;
            product.Slug = dto.Slug;
            product.Description = dto.Description;
            product.Brand = dto.Brand;
            product.IsActive = dto.IsActive;
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;
            product.Version = Guid.NewGuid();

            logger.Information(
                "Successfully prepared product '{ProductId}' for update. User: '{Username}'.",
                id, username);

            return new ApiResponse<ProductDto>(Data: product.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to update product '{ProductId}'. User: '{Username}'.",
                id, username);

            return new ApiResponse<ProductDto>(Error: "An unexpected error occurred while processing the update. Please try again later.", Code: ErrorCodes.Generic);
        }
    }
}