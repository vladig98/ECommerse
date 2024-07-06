using Microsoft.EntityFrameworkCore;
using ProductCatalogService.Events.Contracts;
using System.Globalization;

namespace ProductCatalogService.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductsDbContext _dbContext;
        private readonly ILogger<ProductService> _logger;
        private readonly IEventBus _eventBus;

        public ProductService(ProductsDbContext dbContext, ILogger<ProductService> logger, IEventBus eventBus)
        {
            _dbContext = dbContext;
            _logger = logger;
            _eventBus = eventBus;
        }

        private ProductDto GenerateProductDto(Product product)
        {
            var productDto = new ProductDto
            {
                Category = product.Category.ToString(),
                UpdatedDate = product.UpdatedDate.ToString(GlobalConstants.DateFormat, CultureInfo.InvariantCulture),
                CreationDate = product.CreationDate.ToString(GlobalConstants.DateFormat, CultureInfo.InvariantCulture),
                Description = product.Description,
                Discount = product.Discount,
                Id = product.Id,
                ImageURLs = product.ImageURLs,
                IsActive = product.IsActive,
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                Rating = product.Rating,
                SKU = product.SKU,
                Tags = product.Tags
            };

            return productDto;
        }

        private (bool isDateValid, DateTime date) ParseDate(string date)
        {
            var dateValid = DateTime.TryParseExact(date, GlobalConstants.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate);

            return (dateValid, parsedDate);
        }

        public async Task<ServiceResult<ProductDto>> CreateProduct(CreateProductDto productData)
        {
            var productExists = await _dbContext.Products.FirstOrDefaultAsync(x => x.Name == productData.Name);

            if (productExists != null)
            {
                _logger.LogError(GlobalConstants.ProductAlredyExists);
                return ServiceResult<ProductDto>.Failure(GlobalConstants.ProductAlredyExists);
            }

            var (createdDateValid, createdDate) = ParseDate(productData.CreationDate);

            if (!createdDateValid)
            {
                _logger.LogError(GlobalConstants.InvalidCreatedDate);
                return ServiceResult<ProductDto>.Failure(GlobalConstants.InvalidCreatedDate);
            }

            var (updatedDateValid, updatedDate) = ParseDate(productData.UpdatedDate);

            if (!updatedDateValid)
            {
                _logger.LogError(GlobalConstants.InvalidUpdatedDate);
                return ServiceResult<ProductDto>.Failure(GlobalConstants.InvalidUpdatedDate);
            }

            var categoryValid = Enum.TryParse<ProductCategory>(productData.Category, true, out ProductCategory category);

            if (!categoryValid)
            {
                _logger.LogError(GlobalConstants.InvalidCategory);
                return ServiceResult<ProductDto>.Failure(GlobalConstants.InvalidCategory);
            }

            createdDate = DateTime.SpecifyKind(createdDate, DateTimeKind.Utc);
            updatedDate = DateTime.SpecifyKind(updatedDate, DateTimeKind.Utc);

            var product = new Product
            {
                Category = category,
                CreationDate = createdDate,
                Description = productData.Description,
                Discount = productData.Discount,
                Id = Guid.NewGuid().ToString(),
                ImageURLs = productData.ImageURLs,
                IsActive = productData.IsActive,
                Name = productData.Name,
                Price = productData.Price,
                Quantity = productData.Quantity,
                Rating = productData.Rating,
                SKU = productData.SKU,
                Tags = productData.Tags,
                UpdatedDate = updatedDate
            };

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var productDto = GenerateProductDto(product);

            var productCreatedEvent = new ProductCreatedEvent { ProductId = product.Id };
            _eventBus.Publish(productCreatedEvent);

            _logger.LogInformation(GlobalConstants.ProductCreated);
            return ServiceResult<ProductDto>.Success(productDto, GlobalConstants.ProductCreated);
        }

        public async Task<ServiceResult<AllProductsDto>> GetAllProducts()
        {
            var products = await _dbContext.Products.ToListAsync();

            var productDtos = new List<ProductDto>();

            foreach (var product in products)
            {
                productDtos.Add(GenerateProductDto(product));
            }

            var allProductsDto = new AllProductsDto
            {
                Products = productDtos
            };

            _logger.LogInformation(GlobalConstants.AllProductsRetrieved);
            return ServiceResult<AllProductsDto>.Success(allProductsDto, GlobalConstants.AllProductsRetrieved);
        }

        public async Task<ServiceResult<ProductDto>> GetProduct(string id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                _logger.LogError(GlobalConstants.ProductDoesNotExist);
                return ServiceResult<ProductDto>.Failure(GlobalConstants.ProductDoesNotExist);
            }

            var productDto = GenerateProductDto(product);

            _logger.LogInformation(GlobalConstants.ProductRetrieved);
            return ServiceResult<ProductDto>.Success(productDto, GlobalConstants.ProductRetrieved);
        }

        public async Task<ServiceResult<ProductDto>> UpdateProduct(string id, EditProductDto updatedData)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                _logger.LogError(GlobalConstants.ProductDoesNotExist);
                return ServiceResult<ProductDto>.Failure(GlobalConstants.ProductDoesNotExist);
            }

            var nameTaken = await _dbContext.Products.Where(x => x.Id != id).FirstOrDefaultAsync(x => x.Name == updatedData.Name);

            if (nameTaken != null)
            {
                _logger.LogError(GlobalConstants.InvalidName);
                return ServiceResult<ProductDto>.Failure(GlobalConstants.InvalidName);
            }

            var (createdDateValid, createdDate) = ParseDate(updatedData.CreationDate);

            if (!createdDateValid)
            {
                createdDate = product.CreationDate;
            }

            var (updatedDateValid, updatedDate) = ParseDate(updatedData.UpdatedDate);
            
            if (!updatedDateValid)
            {
                updatedDate = product.UpdatedDate;
            }

            var newCategory = updatedData.Category;
            var categoryValid = Enum.TryParse<ProductCategory>(newCategory, true, out ProductCategory category);

            if (!categoryValid)
            {
                category = product.Category;
            }

            createdDate = DateTime.SpecifyKind(createdDate, DateTimeKind.Utc);
            updatedDate = DateTime.SpecifyKind(updatedDate, DateTimeKind.Utc);

            product.SKU = updatedData.SKU ?? product.SKU;
            product.Quantity = updatedData.Quantity ?? product.Quantity;
            product.Price = updatedData.Price ?? product.Price;
            product.Description = updatedData.Description ?? product.Description;
            product.CreationDate = createdDate;
            product.UpdatedDate = updatedDate;
            product.Category = category;
            product.Discount = updatedData.Discount ?? product.Discount;
            product.ImageURLs = updatedData.ImageURLs.Count == 0 ? product.ImageURLs : updatedData.ImageURLs;
            product.IsActive = updatedData.IsActive ?? product.IsActive;
            product.Rating = updatedData.Rating ?? product.Rating;
            product.Tags = updatedData.Tags.Count == 0 ? product.Tags : updatedData.Tags;
            product.Name = updatedData.Name ?? product.Name;

            await _dbContext.SaveChangesAsync();

            var productDto = GenerateProductDto(product);

            _logger.LogInformation(GlobalConstants.ProductUpdated);
            return ServiceResult<ProductDto>.Success(productDto, GlobalConstants.ProductUpdated);
        }

        public async Task<ServiceResult<ProductDto>> DeleteProduct(string id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                _logger.LogError(GlobalConstants.ProductDoesNotExist);
                return ServiceResult<ProductDto>.Failure(GlobalConstants.ProductDoesNotExist);
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(GlobalConstants.ProductDeleted);

            return ServiceResult<ProductDto>.Success(null!, string.Empty);
        }
    }
}
