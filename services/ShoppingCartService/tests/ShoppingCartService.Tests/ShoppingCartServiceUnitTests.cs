using Microsoft.EntityFrameworkCore;
using Moq;

namespace ShoppingCartService.Tests
{
    public class ShoppingCartServiceUnitTests
    {
        private DbContextOptions<ShoppingCartDbContext> GetOptions()
        {
            return new DbContextOptionsBuilder<ShoppingCartDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        }

        private CreateCartItemDto GenerateCreateItemDto()
        {
            var createItemDto = new CreateCartItemDto
            {
                UserId = "1",
                ProductId = "1",
                Quantity = 1
            };

            return createItemDto;
        }

        private User GenerateUser()
        {
            var user = new User
            {
                City = "Sofia",
                Country = "Bulgaira",
                DateOfBirth = "01/01/2024",
                Email = "test@gmail.com",
                FirstName = "FirstName",
                LastName = "LastName",
                Id = "tableId",
                LoyaltyPoints = 1,
                MembershipLevel = "Gold",
                PhoneNumber = "555555555",
                PostalCode = "3212SA",
                PreferredCurrency = "EUR",
                PreferredLanguage = "EN",
                Role = "User",
                State = "Sofia",
                Street = "StreetName Bld.",
                UserId = "1",
                Username = "Username"
            };

            return user;
        }

        private Product GenerateProduct()
        {
            var product = new Product
            {
                Category = "Food",
                CreationDate = DateTime.UtcNow,
                Description = "Chocolate waffles.",
                Discount = 0,
                Id = "productId",
                ImageURLs = new List<string> { @"https://google.com/" },
                IsActive = true,
                Name = "Chocolate waffles",
                Price = 1.99,
                ProductId = "1",
                Quantity = 2,
                Rating = 1,
                SKU = "12341241234",
                Tags = new List<string> { "waffle", "chocolate", "food" },
                UpdatedDate = DateTime.UtcNow
            };

            return product;
        }

        private CartDto GenerateCartDto()
        {
            var cartDto = new CartDto
            {
                Id = "1",
                CreatedAt = "01/01/2024",
                UpdatedAt = "01/01/2024",
                UserId = "1",
                Items = new List<CartItemDto>
                {
                    new CartItemDto 
                    {
                        Id = "1",
                        ProductId = "1",
                        Quantity = 1
                    }
                }
            };

            return cartDto;
        }

        [Fact]
        public async Task AddItemToCart_ValidData_ShouldPass()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                Assert.True(result.Succeeded);
                Assert.Equal(GlobalConstants.ProductAddedSuccessfully, result.Message);
            }
        }

        [Fact]
        public async Task AddItemToCart_InvalidUserId_ShouldFail()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();
                cartItem.UserId = "3";

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                Assert.False(result.Succeeded);
                Assert.Equal(GlobalConstants.UserDoesNotExist, result.Message);
            }
        }

        [Fact]
        public async Task AddItemToCart_InvalidProductId_ShouldFail()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();
                cartItem.ProductId = "3";

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                Assert.False(result.Succeeded);
                Assert.Equal(GlobalConstants.ProductDoesNotExist, result.Message);
            }
        }

        [Fact]
        public async Task AddItemToCart_NotEnoughQuantity_ShouldFail()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();
                cartItem.Quantity = 100;

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                Assert.False(result.Succeeded);
                Assert.Equal(GlobalConstants.ProductNotEnough, result.Message);
            }
        }

        [Fact]
        public async Task AddItemToCart_AddExistingItem_ShouldPass()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                cartItem.Quantity = 2;

                var resultSecondTime = await cartService.AddItemToCart(cartItem);

                Assert.True(resultSecondTime.Succeeded);
                Assert.Equal(GlobalConstants.ProductAddedSuccessfully, resultSecondTime.Message);
                Assert.Equal(cartItem.Quantity, context.CartItems.First().Quantity);
            }
        }

        [Fact]
        public async Task GetCart_ValidData_ShouldPass()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                var cartResult = await cartService.GetCart(user.UserId);

                var cartDto = GenerateCartDto();

                Assert.True(cartResult.Succeeded);
                Assert.Equal(GlobalConstants.CartFound, cartResult.Message);
                Assert.Equal(cartDto.UserId, cartResult.Data.UserId);
                Assert.Equal(cartDto.Items.First().ProductId, cartResult.Data.Items.First().ProductId);
            }
        }

        [Fact]
        public async Task GetCart_InvalidUser_ShouldFail()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                var cartResult = await cartService.GetCart("3");

                Assert.False(cartResult.Succeeded);
                Assert.Equal(GlobalConstants.UserDoesNotExist, cartResult.Message);
            }
        }

        [Fact]
        public async Task GetCart_CartDoesNotExist_ShouldFail()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var cartResult = await cartService.GetCart(user.UserId);

                Assert.False(cartResult.Succeeded);
                Assert.Equal(GlobalConstants.CartNotExist, cartResult.Message);
            }
        }

        [Fact]
        public async Task GetCart_CartEmpty_ShouldFail()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                context.CartItems.Remove(context.CartItems.First());
                context.SaveChanges();

                var cartResult = await cartService.GetCart(user.UserId);

                Assert.False(cartResult.Succeeded);
                Assert.Equal(GlobalConstants.CartNotExist, cartResult.Message);
            }
        }

        [Fact]
        public async Task DeleteProductFromCart_ValidData_ShouldPass()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                var removeResult = await cartService.DeleteItemFromCart(user.UserId, product.ProductId);

                Assert.True(removeResult.Succeeded);
                Assert.Equal(GlobalConstants.ProductDeleted, removeResult.Message);
            }
        }

        [Fact]
        public async Task DeleteProductFromCart_InvalidUserId_ShouldFail()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                var removeResult = await cartService.DeleteItemFromCart("3", product.ProductId);

                Assert.False(removeResult.Succeeded);
                Assert.Equal(GlobalConstants.UserDoesNotExist, removeResult.Message);
            }
        }

        [Fact]
        public async Task DeleteProductFromCart_CartDoesNotExist_ShouldFail()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var removeResult = await cartService.DeleteItemFromCart(user.UserId, product.ProductId);

                Assert.False(removeResult.Succeeded);
                Assert.Equal(GlobalConstants.CartNotExist, removeResult.Message);
            }
        }

        [Fact]
        public async Task DeleteProductFromCart_CartEmpty_ShouldFail()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                context.CartItems.Remove(context.CartItems.First());
                context.SaveChanges();

                var removeResult = await cartService.DeleteItemFromCart(user.UserId, product.ProductId);

                Assert.False(removeResult.Succeeded);
                Assert.Equal(GlobalConstants.CartNotExist, removeResult.Message);
            }
        }

        [Fact]
        public async Task DeleteProductFromCart_InvalidProductId_ShouldFail()
        {
            using (var context = new ShoppingCartDbContext(GetOptions()))
            {
                var user = GenerateUser();
                var product = GenerateProduct();

                context.Users.Add(user);
                context.Products.Add(product);

                context.SaveChanges();

                var cartItem = GenerateCreateItemDto();

                var cartRepository = new Mock<CartRepository>(context);
                var cartService = new CartService(cartRepository.Object, context);

                var result = await cartService.AddItemToCart(cartItem);

                var removeResult = await cartService.DeleteItemFromCart(user.UserId, "3");

                Assert.False(removeResult.Succeeded);
                Assert.Equal(GlobalConstants.ProductNotFound, removeResult.Message);
            }
        }
    }
}