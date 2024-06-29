namespace ProductCatalogService.DTOs
{
    public class CreateProductDto
    {
        public CreateProductDto()
        {
            Name = string.Empty;
            Description = string.Empty;
            SKU = string.Empty;
            ImageURLs = new List<string>();
            Tags = new List<string>();
            Category = string.Empty;
            CreationDate = string.Empty;
            UpdatedDate = string.Empty;
        }

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
