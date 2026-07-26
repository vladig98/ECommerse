namespace ECommerse.Catalog.Models;

public class Category : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public Category? ParentCategory { get; set; }
    public string? ParentId { get; set; }

    public ICollection<Category> SubCategories { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];

    public CategoryDto ToDto()
    {
        return new CategoryDto(
            Id: Id,
            CreatedAt: CreatedAt,
            UpdatedAt: UpdatedAt,
            Version: Version,
            Name: Name,
            Slug: Slug,
            ParentCategoryId: ParentId
        );
    }
}
