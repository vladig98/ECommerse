using Microsoft.EntityFrameworkCore;

namespace OrderManagementService.Tests
{
    public class OrderManagementUnitTests
    {
        private DbContextOptions<OrderManagementDbContext> GetOptions()
        {
            return new DbContextOptionsBuilder<OrderManagementDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        }

        private User GetValidUser()
        {
            return new User()
            {
                City = "City",
                Country = "Country",
                DateOfBirth = "11-11-2024",
                Email = "test@gmail.com",
                FirstName = "John",
                Id = "1",
                LastName = "Doe",
                LoyaltyPoints = 0,
                MembershipLevel = "Gold",
                PhoneNumber = "1234567890",
                PostalCode = "ddsada",
                PreferredCurrency = "EUR",
                PreferredLanguage = "en",
                Role = "User",
                State = "state",
                Street = "street",
                UserId = "1",
                Username = "JohnDoe"
            };
        }

        private Product GetValidProduct()
        {
            return new Product()
            {
                Category = "food",
                CreationDate = DateTime.Now,
                Description = "Description",
                Discount = 0,
                Id = "1",
                ImageURLs = new List<string>()
                {
                    "link1",
                    "link2"
                },
                IsActive = true,
                Name = "Name",
                Price = 2.99,
                ProductId = "1",
                Quantity = 100,
                Rating = 5,
                SKU = "1",
                Tags = new List<string>()
                {
                    "food",
                    "food2"
                },
                UpdatedDate = DateTime.Now
            };
        }

        private CreateOrderDto GetValidCreateOrderData()
        {
            return new CreateOrderDto()
            {
                Notes = "Please leave the package at the front door!",
                PaymentDetails = new CreatePaymentDto()
                {
                    CardHolder = "John Doe",
                    CardNumber = "1234 1234 1234 1234",
                    CVC = "123",
                    ExpiryDate = "11-24"
                },
                PaymentOption = "CreditCard",
                Products = new List<CreateOrderProductDto>()
                {
                    new CreateOrderProductDto()
                    {
                        ProductId = "1",
                        Quantity = 10
                    }
                },
                ShippinAddress = "First Street",
                UserId = "1"
            };
        }

        [Fact]
        public async Task CreateOrdder_ValidData_ShouldPass()
        {
            using OrderManagementDbContext context = new OrderManagementDbContext(GetOptions());

            // Add user and product to the DB
            context.Users.Add(GetValidUser());
            context.Products.Add(GetValidProduct());
            context.SaveChanges();

            CreateOrderDto createOrder = GetValidCreateOrderData();

            // Call the service to generate the order
            OrderService orderService = new OrderService(context);
            ServiceResult<OrderDto> result = await orderService.Create(createOrder, "JohnDoe");

            Assert.Equal($"Order {1:d19} created successfully for user JohnDoe!", result.Message);
            Assert.True(result.Succeeded);
            Assert.Equal(1, context.Orders.Count());
            Assert.Equal(createOrder.Notes, result.Data.Notes);
        }
    }
}