using System.ComponentModel.DataAnnotations;

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

        [Required]
        [Length(3, 50)]
        public string Name { get; set; }
        [Required]
        [Length(3, int.MaxValue)]
        public string Description { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public double Price { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        public string SKU { get; set; }
        [Required]
        public List<string> ImageURLs { get; set; }
        [Required]
        public List<string> Tags { get; set; }
        [Required]
        public string CreationDate { get; set; }
        [Required]
        public string UpdatedDate { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public double? Discount { get; set; }
        public double? Rating { get; set; }
    }
}
