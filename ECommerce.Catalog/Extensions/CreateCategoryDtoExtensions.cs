namespace ECommerce.Catalog.Extensions;

public static class CreateCategoryDtoExtensions
{
    extension(CreateCategoryDto createCategoryDto)
    {
        public Category ToModel()
        {
            return new Category()
            {
                Name = createCategoryDto.Name,
                ParentCategoryId = createCategoryDto.ParentCategoryId,
                Slug = createCategoryDto.Slug
            };
        }
    }
}
