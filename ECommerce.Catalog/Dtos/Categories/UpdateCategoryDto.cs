namespace ECommerce.Catalog.Dtos.Categories;

public record class UpdateCategoryDto
(
    string Name,
    string Slug,
    Guid? ParentCategoryId
);
