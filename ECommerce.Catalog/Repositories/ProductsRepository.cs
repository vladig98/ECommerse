namespace ECommerce.Catalog.Repositories;

public class ProductsRepository(MainDbContext dbContext, ILogger logger) : IProductsRepository
{
    public async Task<ApiResponse<Product>> CreateAsync(string username, CreateProductDto dto, CancellationToken token)
    {
        try
        {
            Product product = dto.ToModel();
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(token);

            logger.Information("Successfully created product '{ProductTitle}'. User: '{Username}'", product.Title, username);

            return ApiResponse<Product>.Success(product);
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving product '{ProductTitle}'. User: '{Username}'", dto.Title, username);
            return ApiResponse<Product>.Failure("A database constraint was violated (e.g., duplicate attribute or SKU). Please check your data and try again.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing product '{ProductTitle}' to the database. User: '{Username}'", dto.Title, username);
            return ApiResponse<Product>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ApiResponse<Product>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        string title = "N/A";

        try
        {
            Product? product = await dbContext.Products
                .GetAllRelatedEntities()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (product is null)
            {
                logger.Warning("Delete aborted: Product '{ProductId}' was not found. User: '{Username}'.", id, username);
                return ApiResponse<Product>.NotFound("The requested product could not be found. It may have already been removed.");
            }

            title = product.Title;

            dbContext.Entry(product).Property(p => p.Version).OriginalValue = version;
            dbContext.Products.Remove(product);

            await dbContext.SaveChangesAsync(token);

            logger.Information("Successfully prepared product '{ProductId}' for deletion. User: '{Username}'.", id, username);

            return ApiResponse<Product>.Success(product);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency error while deleting product {ProductTitle} (ID: {ProductId}). User: '{Username}'", title, id, username);
            return ApiResponse<Product>.Conflict("The product has been modified by another process. Please refresh and try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while deleting product {ProductTitle} (ID: {ProductId}). User: '{Username}'", title, id, username);
            return ApiResponse<Product>.Failure("This product cannot be deleted because it is currently referenced by other records in the system.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing deletion of product {ProductTitle} (ID: {ProductId}) to the database. User: '{Username}'", title, id, username);
            return ApiResponse<Product>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }
    }

    public async Task<ApiResponse<List<Product>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<Product> products = await dbContext.Products.ToListAsync(token);

            logger.Debug("Retrieved {Count} products. User: '{Username}'", products.Count, username);

            return ApiResponse<List<Product>>.Success(products);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Database failure while retrieving all products. User: '{Username}'", username);
            return ApiResponse<List<Product>>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<Product>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products
                .GetAllRelatedEntities()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (product is null)
            {
                logger.Warning("Read aborted: Product '{ProductId}' was not found. User: '{Username}'.", id, username);
                return ApiResponse<Product>.NotFound("The requested product could not be found.");
            }

            logger.Debug("Retrieved product '{ProductId}'. User: '{Username}'", id, username);

            return ApiResponse<Product>.Success(product);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Database failure while retrieving product '{ProductId}'. User: '{Username}'", id, username);
            return ApiResponse<Product>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<Product>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token)
    {
        try
        {
            Product? product = await dbContext.Products
                .GetAllRelatedEntities()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (product is null)
            {
                logger.Warning("Update aborted: Product '{ProductId}' was not found. User: '{Username}'.", id, username);
                return ApiResponse<Product>.NotFound("The requested product could not be found.");
            }

            dbContext.Entry(product).Property(p => p.Version).OriginalValue = version;
            product.Update(dto);

            await dbContext.SaveChangesAsync(token);

            logger.Information("Successfully prepared product '{ProductId}' for update. User: '{Username}'.", id, username);

            return ApiResponse<Product>.Success(product);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency update error while saving product '{ProductTitle}'. User: '{Username}'", dto.Title, username);
            return ApiResponse<Product>.Conflict("The entity has been modified by another process. Please try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving product '{ProductTitle}'. User: '{Username}'", dto.Title, username);
            return ApiResponse<Product>.Failure("A database constraint was violated. Please check your data and try again.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing product '{ProductTitle}' to the database. User: '{Username}'", dto.Title, username);
            return ApiResponse<Product>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }
}