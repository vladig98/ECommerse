using System.ComponentModel.DataAnnotations;

namespace ProductCatalogService.DTOs
{
    public class EditProductDto
    {
        public EditProductDto()
        {
            ImageURLs = new List<string>();
            Tags = new List<string>();
        }

        [Length(3, 50)]
        public string? Name { get; set; }
        [Length(3, int.MaxValue)]
        public string? Description { get; set; }
        [Range(1, int.MaxValue)]
        public int? Quantity { get; set; }
        [Range(0.01, double.MaxValue)]
        public double? Price { get; set; }
        public string? Category { get; set; }
        public string? SKU { get; set; }
        public List<string> ImageURLs { get; set; }
        public List<string> Tags { get; set; }
        public string? CreationDate { get; set; }
        public string? UpdatedDate { get; set; }
        public bool? IsActive { get; set; }
        public double? Discount { get; set; }
        public double? Rating { get; set; }
    }
}
