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
                SubCategories: category.SubCategories?.Select(x => x.ToDto()).Where(x => x is not null).OfType<CategoryDto>().ToList() ?? []
            );
        }

        public void Update(UpdateCategoryDto updateCategoryDto)
        {
            if (category is null)
            {
                return;
            }

            category.Name = updateCategoryDto.Name;
            category.ParentCategoryId = updateCategoryDto.ParentCategoryId;
            category.ParentCategory = null;
            category.Slug = updateCategoryDto.Slug;
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
