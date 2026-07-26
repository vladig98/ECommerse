namespace ECommerse.Catalog.Dtos.Categories;

public readonly record struct CategoryDto(
    string Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Name,
    string Slug,
    string? ParentCategoryId
);
