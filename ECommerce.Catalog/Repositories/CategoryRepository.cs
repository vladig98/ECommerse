namespace ECommerce.Catalog.Repositories;

public class CategoryRepository(MainDbContext dbContext, ILogger logger) : ICategoryRepository
{
    public ApiResponse<Category> Create(string username, CreateCategoryDto dto)
    {
        try
        {
            Category category = new()
            {
                Name = dto.Name,
                Slug = dto.Slug
            };

            dbContext.Categories.Add(category);

            logger.Information(
                "Successfully prepared category '{CategoryName}' for creation. User: '{Username}'",
                category.Name, username);

            return ApiResponse<Category>.Success(category);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while preparing category '{CategoryName}'. User: '{Username}'",
                dto.Name, username);

            return ApiResponse<Category>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ApiResponse<Category>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            Category? category = await dbContext.Categories
                .Include(x => x.SubCategories)
                .FirstOrDefaultAsync(x => x.Id == id, token);

            if (category is null)
            {
                logger.Warning(
                    "Delete aborted: Category '{CategoryId}' was not found. User: '{Username}'",
                    id, username);

                return ApiResponse<Category>.NotFound("The requested category could not be found. It may have already been removed.");
            }

            dbContext.Entry(category).Property(p => p.Version).OriginalValue = version;
            dbContext.Categories.Remove(category);

            logger.Information(
                "Successfully prepared category '{CategoryId}' for deletion. User: '{Username}'.",
                id, username);

            return ApiResponse<Category>.Success(category);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to delete category '{CategoryId}'. User: '{Username}'",
                id, username);

            return ApiResponse<Category>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }
    }

    public async Task<ApiResponse<List<Category>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<Category> categories = await dbContext.Categories.ToListAsync(token);

            logger.Debug(
                "Retrieved {Count} categories. User: '{Username}'",
                categories.Count, username);

            return ApiResponse<List<Category>>.Success(categories);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving all categories. User: '{Username}'",
                username);

            return ApiResponse<List<Category>>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<Category>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            Category? category = await dbContext.Categories
                .Include(x => x.ParentCategory)
                .Include(x => x.SubCategories)
                .FirstOrDefaultAsync(x => x.Id == id, token);

            if (category is null)
            {
                logger.Warning(
                    "Read aborted: Category '{CategoryId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<Category>.NotFound("The requested category could not be found.");
            }

            logger.Debug(
                "Retrieved category '{CategoryId}'. User: '{Username}'",
                id, username);

            return ApiResponse<Category>.Success(category);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving category '{CategoryId}'. User: '{Username}'",
                id, username);

            return ApiResponse<Category>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<Category>> UpdateAsync(string username, Guid id, Guid version, UpdateCategoryDto dto, CancellationToken token)
    {
        try
        {
            Category? category = await dbContext.Categories
                .Include(x => x.SubCategories)
                .FirstOrDefaultAsync(x => x.Id == id, token);

            if (category is null)
            {
                logger.Warning(
                    "Update aborted: Category '{CategoryId}' was not found. User: '{Username}'.",
                    id, username);

                return ApiResponse<Category>.NotFound("The requested category could not be found.");
            }

            dbContext.Entry(category).Property(p => p.Version).OriginalValue = version;

            category.Name = dto.Name;
            category.Slug = dto.Slug;
            category.UpdatedAt = DateTime.UtcNow;
            category.Version = Guid.NewGuid();

            logger.Information(
                "Successfully prepared category '{CategoryId}' for update. User: '{Username}'.",
                id, username);

            return ApiResponse<Category>.Success(category);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to update category '{CategoryId}'. User: '{Username}'.",
                id, username);

            return ApiResponse<Category>.Failure("An unexpected error occurred while processing the update. Please try again later.");
        }
    }
}