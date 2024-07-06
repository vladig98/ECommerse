namespace ProductCatalogService.DTOs
{
    public class ProductDto
    {
        public ProductDto()
        {
            Id = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            SKU = string.Empty;
            ImageURLs = new List<string>();
            Tags = new List<string>();
            Category = string.Empty;
            CreationDate = string.Empty;
            UpdatedDate = string.Empty;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public string SKU { get; set; }
        public List<string> ImageURLs { get; set; }
        public List<string> Tags { get; set; }
        public string CreationDate { get; set; }
        public string UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public double? Discount { get; set; }
        public double? Rating { get; set; }
    }
}
