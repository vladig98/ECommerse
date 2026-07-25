using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductCatalogService.DTOs;
using ProductCatalogService.Events;
using System;

namespace ProductCatalogService.Tests
{
    public class ProductCataloguesUnitTests
    {
        private DbContextOptions<ProductsDbContext> GetOptions()
        {
            return new DbContextOptionsBuilder<ProductsDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        }

        private CreateProductDto GetProductData()
        {
            var createProductDto = new CreateProductDto
            {
                Category = "Food",
                CreationDate = "01/01/2024",
                Description = "This is a test product!",
                Discount = 10.0,
                ImageURLs = new List<string> { "https://google.com/", "https://youtube.com/" },
                IsActive = true,
                Name = "Test product",
                Price = 19.99,
                Quantity = 3,
                Rating = 10.0,
                SKU = "321312321",
                Tags = new List<string> { "tag1", "tag2", "tag3" },
                UpdatedDate = "01/02/2024"
            };

            return createProductDto;
        }

        private ProductDto GetProductDto()
        {
            var productDto = new ProductDto
            {
                Category = "Food",
                CreationDate = "01/01/2024",
                Description = "This is a test product!",
                Discount = 10.0,
                ImageURLs = new List<string> { "https://google.com/", "https://youtube.com/" },
                IsActive = true,
                Name = "Test product",
                Price = 19.99,
                Quantity = 3,
                Rating = 10.0,
                SKU = "321312321",
                Tags = new List<string> { "tag1", "tag2", "tag3" },
                UpdatedDate = "01/02/2024"
            };

            return productDto;
        }

        private ProductService CreateProductService(ProductsDbContext context)
        {
            var mockedLogger = new Mock<ILogger<ProductService>>();
            var eventBus = new Mock<EventBusRabbitMQ>();
            var productsService = new ProductService(context, mockedLogger.Object, eventBus.Object);

            return productsService;
        }

        [Fact]
        public async Task CreateProduct_CreateValidData_ShouldPass()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();
                var productDto = GetProductDto();

                var result = await productsService.CreateProduct(createProductDto);

