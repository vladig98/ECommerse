namespace ECommerce.Catalog.Models;

public class Category : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    public ICollection<Category> SubCategories { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
