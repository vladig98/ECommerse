namespace ECommerce.Catalog.Dtos.Categories;

public record class CategoryDto
(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Name,
    string Slug,
    CategoryDto? ParentCategory,
    List<CategoryDto> SubCategories
);
