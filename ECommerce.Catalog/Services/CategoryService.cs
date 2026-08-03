namespace ECommerce.Catalog.Services;

public class CategoryService(ICategoryRepository categoryRepository, ILogger logger) : ICategoryService
{
    public async Task<ApiResponse<CategoryDto>> CreateAsync(string username, CreateCategoryDto dto, CancellationToken token)
    {
        try
        {
            Category category = dto.ToModel();
            Category createdCategory = await categoryRepository.AddAsync(category, token);
            CategoryDto categoryDto = createdCategory.ToDto()!;

            logger.Information("Successfully created category '{CategoryName}' (ID: {CategoryId}). User: '{Username}'", createdCategory.Name, createdCategory.Id, username);

            return ApiResponse<CategoryDto>.Success(categoryDto);
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
    }

    public async Task<ApiResponse<CategoryDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            Category? category = await categoryRepository.DeleteAsync(id, version, token);
            if (category is null)
            {
                logger.Warning("Delete aborted: Category '{CategoryId}' was not found. User: '{Username}'.", id, username);
                return ApiResponse<CategoryDto>.NotFound("The requested category could not be found. It may have already been removed.");
            }

            CategoryDto deletedDto = category.ToDto()!;

            logger.Information("Successfully deleted category '{CategoryName}' (ID: {CategoryId}). User: '{Username}'", category.Name, id, username);

            return ApiResponse<CategoryDto>.Success(deletedDto);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency error while deleting category '{CategoryId}'. User: '{Username}'", id, username);
            return ApiResponse<CategoryDto>.Conflict("The category has been modified by another process. Please refresh and try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while deleting category '{CategoryId}'. User: '{Username}'", id, username);
            return ApiResponse<CategoryDto>.Failure("This category cannot be deleted because it is currently referenced by other records in the system.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing deletion of category '{CategoryId}' to the database. User: '{Username}'", id, username);
            return ApiResponse<CategoryDto>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }
    }

    public async Task<ApiResponse<PagedResult<CategoryDto>>> GetAllAsync(string username, int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        try
        {
            PagedResult<Category> pagedModels = await categoryRepository.GetAllAsync(pageNumber, itemsPerPage, token);

            List<CategoryDto> dtos = [.. pagedModels.Items.Select(x => x.ToDto()!)];

            PagedResult<CategoryDto> pagedDtos = new(
                dtos,
                pagedModels.TotalCount,
                pagedModels.PageNumber,
                pagedModels.ItemsPerPage,
                pagedModels.TotalPages
            );

            logger.Debug("User '{Username}' retrieved page {Page} of categories. Total count: {TotalCount}", username, pageNumber, pagedModels.TotalCount);

            return ApiResponse<PagedResult<CategoryDto>>.Success(pagedDtos);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Database failure while retrieving all categories. User: '{Username}'", username);
            return ApiResponse<PagedResult<CategoryDto>>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<CategoryDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            Category? category = await categoryRepository.GetAsync(id, token);
            if (category is null)
            {
                logger.Warning("Read aborted: Category '{CategoryId}' was not found. User: '{Username}'", id, username);
                return ApiResponse<CategoryDto>.NotFound($"The requested category with ID '{id}' could not be found.");
            }

            logger.Debug("Retrieved category '{CategoryId}'. User: '{Username}'", id, username);

            return ApiResponse<CategoryDto>.Success(category.ToDto()!);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Database failure while retrieving category '{CategoryId}'. User: '{Username}'", id, username);
            return ApiResponse<CategoryDto>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<CategoryDto>> UpdateAsync(string username, Guid id, Guid version, UpdateCategoryDto dto, CancellationToken token)
    {
        try
        {
            Category? category = await categoryRepository.GetAsync(id, token);
            if (category is null)
            {
                logger.Warning("Update aborted: Category '{CategoryId}' was not found. User: '{Username}'.", id, username);
                return ApiResponse<CategoryDto>.NotFound("The requested category could not be found.");
            }

            category.Update(dto);
            await categoryRepository.UpdateAsync(category, version, token);

            CategoryDto categoryDto = category.ToDto()!;

            logger.Information("Successfully updated category '{CategoryName}' (ID: {CategoryId}). User: '{Username}'.", category.Name, id, username);

            return ApiResponse<CategoryDto>.Success(categoryDto);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency update error while saving category '{CategoryId}'. User: '{Username}'", id, username);
            return ApiResponse<CategoryDto>.Conflict("The entity has been modified by another process. Please try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving category '{CategoryId}'. User: '{Username}'", id, username);
            return ApiResponse<CategoryDto>.Failure("A database constraint was violated. Please check your data and try again.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing category '{CategoryId}' to the database. User: '{Username}'", id, username);
            return ApiResponse<CategoryDto>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }
}