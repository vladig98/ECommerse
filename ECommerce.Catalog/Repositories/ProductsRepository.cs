namespace ECommerce.Catalog.Repositories;

public class ProductsRepository(MainDbContext dbContext, ILogger logger) : IProductsRepository
{
    public ApiResponse<Product> Create(string username, CreateProductDto dto)
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

            return ApiResponse<Product>.Success(product);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while preparing product '{ProductTitle}'. User: '{Username}'",
                dto.Title, username);

            return ApiResponse<Product>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ApiResponse<Product>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products
                .Include(x => x.Variants)
                    .ThenInclude(x => x.Media)
                .Include(x => x.Variants)
                    .ThenInclude(x => x.VariantAttributes)
                        .ThenInclude(x => x.Attribute)
                .Include(x => x.Category)
                    .ThenInclude(x => x.SubCategories)
                .Include(x => x.Media)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (product is null)
            {
                logger.Warning(
                    "Delete aborted: Product '{ProductId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<Product>.NotFound("The requested product could not be found. It may have already been removed.");
            }

            dbContext.Entry(product).Property(p => p.Version).OriginalValue = version;
            dbContext.Products.Remove(product);

            logger.Information(
                "Successfully prepared product '{ProductId}' for deletion. User: '{Username}'.",
                id, username);

            return ApiResponse<Product>.Success(product);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to delete product '{ProductId}'. User: '{Username}'.",
                id, username);

            return ApiResponse<Product>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }
    }

    public async Task<ApiResponse<List<Product>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<Product> products = await dbContext.Products.ToListAsync(token);

            logger.Debug(
                "Retrieved {Count} products. User: '{Username}'",
                products.Count, username);

            return ApiResponse<List<Product>>.Success(products);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving all products. User: '{Username}'",
                username);

            return ApiResponse<List<Product>>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<Product>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products
                .Include(x => x.Variants)
                    .ThenInclude(x => x.Media)
                .Include(x => x.Variants)
                    .ThenInclude(x => x.VariantAttributes)
                .Include(x => x.Category)
                    .ThenInclude(x => x.ParentCategory)
                .Include(x => x.Media)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (product is null)
            {
                logger.Warning(
                    "Read aborted: Product '{ProductId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<Product>.NotFound("The requested product could not be found.");
            }

            logger.Debug(
                "Retrieved product '{ProductId}'. User: '{Username}'",
                id, username);

            return ApiResponse<Product>.Success(product);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving product '{ProductId}'. User: '{Username}'",
                id, username);

            return ApiResponse<Product>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<Product>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products
                .Include(x => x.Variants)
                    .ThenInclude(x => x.Media)
                .Include(x => x.Variants)
                    .ThenInclude(x => x.VariantAttributes)
                        .ThenInclude(x => x.Attribute)
                .Include(x => x.Category)
                    .ThenInclude(x => x.ParentCategory)
                .Include(x => x.Media)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (product is null)
            {
                logger.Warning(
                    "Update aborted: Product '{ProductId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<Product>.NotFound("The requested product could not be found.");
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

            return ApiResponse<Product>.Success(product);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to update product '{ProductId}'. User: '{Username}'.",
                id, username);

            return ApiResponse<Product>.Failure("An unexpected error occurred while processing the update. Please try again later.");
        }
    }
}