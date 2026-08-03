namespace ECommerce.Catalog.Dtos.Categories;

public record class CreateCategoryDto
(
    string Name,
    string Slug,
    Guid? ParentCategoryId
);
