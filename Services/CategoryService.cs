namespace ECommerce.Catalog.Services;

public class CategoryService(MainDbContext dbContext, ILogger logger) : ICategoryService
{
    public ApiResponse<CategoryDto> Create(string username, CreateCategoryDto dto)
    {
        try
        {
            Category category = new()
            {
                Name = dto.Name,
                ParentCategoryId = dto.ParentCategoryId,
                Slug = dto.Slug
            };

            dbContext.Categories.Add(category);

            logger.Information(
                "Successfully prepared category '{CategoryName}' for creation. User: '{Username}'",
                category.Name, username);

            return new ApiResponse<CategoryDto>(Data: category.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Memory/Tracker failure while preparing category '{CategoryName}'. User: '{Username}'",
                dto.Name, username);

            return new ApiResponse<CategoryDto>(Error: "An unexpected error occurred while processing your request. Please try again later.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<CategoryDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
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

                return new ApiResponse<CategoryDto>(Error: "The requested category could not be found. It may have already been removed.", Code: ErrorCodes.NotFound);
            }

            dbContext.Entry(category).Property(p => p.Version).OriginalValue = version;
            dbContext.Categories.Remove(category);

            logger.Information(
                "Successfully prepared category '{CategoryId}' for deletion. User: '{Username}'.",
                id, username);

            return new ApiResponse<CategoryDto>(Data: category.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to delete category '{CategoryId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<CategoryDto>(Error: "An unexpected error occurred while processing the deletion. Please try again later.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            List<CategoryDto> categories = await dbContext.Categories.Select(x => x.ToDto()).ToListAsync(token);

            logger.Debug(
                "Retrieved {Count} categories. User: '{Username}'",
                categories.Count, username);

            return new ApiResponse<IEnumerable<CategoryDto>>(Data: categories);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving all categories. User: '{Username}'",
                username);

            return new ApiResponse<IEnumerable<CategoryDto>>(Error: "An unexpected error occurred while retrieving the data.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<CategoryDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            Category? category = await dbContext.Categories
                .Include(x => x.SubCategories)
                .FirstOrDefaultAsync(x => x.Id == id, token);

            if (category is null)
            {
                logger.Warning(
                    "Read aborted: Category '{CategoryId}' was not found. User: '{Username}'.",
                    id, username);

                return new ApiResponse<CategoryDto>(Error: "The requested category could not be found.", Code: ErrorCodes.NotFound);
            }

            logger.Debug(
                "Retrieved category '{CategoryId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<CategoryDto>(Data: category.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Database failure while retrieving category '{CategoryId}'. User: '{Username}'",
                id, username);

            return new ApiResponse<CategoryDto>(Error: "An unexpected error occurred while retrieving the data.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<CategoryDto>> UpdateAsync(string username, Guid id, Guid version, UpdateCategoryDto dto, CancellationToken token)
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

                return new ApiResponse<CategoryDto>(Error: "The requested category could not be found.", Code: ErrorCodes.NotFound);
            }

            dbContext.Entry(category).Property(p => p.Version).OriginalValue = version;

            category.Name = dto.Name;
            category.Slug = dto.Slug;
            category.ParentCategoryId = dto.ParentCategoryId;
            category.UpdatedAt = DateTime.UtcNow;
            category.Version = Guid.NewGuid();

            logger.Information(
                "Successfully prepared category '{CategoryId}' for update. User: '{Username}'.",
                id, username);

            return new ApiResponse<CategoryDto>(Data: category.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Failure while preparing to update category '{CategoryId}'. User: '{Username}'.",
                id, username);

            return new ApiResponse<CategoryDto>(Error: "An unexpected error occurred while processing the update. Please try again later.", Code: ErrorCodes.Generic);
        }
    }
}