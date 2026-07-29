namespace ECommerce.Catalog.Dtos.Categories;

public readonly record struct CategoryDto(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Name,
    string Slug,
    Guid? ParentCategoryId
);
