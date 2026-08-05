namespace ECommerce.Catalog.Apis;

internal static class ProductApis
{
    extension(IEndpointRouteBuilder router)
    {
        public IEndpointRouteBuilder MapProductEndpoints()
        {
            router.MapGet("/products", GetProductsAsync).AllowAnonymous();
            router.MapPost("/products", CreateProductAsync).AllowAnonymous();
            router.MapGet("/products/{id}", GetProductAsync).WithName(nameof(GetProductAsync)).AllowAnonymous();
            router.MapPut("/products/{id}", UpdateProductAsync).AllowAnonymous();
            router.MapDelete("/products/{id}", DeleteProductAsync).AllowAnonymous();

            return router;
        }
    }

    private static async Task<IResult> GetProductsAsync(
            [FromKeyedServices(KeyedServices.CachedProductService)] IProductService productsService,
            HttpContext context,
            CancellationToken token,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<PagedResult<ProductDto>> response = await productsService.GetAllAsync(username, pageNumber, pageSize, token).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> GetProductAsync(
        [FromRoute] Guid id,
        [FromKeyedServices(KeyedServices.CachedProductService)] IProductService productsService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<ProductDto> response = await productsService.GetAsync(username, id, token).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> CreateProductAsync(
        [FromBody] CreateProductDto dto,
        [FromKeyedServices(KeyedServices.CachedProductService)] IProductService productsService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<ProductDto> response = await productsService.CreateAsync(username, dto, token).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.CreatedAtRoute(
            value: response.Data,
            routeName: nameof(GetProductAsync),
            routeValues: new { id = response.Data!.Id }
        );
    }

    private static async Task<IResult> UpdateProductAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "If-Match")] Guid version,
        [FromBody] UpdateProductDto dto,
        [FromKeyedServices(KeyedServices.CachedProductService)] IProductService productsService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<ProductDto> response = await productsService.UpdateAsync(username, id, version, dto, token).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> DeleteProductAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "If-Match")] Guid version,
        [FromKeyedServices(KeyedServices.CachedProductService)] IProductService productsService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<ProductDto> response = await productsService.DeleteAsync(username, id, version, token).ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }
}
