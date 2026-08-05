namespace ECommerce.Catalog.Dtos.Categories;

internal record class CreateCategoryDto
(
    string Name,
    string Slug,
    Guid? ParentCategoryId
);