                Assert.Equal(1, context.Products.Count());
                Assert.Equal(GlobalConstants.ProductCreated, result.Message);
                Assert.Equal(productDto.Category, result.Data.Category);
                Assert.Equal(productDto.CreationDate, result.Data.CreationDate);
                Assert.Equal(productDto.Description, result.Data.Description);
                Assert.Equal(productDto.ImageURLs, result.Data.ImageURLs);
                Assert.Equal(productDto.IsActive, result.Data.IsActive);
                Assert.Equal(productDto.Name, result.Data.Name);
                Assert.Equal(productDto.Price, result.Data.Price);
                Assert.Equal(productDto.Quantity, result.Data.Quantity);
                Assert.Equal(productDto.Rating, result.Data.Rating);
                Assert.Equal(productDto.SKU, result.Data.SKU);
                Assert.Equal(productDto.Tags, result.Data.Tags);
                Assert.Equal(productDto.UpdatedDate, result.Data.UpdatedDate);
            }
        }

        [Fact]
        public async Task CreateProduct_CreateExistingName_ShouldFail()
        {
            using(var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);
                var result2 = await productsService.CreateProduct(createProductDto);

                Assert.Equal(1, context.Products.Count());
                Assert.Equal(GlobalConstants.ProductAlredyExists, result2.Message);
                Assert.Null(result2.Data);
            }
        }

        [Fact]
        public async Task CreateProduct_CreateInvalidCreationDate_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                createProductDto.CreationDate = "";

                var result = await productsService.CreateProduct(createProductDto);

                Assert.Equal(0, context.Products.Count());
                Assert.Equal(GlobalConstants.InvalidCreatedDate, result.Message);
                Assert.Null(result.Data);
            }
        }

        [Fact]
        public async Task CreateProduct_CreateInvalidUpdatedDate_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                createProductDto.UpdatedDate = "";

                var result = await productsService.CreateProduct(createProductDto);

                Assert.Equal(0, context.Products.Count());
                Assert.Equal(GlobalConstants.InvalidUpdatedDate, result.Message);
                Assert.Null(result.Data);
            }
        }

        [Fact]
        public async Task CreateProduct_CreateInvalidCategory_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                createProductDto.Category = "";

                var result = await productsService.CreateProduct(createProductDto);

                Assert.Equal(0, context.Products.Count());
                Assert.Equal(GlobalConstants.InvalidCategory, result.Message);
                Assert.Null(result.Data);
            }
        }

        [Fact]
        public async Task GetAllProducts_Valid_ShouldPass()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                for (int i = 0; i < 10; i++)
                {
                    createProductDto.Name += i;
                    await productsService.CreateProduct(createProductDto);
                }

                var result = await productsService.GetAllProducts();

                Assert.Equal(10, result.Data.Products.Count);
            }
        }

        [Fact]
        public async Task GetAllProducts_NoProducts_ShouldPass()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var result = await productsService.GetAllProducts();

                Assert.Empty(result.Data.Products);
            }
        }

        [Fact]
        public async Task GetProduct_Valid_ShouldPass()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);

                var productId = result.Data.Id;

                var getProductResult = await productsService.GetProduct(productId);

                var productDto = GetProductDto();

                Assert.Equal(productDto.Category, getProductResult.Data.Category);
                Assert.Equal(productDto.CreationDate, getProductResult.Data.CreationDate);
                Assert.Equal(productDto.Description, getProductResult.Data.Description);
                Assert.Equal(productDto.ImageURLs, getProductResult.Data.ImageURLs);
                Assert.Equal(productDto.IsActive, getProductResult.Data.IsActive);
                Assert.Equal(productDto.Name, getProductResult.Data.Name);
                Assert.Equal(productDto.Price, getProductResult.Data.Price);
                Assert.Equal(productDto.Quantity, getProductResult.Data.Quantity);
                Assert.Equal(productDto.Rating, getProductResult.Data.Rating);
                Assert.Equal(productDto.SKU, getProductResult.Data.SKU);
                Assert.Equal(productDto.Tags, getProductResult.Data.Tags);
                Assert.Equal(productDto.UpdatedDate, getProductResult.Data.UpdatedDate);
            }
        }

        [Fact]
        public async Task GetProduct_InvalidId_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);

                var getProductResult = await productsService.GetProduct("1");

                Assert.Equal(GlobalConstants.ProductDoesNotExist, getProductResult.Message);
                Assert.Null(getProductResult.Data);
            }
        }

        [Fact]
        public async Task GetProduct_NoProducts_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var getProductResult = await productsService.GetProduct("1");

                Assert.Equal(GlobalConstants.ProductDoesNotExist, getProductResult.Message);
                Assert.Null(getProductResult.Data);
            }
        }

        [Fact]
        public async Task DeleteProduct_Valid_ShouldPass()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);

                var productId = result.Data.Id;

                var getProductResult = await productsService.DeleteProduct(productId);

                var productDto = GetProductDto();

                Assert.Empty(context.Products);
            }
        }

        [Fact]
        public async Task DeleteProduct_InvalidId_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);

                var getProductResult = await productsService.DeleteProduct("1");

                Assert.Equal(GlobalConstants.ProductDoesNotExist, getProductResult.Message);
                Assert.Null(getProductResult.Data);
            }
        }

        [Fact]
        public async Task DeleteProduct_NoProducts_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var getProductResult = await productsService.DeleteProduct("1");

                Assert.Equal(GlobalConstants.ProductDoesNotExist, getProductResult.Message);
                Assert.Null(getProductResult.Data);
            }
        }

        [Fact]
        public async Task UpdateProduct_Valid_ShouldPass()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);

                var updatedData = new EditProductDto
                {
                    Category = "Food",
                    CreationDate = "01/01/2024",
                    Description = "This is a test product!",
                    Discount = 10.0,
                    ImageURLs = new List<string> { "https://google.com/", "https://youtube.com/" },
                    IsActive = true,
                    Name = "Test product",
                    Price = 19.99,
                    Quantity = 3,
                    Rating = 10.0,
                    SKU = "321312321",
                    Tags = new List<string> { "tag1", "tag2", "tag3" },
                    UpdatedDate = "01/02/2024"
                };

                var productId = result.Data.Id;
                var updateResult = await productsService.UpdateProduct(productId, updatedData);

                var productDto = GetProductDto();

                Assert.Equal(GlobalConstants.ProductUpdated, updateResult.Message);
                Assert.Equal(productDto.Category, updateResult.Data.Category);
                Assert.Equal(productDto.CreationDate, updateResult.Data.CreationDate);
                Assert.Equal(productDto.Description, updateResult.Data.Description);
                Assert.Equal(productDto.ImageURLs, updateResult.Data.ImageURLs);
                Assert.Equal(productDto.IsActive, updateResult.Data.IsActive);
                Assert.Equal(productDto.Name, updateResult.Data.Name);
                Assert.Equal(productDto.Price, updateResult.Data.Price);
                Assert.Equal(productDto.Quantity, updateResult.Data.Quantity);
                Assert.Equal(productDto.Rating, updateResult.Data.Rating);
                Assert.Equal(productDto.SKU, updateResult.Data.SKU);
                Assert.Equal(productDto.Tags, updateResult.Data.Tags);
                Assert.Equal(productDto.UpdatedDate, updateResult.Data.UpdatedDate);
            }
        }

        [Fact]
        public async Task UpdateProduct_InvalidId_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);

                var updatedData = new EditProductDto
                {
                    Category = "Food",
                    CreationDate = "01/01/2024",
                    Description = "This is a test product!",
                    Discount = 10.0,
                    ImageURLs = new List<string> { "https://google.com/", "https://youtube.com/" },
                    IsActive = true,
                    Name = "Test product",
                    Price = 19.99,
                    Quantity = 3,
                    Rating = 10.0,
                    SKU = "321312321",
                    Tags = new List<string> { "tag1", "tag2", "tag3" },
                    UpdatedDate = "01/02/2024"
                };

                var updateResult = await productsService.UpdateProduct("1", updatedData);

                Assert.Equal(GlobalConstants.ProductDoesNotExist, updateResult.Message);
                Assert.Null(updateResult.Data);
            }
        }

        [Fact]
        public async Task UpdateProduct_InvalidName_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);

                createProductDto.Name = "testName2";
                await productsService.CreateProduct(createProductDto);

                var updatedData = new EditProductDto
                {
                    Category = "Food",
                    CreationDate = "01/01/2024",
                    Description = "This is a test product!",
                    Discount = 10.0,
                    ImageURLs = new List<string> { "https://google.com/", "https://youtube.com/" },
                    IsActive = true,
                    Name = "testName2",
                    Price = 19.99,
                    Quantity = 3,
                    Rating = 10.0,
                    SKU = "321312321",
                    Tags = new List<string> { "tag1", "tag2", "tag3" },
                    UpdatedDate = "01/02/2024"
                };

                var productId = result.Data.Id;
                var updateResult = await productsService.UpdateProduct(productId, updatedData);

                Assert.Equal(GlobalConstants.InvalidName, updateResult.Message);
                Assert.Null(updateResult.Data);
            }
        }

        [Fact]
        public async Task UpdateProduct_InvalidCreatedDate_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);

                var updatedData = new EditProductDto
                {
                    Category = "Food",
                    CreationDate = "",
                    Description = "This is a test product!",
                    Discount = 10.0,
                    ImageURLs = new List<string> { "https://google.com/", "https://youtube.com/" },
                    IsActive = true,
                    Name = "Test product",
                    Price = 19.99,
                    Quantity = 3,
                    Rating = 10.0,
                    SKU = "321312321",
                    Tags = new List<string> { "tag1", "tag2", "tag3" },
                    UpdatedDate = "01/02/2024"
                };

                var productDto = GetProductDto();

                var productId = result.Data.Id;
                var updateResult = await productsService.UpdateProduct(productId, updatedData);

                Assert.Equal(productDto.CreationDate, updateResult.Data.CreationDate);
            }
        }

        [Fact]
        public async Task UpdateProduct_InvalidUpdatedDate_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);

                var updatedData = new EditProductDto
                {
                    Category = "Food",
                    CreationDate = "01/01/2024",
                    Description = "This is a test product!",
                    Discount = 10.0,
                    ImageURLs = new List<string> { "https://google.com/", "https://youtube.com/" },
                    IsActive = true,
                    Name = "Test product",
                    Price = 19.99,
                    Quantity = 3,
                    Rating = 10.0,
                    SKU = "321312321",
                    Tags = new List<string> { "tag1", "tag2", "tag3" },
                    UpdatedDate = ""
                };

                var productDto = GetProductDto();

                var productId = result.Data.Id;
                var updateResult = await productsService.UpdateProduct(productId, updatedData);

                Assert.Equal(productDto.UpdatedDate, updateResult.Data.UpdatedDate);
            }
        }

        [Fact]
        public async Task UpdateProduct_InvalidCategory_ShouldFail()
        {
            using (var context = new ProductsDbContext(GetOptions()))
            {
                var productsService = CreateProductService(context);

                var createProductDto = GetProductData();

                var result = await productsService.CreateProduct(createProductDto);

                var updatedData = new EditProductDto
                {
                    Category = "",
                    CreationDate = "01/01/2024",
                    Description = "This is a test product!",
                    Discount = 10.0,
                    ImageURLs = new List<string> { "https://google.com/", "https://youtube.com/" },
                    IsActive = true,
                    Name = "Test product",
                    Price = 19.99,
                    Quantity = 3,
                    Rating = 10.0,
                    SKU = "321312321",
                    Tags = new List<string> { "tag1", "tag2", "tag3" },
                    UpdatedDate = "01/02/2024"
                };

                var productDto = GetProductDto();

                var productId = result.Data.Id;
                var updateResult = await productsService.UpdateProduct(productId, updatedData);

                Assert.Equal(productDto.Category, updateResult.Data.Category);
            }
        }
    }
}