namespace ECommerce.Catalog.Services;

internal class ProductService(IProductsRepository productRepository, ILogger logger) : IProductService
{
    public async Task<ApiResponse<ProductDto>> CreateAsync(string username, CreateProductDto dto, CancellationToken token)
    {
        try
        {
            Product product = dto.ToModel();

            Product createdProduct = await productRepository.AddAsync(product, token).ConfigureAwait(true);
            ProductDto productDto = createdProduct.ToDto();

            logger.Information("Successfully created product '{ProductTitle}'. User: '{Username}'", product.Title, username);

            return ApiResponse<ProductDto>.Success(productDto);
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving product '{ProductTitle}'. User: '{Username}'", dto.Title, username);
            return ApiResponse<ProductDto>.Failure("A database constraint was violated (e.g., duplicate attribute or SKU). Please check your data and try again.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing product '{ProductTitle}' to the database. User: '{Username}'", dto.Title, username);
            return ApiResponse<ProductDto>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ApiResponse<ProductDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            Product? product = await productRepository.DeleteAsync(id, version, token).ConfigureAwait(true);
            if (product is null)
            {
                logger.Warning("Delete aborted: Product '{ProductId}' was not found. User: '{Username}'.", id, username);
                return ApiResponse<ProductDto>.NotFound("The requested product could not be found. It may have already been removed.");
            }

            ProductDto deletedDto = product.ToDto();

            logger.Information("Successfully deleted product '{ProductTitle}' (ID: {ProductId}). User: '{Username}'.", product.Title, id, username);

            return ApiResponse<ProductDto>.Success(deletedDto);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency error while deleting product '{ProductId}'. User: '{Username}'", id, username);
            return ApiResponse<ProductDto>.Conflict("The product has been modified by another process. Please refresh and try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while deleting product '{ProductId}'. User: '{Username}'", id, username);
            return ApiResponse<ProductDto>.Failure("This product cannot be deleted because it is currently referenced by other records in the system.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing deletion of product '{ProductId}' to the database. User: '{Username}'", id, username);
            return ApiResponse<ProductDto>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> GetAllAsync(string username, int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        try
        {
            PagedResult<Product> pagedModels = await productRepository.GetAllAsync(pageNumber, itemsPerPage, token).ConfigureAwait(true);

            List<ProductDto> dtos = [.. pagedModels.Items.Select(x => x.ToDto())];

            PagedResult<ProductDto> pagedDtos = new(
                dtos,
                pagedModels.TotalCount,
                pagedModels.PageNumber,
                pagedModels.ItemsPerPage,
                pagedModels.TotalPages
            );

            logger.Debug("User '{Username}' retrieved page {Page} of products. Total count: {TotalCount}", username, pageNumber, pagedModels.TotalCount);

            return ApiResponse<PagedResult<ProductDto>>.Success(pagedDtos);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Database failure while retrieving all products. User: '{Username}'", username);
            return ApiResponse<PagedResult<ProductDto>>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<ProductDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            Product? product = await productRepository.GetAsync(id, token).ConfigureAwait(true);
            if (product is null)
            {
                logger.Warning("Read aborted: Product '{ProductId}' was not found. User: '{Username}'", id, username);
                return ApiResponse<ProductDto>.NotFound($"The requested product with ID '{id}' could not be found.");
            }

            logger.Debug("Retrieved product '{ProductId}'. User: '{Username}'", id, username);

            ProductDto dto = product.ToDto();
            return ApiResponse<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Database failure while retrieving product '{ProductId}'. User: '{Username}'", id, username);
            return ApiResponse<ProductDto>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token)
    {
        try
        {
            Product? product = await productRepository.GetAsync(id, token).ConfigureAwait(true);
            if (product is null)
            {
                logger.Warning("Update aborted: Product '{ProductId}' was not found. User: '{Username}'.", id, username);
                return ApiResponse<ProductDto>.NotFound("The requested product could not be found.");
            }

            product.Update(dto);
            await productRepository.UpdateAsync(product, version, token).ConfigureAwait(true);

            logger.Information("Successfully updated product '{ProductTitle}' (ID: {ProductId}). User: '{Username}'.", product.Title, id, username);

            return ApiResponse<ProductDto>.Success(product.ToDto());
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency update error while saving product '{ProductId}'. User: '{Username}'", id, username);
            return ApiResponse<ProductDto>.Conflict("The entity has been modified by another process. Please try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving product '{ProductId}'. User: '{Username}'", id, username);
            return ApiResponse<ProductDto>.Failure("A database constraint was violated. Please check your data and try again.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing product '{ProductId}' to the database. User: '{Username}'", id, username);
            return ApiResponse<ProductDto>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }
}