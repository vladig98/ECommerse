namespace ECommerce.Catalog.Services;

public class ProductService(
    IProductsRepository productsService,
    IProductMediaRepository productMediaService,
    IProductVariantRepository productVariantService,
    IVariantAttributeRepository variantAttributeService,
    ICategoryRepository categoryRepository,
    MainDbContext dbContext,
    ILogger logger) : IProductService
{
    public async Task<ApiResponse<ProductDto>> CreateAsync(string username, CreateProductDto dto, CancellationToken token)
    {
        ApiResponse<Product> productResponse = productsService.Create(username, dto);
        if (!string.IsNullOrWhiteSpace(productResponse.Error))
        {
            logger.Warning("Aborting product creation: Failed to create root product. User: '{Username}'", username);
            return ApiResponse<ProductDto>.FromResponse(productResponse);
        }

        ApiResponse<Category> categoryResponse = await categoryRepository.GetAsync(username, dto.CategoryId, token);
        if (!string.IsNullOrWhiteSpace(categoryResponse.Error))
        {
            logger.Warning("Aborting product creation: Category '{CategoryId}' not found. User: '{Username}'", dto.CategoryId, username);
            return ApiResponse<ProductDto>.FromResponse(categoryResponse);
        }

        Product product = productResponse.Data!;
        Category category = categoryResponse.Data!;

        // Needed for DTO mapping later
        product.Category = category;

        foreach (CreateProductMediaDto mediaDto in dto.ProductMedia)
        {
            ApiResponse<ProductMedia> mediaResponse = productMediaService.Create(username, mediaDto);
            if (!string.IsNullOrWhiteSpace(mediaResponse.Error))
            {
                logger.Warning("Aborting product creation: Failed to create root media '{Url}'. User: '{Username}'", mediaDto.Url, username);
                return ApiResponse<ProductDto>.FromResponse(mediaResponse);
            }

            product.Media.Add(mediaResponse.Data!);
        }

        Dictionary<string, VariantAttribute> trackedNewAttributes = [];

        foreach (CreateProductVariantDto variantDto in dto.ProductVariants)
        {
            ApiResponse<ProductVariant> variantResponse = productVariantService.Create(username, variantDto);
            if (!string.IsNullOrWhiteSpace(variantResponse.Error))
            {
                logger.Warning("Aborting product creation: Failed to create variant '{Sku}'. User: '{Username}'", variantDto.Sku, username);
                return ApiResponse<ProductDto>.FromResponse(variantResponse);
            }

            ProductVariant productVariant = variantResponse.Data!;
            product.Variants.Add(productVariant);

            foreach (CreateProductMediaDto mediaDto in variantDto.Media)
            {
                ApiResponse<ProductMedia> mediaResponse = productMediaService.Create(username, mediaDto);
                if (!string.IsNullOrWhiteSpace(mediaResponse.Error))
                {
                    logger.Warning("Aborting product creation: Failed to create media for variant '{Sku}'. User: '{Username}'", variantDto.Sku, username);
                    return ApiResponse<ProductDto>.FromResponse(mediaResponse);
                }

                productVariant.Media.Add(mediaResponse.Data!);
            }

            foreach (CreateVariantAttributeDto attributeDto in variantDto.NewAttributes)
            {
                string dictKey = $"{attributeDto.Name.ToLower()}-{attributeDto.Value.ToLower()}";

                if (!trackedNewAttributes.TryGetValue(dictKey, out VariantAttribute? attribute))
                {
                    ApiResponse<VariantAttribute> attrResponse = variantAttributeService.Create(username, attributeDto);
                    if (!string.IsNullOrWhiteSpace(attrResponse.Error))
                    {
                        logger.Warning("Aborting product creation: Failed to create new attribute '{Name}: {Value}'. User: '{Username}'", attributeDto.Name, attributeDto.Value, username);
                        return ApiResponse<ProductDto>.FromResponse(attrResponse);
                    }

                    attribute = attrResponse.Data!;
                    trackedNewAttributes[dictKey] = attribute;
                }

                productVariant.VariantAttributes.Add(new ProductVariantAttribute
                {
                    Attribute = attribute
                });
            }

            foreach (Guid attributeId in variantDto.Attributes)
            {
                ApiResponse<VariantAttribute> attrResponse = await variantAttributeService.GetAsync(username, attributeId, token);
                if (!string.IsNullOrWhiteSpace(attrResponse.Error))
                {
                    logger.Warning("Aborting product creation: Existing attribute '{AttributeId}' could not be fetched. User: '{Username}'", attributeId, username);
                    return ApiResponse<ProductDto>.FromResponse(attrResponse);
                }

                VariantAttribute attribute = attrResponse.Data!;
                productVariant.VariantAttributes.Add(new ProductVariantAttribute
                {
                    Attribute = attribute,
                });
            }
        }

        ProductCreated eventCreated = product.ToEventData();
        EventMessage productCreatedMessage = new()
        {
            Key = product.Id.ToString(),
            EventType = nameof(ProductCreated),
            Value = JsonSerializer.Serialize(eventCreated)
        };

        dbContext.EventMessages.Add(productCreatedMessage);

        try
        {
            await dbContext.SaveChangesAsync(token);

            logger.Information("Successfully committed full product graph for '{ProductTitle}' (ID: {ProductId}). User: '{Username}'", product.Title, product.Id, username);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency update error while saving product '{ProductTitle}'. User: '{Username}'", dto.Title, username);
            return ApiResponse<ProductDto>.Conflict("The entity has been modified by another process. Please try again.");
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

        return ApiResponse<ProductDto>.Success(product.ToDto());
    }

    public async Task<ApiResponse<ProductDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        ApiResponse<Product> productResponse = await productsService.DeleteAsync(username, id, version, token);
        if (!string.IsNullOrWhiteSpace(productResponse.Error))
        {
            logger.Warning("Aborting product deletion: Failed to prepare product '{ProductId}' for deletion. User: '{Username}'. Reason: {Error}", id, username, productResponse.Error);
            return ApiResponse<ProductDto>.FromResponse(productResponse);
        }

        Product product = productResponse.Data!;
        ProductDeleted eventDeleted = new
        (
            Id: product.Id
        );

        EventMessage productDeletedMessage = new()
        {
            Key = product.Id.ToString(),
            EventType = nameof(ProductDeleted),
            Value = JsonSerializer.Serialize(eventDeleted)
        };

        dbContext.EventMessages.Add(productDeletedMessage);

        try
        {
            await dbContext.SaveChangesAsync(token);
            logger.Information("Successfully committed deletion for product '{ProductTitle}' (ID: {ProductId}). User: '{Username}'", product.Title, product.Id, username);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency error while deleting product '{ProductTitle}' (ID: {ProductId}). User: '{Username}'", product.Title, id, username);
            return ApiResponse<ProductDto>.Conflict("The product has been modified by another process. Please refresh and try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while deleting product '{ProductTitle}' (ID: {ProductId}). User: '{Username}'", product.Title, id, username);
            return ApiResponse<ProductDto>.Failure("This product cannot be deleted because it is currently referenced by other records in the system.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing deletion of product '{ProductTitle}' (ID: {ProductId}) to the database. User: '{Username}'", product.Title, id, username);
            return ApiResponse<ProductDto>.Failure("An unexpected error occurred while processing the deletion. Please try again later.");
        }

        return ApiResponse<ProductDto>.Success(product.ToDto());
    }

    public async Task<ApiResponse<List<ProductDto>>> GetAllAsync(string username, CancellationToken token)
    {
        ApiResponse<List<Product>> productResponse = await productsService.GetAllAsync(username, token);
        if (!string.IsNullOrWhiteSpace(productResponse.Error))
        {
            logger.Warning("Failed to retrieve products. User: '{Username}'. Reason: {Error}", username, productResponse.Error);
            return ApiResponse<List<ProductDto>>.FromResponse(productResponse);
        }

        List<Product> products = productResponse.Data!;
        return ApiResponse<List<ProductDto>>.Success([.. products.Select(x => x.ToDto())]);
    }

    public async Task<ApiResponse<ProductDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        ApiResponse<Product> productResponse = await productsService.GetAsync(username, id, token);
        if (!string.IsNullOrWhiteSpace(productResponse.Error))
        {
            logger.Warning("Failed to retrieve product '{ProductId}'. User: '{Username}'. Reason: {Error}", id, username, productResponse.Error);
            return ApiResponse<ProductDto>.FromResponse(productResponse);
        }

        Product product = productResponse.Data!;
        return ApiResponse<ProductDto>.Success(product.ToDto());
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token)
    {
        ApiResponse<Product> productResponse = await productsService.UpdateAsync(username, id, version, dto, token);
        if (!string.IsNullOrWhiteSpace(productResponse.Error))
        {
            logger.Warning("Aborting product update: Failed to update root product '{ProductId}'. User: '{Username}'", id, username);
            return ApiResponse<ProductDto>.FromResponse(productResponse);
        }

        ApiResponse<Category> categoryResponse = await categoryRepository.GetAsync(username, dto.CategoryId, token);
        if (!string.IsNullOrWhiteSpace(categoryResponse.Error))
        {
            logger.Warning("Aborting product update: Category '{CategoryId}' not found. User: '{Username}'", dto.CategoryId, username);
            return ApiResponse<ProductDto>.FromResponse(categoryResponse);
        }

        Product product = productResponse.Data!;
        Category category = categoryResponse.Data!;

        // Needed for DTO mapping later
        product.Category = category;

        HashSet<Guid> mediaIds = [.. product.Media.Select(x => x.Id)];
        Dictionary<Guid, ProductVariant> variants = product.Variants.ToDictionary(x => x.Id, x => x);

        foreach (UpdateProductMediaDto mediaDto in dto.ProductMedia)
        {
            if (!mediaIds.Contains(mediaDto.Id))
            {
                logger.Warning("Aborting product update: Media '{MediaId}' is not associated with product '{ProductId}'. User: '{Username}'", mediaDto.Id, id, username);
                return ApiResponse<ProductDto>.NotFound($"Media item '{mediaDto.Id}' does not belong to this product.");
            }

            ApiResponse<ProductMedia> mediaResponse = await productMediaService.UpdateAsync(username, mediaDto.Id, mediaDto.Version, mediaDto, token);
            if (!string.IsNullOrWhiteSpace(mediaResponse.Error))
            {
                logger.Warning("Aborting product update: Failed to update media '{MediaId}'. User: '{Username}'", mediaDto.Id, username);
                return ApiResponse<ProductDto>.FromResponse(mediaResponse);
            }
        }

        Dictionary<Guid, decimal> originalPrices = product.Variants.ToDictionary(x => x.Id, x => x.BasePrice);

        foreach (UpdateProductVariantDto variantDto in dto.ProductVariants)
        {
            if (!variants.TryGetValue(variantDto.Id, out ProductVariant? variant))
            {
                logger.Warning("Aborting product update: Variant '{VariantId}' is not associated with product '{ProductId}'. User: '{Username}'", variantDto.Id, id, username);
                return ApiResponse<ProductDto>.NotFound($"Variant '{variantDto.Id}' does not belong to this product.");
            }

            ApiResponse<ProductVariant> variantResponse = await productVariantService.UpdateAsync(username, variantDto.Id, variantDto.Version, variantDto, token);
            if (!string.IsNullOrWhiteSpace(variantResponse.Error))
            {
                logger.Warning("Aborting product update: Failed to update variant '{Sku}'. User: '{Username}'", variantDto.Sku, username);
                return ApiResponse<ProductDto>.FromResponse(variantResponse);
            }

            HashSet<Guid> variantMediaIds = [.. variant.Media.Select(x => x.Id)];
            foreach (UpdateProductMediaDto mediaDto in variantDto.Media)
            {
                if (!variantMediaIds.Contains(mediaDto.Id))
                {
                    logger.Warning("Aborting product update: Media '{MediaId}' is not associated with variant '{Sku}'. User: '{Username}'", mediaDto.Id, variantDto.Sku, username);
                    return ApiResponse<ProductDto>.NotFound($"Media item '{mediaDto.Id}' does not belong to variant '{variantDto.Sku}'.");
                }

                ApiResponse<ProductMedia> mediaResponse = await productMediaService.UpdateAsync(username, mediaDto.Id, mediaDto.Version, mediaDto, token);
                if (!string.IsNullOrWhiteSpace(mediaResponse.Error))
                {
                    logger.Warning("Aborting product update: Failed to update media '{MediaId}' for variant '{Sku}'. User: '{Username}'", mediaDto.Id, variantDto.Sku, username);
                    return ApiResponse<ProductDto>.FromResponse(mediaResponse);
                }
            }

            HashSet<Guid> existingAttributeIds = [.. variant.VariantAttributes.Select(x => x.AttributeId)];
            HashSet<Guid> incomingAttributeIds = [.. variantDto.Attributes];

            List<ProductVariantAttribute> toRemove = [.. variant.VariantAttributes.Where(va => !incomingAttributeIds.Contains(va.AttributeId))];
            foreach (ProductVariantAttribute mapping in toRemove)
            {
                variant.VariantAttributes.Remove(mapping);
            }

            IEnumerable<Guid> toAdd = incomingAttributeIds.Where(id => !existingAttributeIds.Contains(id));
            foreach (Guid newId in toAdd)
            {
                ApiResponse<VariantAttribute> attrResponse = await variantAttributeService.GetAsync(username, newId, token);
                if (!string.IsNullOrWhiteSpace(attrResponse.Error))
                {
                    logger.Warning("Aborting product creation: Existing attribute '{AttributeId}' could not be fetched. User: '{Username}'", newId, username);
                    return ApiResponse<ProductDto>.FromResponse(attrResponse);
                }

                variant.VariantAttributes.Add(new ProductVariantAttribute
                {
                    Attribute = attrResponse.Data!
                });
            }
        }

        ProductUpdated eventUpdated = product.ToEventDataUpdate();
        EventMessage productUpdatedMessage = new()
        {
            Key = product.Id.ToString(),
            EventType = nameof(ProductUpdated),
            Value = JsonSerializer.Serialize(eventUpdated)
        };

        dbContext.EventMessages.Add(productUpdatedMessage);

        foreach (ProductVariant variant in product.Variants)
        {
            if (!originalPrices.TryGetValue(variant.Id, out decimal originalPrice))
            {
                continue;
            }

            if(variant.BasePrice != originalPrice)
            {
                ProductPriceChanged eventPriceChanged = new
                (
                    ProductId: product.Id,
                    VariantId: variant.Id,
                    Sku: variant.Sku,
                    NewPrice: variant.BasePrice
                );

                EventMessage productPriceChangedMessage = new()
                {
                    Key = product.Id.ToString(),
                    EventType = nameof(ProductPriceChanged),
                    Value = JsonSerializer.Serialize(eventPriceChanged)
                };

                dbContext.EventMessages.Add(productPriceChangedMessage);
            }
        }

        try
        {
            await dbContext.SaveChangesAsync(token);
            logger.Information("Successfully committed update for product graph '{ProductTitle}' (ID: {ProductId}). User: '{Username}'", product.Title, product.Id, username);
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            logger.Error(conflict, "Concurrency update error while saving product '{ProductTitle}'. User: '{Username}'", dto.Title, username);
            return ApiResponse<ProductDto>.Conflict("The entity has been modified by another process. Please try again.");
        }
        catch (DbUpdateException dbEx)
        {
            logger.Error(dbEx, "Database constraint violation while saving product '{ProductTitle}'. User: '{Username}'", dto.Title, username);
            return ApiResponse<ProductDto>.Failure("A database constraint was violated. Please check your data and try again.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected failure while committing product '{ProductTitle}' to the database. User: '{Username}'", dto.Title, username);
            return ApiResponse<ProductDto>.Failure("An unexpected error occurred while processing your request. Please try again later.");
        }

        return ApiResponse<ProductDto>.Success(product.ToDto());
    }
}