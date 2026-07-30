namespace ECommerce.Catalog.Apis;

public static class CategoryApis
{
    extension(IEndpointRouteBuilder router)
    {
        public IEndpointRouteBuilder MapCategoryEndpoints()
        {
            router.MapGet("/categories", GetCategoriesAsync).AllowAnonymous();
            router.MapPost("/categories", CreateCategoryAsync).AllowAnonymous();
            router.MapGet("/categories/{id}", GetCategoryAsync).WithName(nameof(GetCategoryAsync)).AllowAnonymous();
            router.MapPut("/categories/{id}", UpdateCategoryAsync).AllowAnonymous();
            router.MapDelete("/categories/{id}", DeleteCategoryAsync).AllowAnonymous();

            return router;
        }
    }

    private static async Task<IResult> GetCategoriesAsync(
            [FromServices] ICategoryService categoriesService,
            HttpContext context,
            CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<List<CategoryDto>> response = await categoriesService.GetAllAsync(username, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> GetCategoryAsync(
        [FromRoute] Guid id,
        [FromServices] ICategoryService categoriesService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<CategoryDto> response = await categoriesService.GetAsync(username, id, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> CreateCategoryAsync(
        [FromBody] CreateCategoryDto dto,
        [FromServices] ICategoryService categoriesService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<CategoryDto> response = await categoriesService.CreateAsync(username, dto, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.CreatedAtRoute(
            value: response.Data,
            routeName: nameof(GetCategoryAsync),
            routeValues: new { id = response.Data!.Id }
        );
    }

    private static async Task<IResult> UpdateCategoryAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "If-Match")] Guid version,
        [FromBody] UpdateCategoryDto dto,
        [FromServices] ICategoryService categoriesService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<CategoryDto> response = await categoriesService.UpdateAsync(username, id, version, dto, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }

    private static async Task<IResult> DeleteCategoryAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "If-Match")] Guid version,
        [FromServices] ICategoryService categoriesService,
        HttpContext context,
        CancellationToken token)
    {
        string username = context.User.Identity?.Name ?? "Anonymous";
        ApiResponse<CategoryDto> response = await categoriesService.DeleteAsync(username, id, version, token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.ToErrorResult();
        }

        return TypedResults.Ok(response.Data);
    }
}
