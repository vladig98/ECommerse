namespace ECommerce.Catalog.Helpers;

public static class CacheKeys
{
    public const string ProductKey = "Product:Id:{0}";
    public const string AllProductsKey = "Products";

    public const string CategoryKey = "Category:Id:{0}";
    public const string AllCategoriesKey = "Categories";

    public const string AttributeKey = "Attribute:Id:{0}";
    public const string AllAttributesKey = "Attributes";
}
