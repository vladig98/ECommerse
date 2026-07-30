namespace ECommerce.Catalog.Apis;

public static class AttributeApis
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
            [FromServices] IVariantAttributeService attributeService,
            HttpContext context,
            CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<List<VariantAttributeDto>> response = await attributeService.GetAllAsync(username, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> GetAttributeAsync(
        [FromRoute] Guid id,
        [FromServices] IVariantAttributeService attributeService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<VariantAttributeDto> response = await attributeService.GetAsync(username, id, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> CreateAttributeAsync(
        [FromBody] CreateVariantAttributeDto dto,
        [FromServices] IVariantAttributeService attributeService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<VariantAttributeDto> response = await attributeService.CreateAsync(username, dto, token);

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
        [FromServices] IVariantAttributeService attributeService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<VariantAttributeDto> response = await attributeService.UpdateAsync(username, id, version, dto, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> DeleteAttributeAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "If-Match")] Guid version,
        [FromServices] IVariantAttributeService attributeService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<VariantAttributeDto> response = await attributeService.DeleteAsync(username, id, version, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }
}