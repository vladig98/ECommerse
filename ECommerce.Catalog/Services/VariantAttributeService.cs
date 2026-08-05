namespace ECommerce.Catalog.Services;

internal class VariantAttributeService(IVariantAttributeRepository variantAttributeRepository, ILogger logger) : IVariantAttributeService
{
    public async Task<ApiResponse<VariantAttributeDto>> CreateAsync(string username, CreateVariantAttributeDto dto, CancellationToken token)
    {
        try
        {
            VariantAttributeModel attribute = dto.ToModel();

            VariantAttributeModel createdAttribute = await variantAttributeRepository.AddAsync(attribute, token).ConfigureAwait(true);
            VariantAttributeDto attributeDto = createdAttribute.ToDto();

            logger.Information("Successfully created variant attribute '{Name}: {Value}' (ID: {AttributeId}). User: '{Username}'", createdAttribute.Name, createdAttribute.Value, createdAttribute.Id, username);

            return ApiResponse<VariantAttributeDto>.Success(attributeDto);
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving attribute '{Name}: {Value}'. User: '{Username}'", dto.Name, dto.Value, username);
            return ApiResponse<VariantAttributeDto>.Failure("A database constraint was violated. This attribute name and value combination might already exist.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing attribute '{Name}: {Value}' to the database. User: '{Username}'", dto.Name, dto.Value, username);
            return ApiResponse<VariantAttributeDto>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }

    public async Task<ApiResponse<VariantAttributeDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        try
        {
            VariantAttributeModel? attribute = await variantAttributeRepository.DeleteAsync(id, version, token).ConfigureAwait(true);
            if (attribute is null)
            {
                logger.Warning("Delete aborted: Variant attribute '{AttributeId}' was not found. User: '{Username}'.", id, username);
                return ApiResponse<VariantAttributeDto>.NotFound("The requested attribute could not be found. It may have already been removed.");
            }

            VariantAttributeDto deletedDto = attribute.ToDto();

            logger.Information("Successfully deleted variant attribute '{Name}: {Value}' (ID: {AttributeId}). User: '{Username}'", attribute.Name, attribute.Value, id, username);

            return ApiResponse<VariantAttributeDto>.Success(deletedDto);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency error while deleting attribute '{AttributeId}'. User: '{Username}'", id, username);
            return ApiResponse<VariantAttributeDto>.Conflict("The attribute has been modified by another process. Please refresh and try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while deleting attribute '{AttributeId}'. User: '{Username}'", id, username);
            return ApiResponse<VariantAttributeDto>.Failure("This attribute cannot be deleted because it is currently assigned to one or more product variants.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing deletion of attribute '{AttributeId}' to the database. User: '{Username}'", id, username);
            return ApiResponse<VariantAttributeDto>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }
    }

    public async Task<ApiResponse<PagedResult<VariantAttributeDto>>> GetAllAsync(string username, int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        try
        {
            PagedResult<VariantAttributeModel> pagedModels = await variantAttributeRepository.GetAllAsync(pageNumber, itemsPerPage, token).ConfigureAwait(true);

            List<VariantAttributeDto> dtos = [.. pagedModels.Items.Select(x => x.ToDto())];

            PagedResult<VariantAttributeDto> pagedDtos = new(
                dtos,
                pagedModels.TotalCount,
                pagedModels.PageNumber,
                pagedModels.ItemsPerPage,
                pagedModels.TotalPages
            );

            logger.Debug("User '{Username}' retrieved page {Page} of variant attributes. Total count: {TotalCount}", username, pageNumber, pagedModels.TotalCount);

            return ApiResponse<PagedResult<VariantAttributeDto>>.Success(pagedDtos);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Database failure while retrieving all variant attributes. User: '{Username}'", username);
            return ApiResponse<PagedResult<VariantAttributeDto>>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<VariantAttributeDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        try
        {
            VariantAttributeModel? attribute = await variantAttributeRepository.GetAsync(id, token).ConfigureAwait(true);
            if (attribute is null)
            {
                logger.Warning("Read aborted: Variant attribute '{AttributeId}' was not found. User: '{Username}'", id, username);
                return ApiResponse<VariantAttributeDto>.NotFound($"The requested attribute with ID '{id}' could not be found.");
            }

            logger.Debug("Retrieved variant attribute '{AttributeId}'. User: '{Username}'", id, username);

            return ApiResponse<VariantAttributeDto>.Success(attribute.ToDto());
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Database failure while retrieving variant attribute '{AttributeId}'. User: '{Username}'", id, username);
            return ApiResponse<VariantAttributeDto>.Failure("An unexpected error occurred while retrieving the data.");
        }
    }

    public async Task<ApiResponse<VariantAttributeDto>> UpdateAsync(string username, Guid id, Guid version, UpdateVariantAttributeDto dto, CancellationToken token)
    {
        try
        {
            VariantAttributeModel? attribute = await variantAttributeRepository.GetAsync(id, token).ConfigureAwait(true);
            if (attribute is null)
            {
                logger.Warning("Update aborted: Variant attribute '{AttributeId}' was not found. User: '{Username}'.", id, username);
                return ApiResponse<VariantAttributeDto>.NotFound("The requested attribute could not be found.");
            }

            attribute.Update(dto);
            await variantAttributeRepository.UpdateAsync(attribute, version, token).ConfigureAwait(true);

            VariantAttributeDto attributeDto = attribute.ToDto();

            logger.Information("Successfully updated variant attribute '{Name}: {Value}' (ID: {AttributeId}). User: '{Username}'.", attribute.Name, attribute.Value, id, username);

            return ApiResponse<VariantAttributeDto>.Success(attributeDto);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency update error while saving attribute '{AttributeId}'. User: '{Username}'", id, username);
            return ApiResponse<VariantAttributeDto>.Conflict("The entity has been modified by another process. Please try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving attribute '{AttributeId}'. User: '{Username}'", id, username);
            return ApiResponse<VariantAttributeDto>.Failure("A database constraint was violated. This attribute name and value combination might already exist.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing attribute '{AttributeId}' to the database. User: '{Username}'", id, username);
            return ApiResponse<VariantAttributeDto>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }
    }
}