namespace ECommerse.Catalog.Apis;

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

        private static async Task<IResult> GetCategoriesAsync(
            [FromServices] ICategoryService categoriesService,
            HttpContext context,
            CancellationToken token)
        {
            string username = context.User.Identity?.Name ?? "Anonymous";
            ApiResponse<IEnumerable<CategoryDto>> response = await categoriesService.GetAllAsync(username, token);

            if (!string.IsNullOrWhiteSpace(response.Error))
            {
                return GetErrorResponse(response);
            }

            return TypedResults.Ok(response.Data);
        }

        private static async Task<IResult> GetCategoryAsync(
            [FromRoute] string id,
            [FromServices] ICategoryService categoriesService,
            HttpContext context,
            CancellationToken token)
        {
            string username = context.User.Identity?.Name ?? "Anonymous";
            ApiResponse<CategoryDto> response = await categoriesService.GetAsync(username, id, token);

            if (!string.IsNullOrWhiteSpace(response.Error))
            {
                return GetErrorResponse(response);
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
                return GetErrorResponse(response);
            }

            return TypedResults.CreatedAtRoute(
                value: response.Data,
                routeName: nameof(GetCategoryAsync),
                routeValues: new { id = response.Data.Id }
            );
        }

        private static async Task<IResult> UpdateCategoryAsync(
            [FromRoute] string id,
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
                return GetErrorResponse(response);
            }

            return TypedResults.Ok(response.Data);
        }

        private static async Task<IResult> DeleteCategoryAsync(
            [FromRoute] string id,
            [FromHeader(Name = "If-Match")] Guid version,
            [FromServices] ICategoryService categoriesService,
            HttpContext context,
            CancellationToken token)
        {
            string username = context.User.Identity?.Name ?? "Anonymous";
            ApiResponse<CategoryDto> response = await categoriesService.DeleteAsync(username, id, version, token);

            if (!string.IsNullOrWhiteSpace(response.Error))
            {
                return GetErrorResponse(response);
            }

            return TypedResults.Ok(response.Data);
        }

        private static IResult GetErrorResponse<T>(ApiResponse<T> result)
        {
            return result.Code switch
            {
                ErrorCodes.NotFound => TypedResults.NotFound(result.Error),
                ErrorCodes.Conflict => TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed),
                _ => TypedResults.InternalServerError(result.Error)
            };
        }
    }
}
