namespace ECommerse.Catalog.Dtos.Categories;

public record class CreateCategoryDto
(
    string Name,
    string Slug,
    string? ParentId
);
