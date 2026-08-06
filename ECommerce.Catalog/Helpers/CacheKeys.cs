namespace ECommerce.Catalog.Helpers;

public static class CacheKeys
{
    public const string ProductKey = "Product:Id:{0}";
    public const string AllProductsKey = "Products";
    public const string PaginatedProducts = "Products_P{0}_S{1}";

    public const string CategoryKey = "Category:Id:{0}";
    public const string AllCategoriesKey = "Categories";
    public const string PaginatedCategories = "Categories_P{0}_S{1}";

    public const string AttributeKey = "Attribute:Id:{0}";
    public const string AllAttributesKey = "Attributes";
    public const string PaginatedAttributes = "Attributes_P{0}_S{1}";
}
