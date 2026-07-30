namespace ECommerce.Catalog.Services;

public class CategoryService(
    ICategoryRepository categoryRepository,
    MainDbContext dbContext,
    ILogger logger) : ICategoryService
{
    public async Task<ApiResponse<CategoryDto>> CreateAsync(string username, CreateCategoryDto dto, CancellationToken token)
    {
        ApiResponse<Category> categoryResponse = categoryRepository.Create(username, dto);
        if (!string.IsNullOrWhiteSpace(categoryResponse.Error))
        {
            logger.Warning("Aborting category creation: Failed to prepare category '{CategoryName}'. User: '{Username}'", dto.Name, username);
            return ApiResponse<CategoryDto>.FromResponse(categoryResponse);
        }

        Category category = categoryResponse.Data!;

        if (dto.ParentCategoryId.HasValue)
        {
            ApiResponse<Category> parentResponse = await categoryRepository.GetAsync(username, dto.ParentCategoryId.Value, token);
            if (!string.IsNullOrWhiteSpace(parentResponse.Error))
            {
                logger.Warning("Aborting category creation: Parent category '{ParentCategoryId}' not found. User: '{Username}'", dto.ParentCategoryId.Value, username);
                return ApiResponse<CategoryDto>.FromResponse(parentResponse);
            }

            Category parent = parentResponse.Data!;
            category.ParentCategory = parent;
        }

        try
        {
            await dbContext.SaveChangesAsync(token);
            logger.Information("Successfully committed category '{CategoryName}' (ID: {CategoryId}). User: '{Username}'", category.Name, category.Id, username);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency update error while saving category '{CategoryName}'. User: '{Username}'", dto.Name, username);
            return ApiResponse<CategoryDto>.Conflict("The category has been modified by another process. Please try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving category '{CategoryName}'. User: '{Username}'", dto.Name, username);
            return ApiResponse<CategoryDto>.Failure("A database constraint was violated (e.g., duplicate category name or slug). Please check your data and try again.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing category '{CategoryName}' to the database. User: '{Username}'", dto.Name, username);
            return ApiResponse<CategoryDto>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }

        return ApiResponse<CategoryDto>.Success(category.ToDto()!);
    }

    public async Task<ApiResponse<CategoryDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        ApiResponse<Category> categoryResponse = await categoryRepository.DeleteAsync(username, id, version, token);
        if (!string.IsNullOrWhiteSpace(categoryResponse.Error))
        {
            logger.Warning("Aborting category deletion: Failed to prepare category '{CategoryId}' for deletion. User: '{Username}'. Reason: {Error}", id, username, categoryResponse.Error);
            return ApiResponse<CategoryDto>.FromResponse(categoryResponse);
        }

        Category category = categoryResponse.Data!;

        try
        {
            await dbContext.SaveChangesAsync(token);
            logger.Information("Successfully committed deletion for category '{CategoryName}' (ID: {CategoryId}). User: '{Username}'", category.Name, category.Id, username);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency error while deleting category '{CategoryName}' (ID: {CategoryId}). User: '{Username}'", category.Name, id, username);
            return ApiResponse<CategoryDto>.Conflict("The category has been modified by another process. Please refresh and try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while deleting category '{CategoryName}' (ID: {CategoryId}). User: '{Username}'", category.Name, id, username);
            return ApiResponse<CategoryDto>.Failure("This category cannot be deleted because it is currently referenced by other records in the system.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing deletion of category '{CategoryName}' (ID: {CategoryId}) to the database. User: '{Username}'", category.Name, id, username);
            return ApiResponse<CategoryDto>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }

        return ApiResponse<CategoryDto>.Success(category.ToDto()!);
    }

    public async Task<ApiResponse<List<CategoryDto>>> GetAllAsync(string username, CancellationToken token)
    {
        ApiResponse<List<Category>> categoryResponse = await categoryRepository.GetAllAsync(username, token);
        if (!string.IsNullOrWhiteSpace(categoryResponse.Error))
        {
            logger.Warning("Failed to retrieve categories. User: '{Username}'. Reason: {Error}", username, categoryResponse.Error);
            return ApiResponse<List<CategoryDto>>.FromResponse(categoryResponse);
        }

        List<Category> categories = categoryResponse.Data!;
        return ApiResponse<List<CategoryDto>>.Success([.. categories.Select(x => x.ToDto()!)]);
    }

    public async Task<ApiResponse<CategoryDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        ApiResponse<Category> categoryResponse = await categoryRepository.GetAsync(username, id, token);
        if (!string.IsNullOrWhiteSpace(categoryResponse.Error))
        {
            logger.Warning("Failed to retrieve category '{CategoryId}'. User: '{Username}'. Reason: {Error}", id, username, categoryResponse.Error);
            return ApiResponse<CategoryDto>.FromResponse(categoryResponse);
        }

        Category category = categoryResponse.Data!;
        return ApiResponse<CategoryDto>.Success(category.ToDto()!);
    }

    public async Task<ApiResponse<CategoryDto>> UpdateAsync(string username, Guid id, Guid version, UpdateCategoryDto dto, CancellationToken token)
    {
        ApiResponse<Category> categoryResponse = await categoryRepository.UpdateAsync(username, id, version, dto, token);
        if (!string.IsNullOrWhiteSpace(categoryResponse.Error))
        {
            logger.Warning("Aborting category update: Failed to update root category '{CategoryId}'. User: '{Username}'", id, username);
            return ApiResponse<CategoryDto>.FromResponse(categoryResponse);
        }

        Category category = categoryResponse.Data!;

        if (dto.ParentCategoryId.HasValue)
        {
            ApiResponse<Category> parentCategoryResponse = await categoryRepository.GetAsync(username, dto.ParentCategoryId.Value, token);
            if (!string.IsNullOrWhiteSpace(parentCategoryResponse.Error))
            {
                logger.Warning("Aborting category update: Category '{CategoryId}' not found. User: '{Username}'", dto.ParentCategoryId.Value, username);
                return ApiResponse<CategoryDto>.FromResponse(parentCategoryResponse);
            }

            Category parentCategory = parentCategoryResponse.Data!;
            category.ParentCategory = parentCategory;
        }
        else
        {
            category.ParentCategoryId = null;
            category.ParentCategory = null;
        }

        try
        {
            await dbContext.SaveChangesAsync(token);
            logger.Information("Successfully committed update for category graph '{CategoryName}' (ID: {CategoryId}). User: '{Username}'", category.Name, category.Id, username);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency update error while saving category '{CategoryName}'. User: '{Username}'", category.Name, username);
            return ApiResponse<CategoryDto>.Conflict("The entity has been modified by another process. Please try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving category '{CategoryName}'. User: '{Username}'", category.Name, username);
            return ApiResponse<CategoryDto>.Failure("A database constraint was violated. Please check your data and try again.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing category '{CategoryName}' to the database. User: '{Username}'", category.Name, username);
            return ApiResponse<CategoryDto>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }

        return ApiResponse<CategoryDto>.Success(category.ToDto()!);
    }
}
