namespace ECommerce.Catalog.Apis;

internal static class AttributeApis
{
    extension(IEndpointRouteBuilder router)
    {
        public IEndpointRouteBuilder MapAttributeEndpoints()
        {
            router.MapGet("/attributes", GetAttributesAsync).AllowAnonymous();
            router.MapPost("/attributes", CreateAttributeAsync).AllowAnonymous();
            router.MapGet("/attributes/{id}", GetAttributeAsync).WithName(nameof(GetAttributeAsync)).AllowAnonymous();
            router.MapPut("/attributes/{id}", UpdateAttributeAsync).AllowAnonymous();
            router.MapDelete("/attributes/{id}", DeleteAttributeAsync).AllowAnonymous();

            return router;
        }
    }

    private static async Task<IResult> GetAttributesAsync(
            [FromKeyedServices(KeyedServices.CachedAttributeService)] IVariantAttributeService attributeService,
            HttpContext context,
            CancellationToken token,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<PagedResult<VariantAttributeDto>> response = await attributeService.GetAllAsync(username, pageNumber, pageSize, token).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> GetAttributeAsync(
        [FromRoute] Guid id,
        [FromKeyedServices(KeyedServices.CachedAttributeService)] IVariantAttributeService attributeService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<VariantAttributeDto> response = await attributeService.GetAsync(username, id, token).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> CreateAttributeAsync(
        [FromBody] CreateVariantAttributeDto dto,
        [FromKeyedServices(KeyedServices.CachedAttributeService)] IVariantAttributeService attributeService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<VariantAttributeDto> response = await attributeService.CreateAsync(username, dto, token).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.CreatedAtRoute(
            value: response.Data,
            routeName: nameof(GetAttributeAsync),
            routeValues: new { id = response.Data!.Id }
        );
    }

    private static async Task<IResult> UpdateAttributeAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "If-Match")] Guid version,
        [FromBody] UpdateVariantAttributeDto dto,
        [FromKeyedServices(KeyedServices.CachedAttributeService)] IVariantAttributeService attributeService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<VariantAttributeDto> response = await attributeService.UpdateAsync(username, id, version, dto, token).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> DeleteAttributeAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "If-Match")] Guid version,
        [FromKeyedServices(KeyedServices.CachedAttributeService)] IVariantAttributeService attributeService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<VariantAttributeDto> response = await attributeService.DeleteAsync(username, id, version, token).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }
}