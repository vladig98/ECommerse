namespace ECommerse.Catalog.Services;

public class ProductsService(MainDbContext dbContext, ILogger logger) : IProductsService
{
    public async Task<ApiResponse<ProductDto>> CreateAsync(string username, CreateProductDto dto, CancellationToken token)
    {
        Product product = new()
        {
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow,
            Description = dto.Description,
            IsActive = true,
            Price = dto.Price,
            Sku = dto.Sku,
            StockQuantity = dto.Quantity,
            Title = dto.Title,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid()
        };

        try
        {
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(token);

            logger.Information("Added a new product 'name = {Name}'. User '{Username}'", dto.Title, username);

            return new ApiResponse<ProductDto>(Data: product.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to add product 'name = {Name}'. User '{Username}'", dto.Title, username);
            return new ApiResponse<ProductDto>(Error: $"Failed to add product with title {dto.Title}.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductDto>> DeleteAsync(string username, string id, Guid version, CancellationToken token)
    {
        Product? product = await dbContext.Products.FindAsync(keyValues: [id], cancellationToken: token);

        if (product is null)
        {
            logger.Warning("Missing product 'id = {Id}' on delete. User '{Username}'.", id, username);
            return new ApiResponse<ProductDto>(Error: $"No product with id {id} was found", Code: ErrorCodes.NotFound);
        }

        dbContext.Entry(product).Property(p => p.Version).OriginalValue = version;
        dbContext.Products.Remove(product);

        try
        {
            await dbContext.SaveChangesAsync(token);

            logger.Information("Product 'id = {Id}' was deleted successfully. User '{Username}'.", id, username);
            return new ApiResponse<ProductDto>(Data: product.ToDto());
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.Warning(ex, "Product 'id = {Id}' couldn't be deleted due to concurrency conflict. User '{Username}'.", id, username);
            return new ApiResponse<ProductDto>(Error: "This product was modified by another request. Please reload and try again.", Code: ErrorCodes.Conflict);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Product 'id = {Id}' couldn't be deleted. User '{Username}'.", id, username);
            return new ApiResponse<ProductDto>(Error: "This product couldn't be deleted. Please reload and try again.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            IEnumerable<ProductDto> products = await dbContext.Products.Select(x => x.ToDto()).ToListAsync(token);
            logger.Information("Retrieved all products. User '{Username}'", username);

            return new ApiResponse<IEnumerable<ProductDto>>(Data: products);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to get all products. User '{Username}'", username);
            return new ApiResponse<IEnumerable<ProductDto>>(Error: "Failed to get all products.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductDto>> GetAsync(string username, string id, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products.FindAsync(keyValues: [id], cancellationToken: token);
            if (product is null)
            {
                logger.Warning("Missing product 'id = {Id}' on get. User '{Username}'.", id, username);
                return new ApiResponse<ProductDto>(Error: $"No product with id {id} was found", Code: ErrorCodes.NotFound);
            }

            logger.Information("Retrieved product 'id = {Id}'. User '{Username}'", id, username);

            return new ApiResponse<ProductDto>(Data: product.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to get product 'id = {Id}'. User '{Username}'", id, username);
            return new ApiResponse<ProductDto>(Error: $"Failed to get product with id = {id}.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(string username, string id, Guid version, UpdateProductDto dto, CancellationToken token)
    {
        Product? product = await dbContext.Products.FindAsync(keyValues: [id], cancellationToken: token);

        if (product is null)
        {
            logger.Warning("Missing product 'id = {Id}' on update. User '{Username}'.", id, username);
            return new ApiResponse<ProductDto>(Error: $"No product with id {id} was found", Code: ErrorCodes.NotFound);
        }

        dbContext.Entry(product).Property(p => p.Version).OriginalValue = version;

        product.Title = dto.Title;
        product.Sku = dto.Sku;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.Quantity;
        product.IsActive = dto.IsActive;
        product.CategoryId = dto.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        product.Version = Guid.NewGuid();

        try
        {
            await dbContext.SaveChangesAsync(token);

            logger.Information("Product 'id = {Id}' was updated successfully. User '{Username}'.", id, username);
            return new ApiResponse<ProductDto>(Data: product.ToDto());
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.Warning(ex, "Product 'id = {Id}' couldn't be updated due to concurrency conflict. User '{Username}'.", id, username);
            return new ApiResponse<ProductDto>(Error: "This product was modified by another request. Please reload and try again.", Code: ErrorCodes.Conflict);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Product 'id = {Id}' couldn't be updated. User '{Username}'.", id, username);
            return new ApiResponse<ProductDto>(Error: "This product couldn't be updated. Please reload and try again.", Code: ErrorCodes.Generic);
        }
    }
}
