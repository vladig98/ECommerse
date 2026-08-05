namespace ECommerce.Catalog.Dtos.Categories;

internal record class UpdateCategoryDto
(
    string Name,
    string Slug,
    Guid? ParentCategoryId
);
