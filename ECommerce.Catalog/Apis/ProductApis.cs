namespace ECommerce.Catalog.Apis;

public static class ProductApis
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
            [FromServices] IProductService productsService,
            HttpContext context,
            CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<List<ProductDto>> response = await productsService.GetAllAsync(username, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> GetProductAsync(
        [FromRoute] Guid id,
        [FromServices] IProductService productsService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<ProductDto> response = await productsService.GetAsync(username, id, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> CreateProductAsync(
        [FromBody] CreateProductDto dto,
        [FromServices] IProductService productService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<ProductDto> response = await productService.CreateAsync(username, dto, token);

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
        [FromServices] IProductService productsService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<ProductDto> response = await productsService.UpdateAsync(username, id, version, dto, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> DeleteProductAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "If-Match")] Guid version,
        [FromServices] IProductService productsService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<ProductDto> response = await productsService.DeleteAsync(username, id, version, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }
}
