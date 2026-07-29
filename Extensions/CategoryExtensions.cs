namespace ECommerce.Catalog.Extensions;

public static class CategoryExtensions
{
    extension(Category category)
    {
        public CategoryDto ToDto()
        {
            return new CategoryDto
            (
                Id: category.Id,
                CreatedAt: category.CreatedAt,
                UpdatedAt: category.UpdatedAt,
                Version: category.Version,
                Name: category.Name,
                Slug: category.Slug,
                ParentCategoryId: category.ParentCategoryId
            );
        }
    }
}
