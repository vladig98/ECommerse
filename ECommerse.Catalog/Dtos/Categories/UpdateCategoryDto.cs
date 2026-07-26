namespace ECommerse.Catalog.Dtos.Categories;

public record class UpdateCategoryDto
(
    string Name,
    string Slug,
    string? ParentId
);
