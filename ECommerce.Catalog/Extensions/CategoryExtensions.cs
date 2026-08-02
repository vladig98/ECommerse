namespace ECommerce.Catalog.Extensions;

public static class CategoryExtensions
{
    extension(Category? category)
    {
        public CategoryDto? ToDto()
        {
            if (category is null)
            {
                return null;
            }

            return new CategoryDto
            (
                Id: category.Id,
                CreatedAt: category.CreatedAt,
                UpdatedAt: category.UpdatedAt,
                Version: category.Version,
                Name: category.Name,
                Slug: category.Slug,
                ParentCategory: category.ParentCategory?.ToDto(),
                SubCategories: category.SubCategories?.Select(x => x.ToDto()).ToList() ?? []
            );
        }

        public CategoryEventDto? ToEventData()
        {
            if (category is null)
            {
                return null;
            }

            CategoryEventDto categoryEventData = new
            (
                Id: category.Id,
                Name: category.Name,
                Slug: category.Slug
            );

            return categoryEventData;
        }
    }
}
