namespace ECommerse.Catalog.Services;

public class CategoryService(MainDbContext dbContext, ILogger logger) : ICategoryService
{
    public async Task<ApiResponse<CategoryDto>> CreateAsync(string username, CreateCategoryDto dto, CancellationToken token)
    {
        Category category = new()
        {
            Name = dto.Name,
            ParentId = dto.ParentId,
            Slug = dto.Slug,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid()
        };

        try
        {
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync(token);

            logger.Information("Added a new category 'name = {Name}'. User '{Username}'", dto.Name, username);

            return new ApiResponse<CategoryDto>(Data: category.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to add category 'name = {Name}'. User '{Username}'", dto.Name, username);
            return new ApiResponse<CategoryDto>(Error: $"Failed to add category with name {dto.Name}.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<CategoryDto>> DeleteAsync(string username, string id, Guid version, CancellationToken token)
    {
        Category? category = await dbContext.Categories.FindAsync(keyValues: [id], cancellationToken: token);

        if (category is null)
        {
            logger.Warning("Missing category 'id = {Id}' on delete. User '{Username}'.", id, username);
            return new ApiResponse<CategoryDto>(Error: $"No category with id {id} was found", Code: ErrorCodes.NotFound);
        }

        dbContext.Entry(category).Property(p => p.Version).OriginalValue = version;
        dbContext.Categories.Remove(category);

        try
        {
            await dbContext.SaveChangesAsync(token);

            logger.Information("Category 'id = {Id}' was deleted successfully. User '{Username}'.", id, username);
            return new ApiResponse<CategoryDto>(Data: category.ToDto());
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.Warning(ex, "Category 'id = {Id}' couldn't be deleted due to concurrency conflict. User '{Username}'.", id, username);
            return new ApiResponse<CategoryDto>(Error: "This category was modified by another request. Please reload and try again.", Code: ErrorCodes.Conflict);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Category 'id = {Id}' couldn't be deleted. User '{Username}'.", id, username);
            return new ApiResponse<CategoryDto>(Error: "This category couldn't be deleted. Please reload and try again.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllAsync(string username, CancellationToken token)
    {
        try
        {
            IEnumerable<CategoryDto> categories = await dbContext.Categories.Select(x => x.ToDto()).ToListAsync(token);
            logger.Information("Retrieved all categories. User '{Username}'", username);

            return new ApiResponse<IEnumerable<CategoryDto>>(Data: categories);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to get all categories. User '{Username}'", username);
            return new ApiResponse<IEnumerable<CategoryDto>>(Error: "Failed to get all categories.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<CategoryDto>> GetAsync(string username, string id, CancellationToken token)
    {
        try
        {
            Category? category = await dbContext.Categories.FindAsync(keyValues: [id], cancellationToken: token);
            if (category is null)
            {
                logger.Warning("Missing category 'id = {Id}' on get. User '{Username}'.", id, username);
                return new ApiResponse<CategoryDto>(Error: $"No category with id {id} was found", Code: ErrorCodes.NotFound);
            }

            logger.Information("Retrieved category 'id = {Id}'. User '{Username}'", id, username);

            return new ApiResponse<CategoryDto>(Data: category.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to get category 'id = {Id}'. User '{Username}'", id, username);
            return new ApiResponse<CategoryDto>(Error: $"Failed to get category with id = {id}.", Code: ErrorCodes.Generic);
        }
    }

    public async Task<ApiResponse<CategoryDto>> UpdateAsync(string username, string id, Guid version, UpdateCategoryDto dto, CancellationToken token)
    {
        Category? category = await dbContext.Categories.FindAsync(keyValues: [id], cancellationToken: token);

        if (category is null)
        {
            logger.Warning("Missing category 'id = {Id}' on update. User '{Username}'.", id, username);
            return new ApiResponse<CategoryDto>(Error: $"No category with id {id} was found", Code: ErrorCodes.NotFound);
        }

        dbContext.Entry(category).Property(p => p.Version).OriginalValue = version;

        category.Name = dto.Name;
        category.Slug = dto.Slug;
        category.ParentId = dto.ParentId;
        category.UpdatedAt = DateTime.UtcNow;

        category.Version = Guid.NewGuid();

        try
        {
            await dbContext.SaveChangesAsync(token);

            logger.Information("Category 'id = {Id}' was updated successfully. User '{Username}'.", id, username);
            return new ApiResponse<CategoryDto>(Data: category.ToDto());
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.Warning(ex, "Category 'id = {Id}' couldn't be updated due to concurrency conflict. User '{Username}'.", id, username);
            return new ApiResponse<CategoryDto>(Error: "This category was modified by another request. Please reload and try again.", Code: ErrorCodes.Conflict);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Category 'id = {Id}' couldn't be updated. User '{Username}'.", id, username);
            return new ApiResponse<CategoryDto>(Error: "This category couldn't be updated. Please reload and try again.", Code: ErrorCodes.Generic);
        }
    }
}
