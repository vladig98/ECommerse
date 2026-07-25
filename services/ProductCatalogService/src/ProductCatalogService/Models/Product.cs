namespace ProductCatalogService.Models
{
    public class Product
    {
        public Product()
        {
            Id = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            SKU = string.Empty;
            ImageURLs = new List<string>();
            Tags = new List<string>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public ProductCategory Category { get; set; }
        public string SKU { get; set; }
        public List<string> ImageURLs { get; set; }
        public List<string> Tags { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public double? Discount { get; set; }
        public double? Rating { get; set; }
    }
}
