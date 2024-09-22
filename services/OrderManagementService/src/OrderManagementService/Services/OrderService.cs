using Microsoft.EntityFrameworkCore;
using OrderManagementService.Services.Contracts;
using System.Globalization;

namespace OrderManagementService.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderManagementDbContext _context;

        public OrderService(OrderManagementDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<OrderDto>> Create(CreateOrderDto orderDetails, string username)
        {
            if (!_context.Users.Any(x => x.Username == username))
            {
                return ServiceResult<OrderDto>.Failure("No such user exists!");
            }

            if (!orderDetails.Products.All(x => _context.Products.Any(y => y.ProductId == x.ProductId)))
            {
                return ServiceResult<OrderDto>.Failure($"Products with IDs {string.Join(", ", orderDetails.Products.Where(x => !_context.Products.Any(y => y.ProductId == x.ProductId)).Select(x => x.ProductId))} do not exist!");
            }

            if (!orderDetails.Products.All(x => x.Quantity <= _context.Products.First(y => y.ProductId == x.ProductId).Quantity))
            {
                return ServiceResult<OrderDto>.Failure($"Products with IDs {string.Join(", ", orderDetails.Products.Where(x => x.Quantity > _context.Products.First(y => y.ProductId == x.ProductId).Quantity).Select(x => x.ProductId))} do not have sufficient quantity!");
            }

            if (!DateTime.TryParseExact(orderDetails.PaymentDetails.ExpiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expiryDate))
            {
                return ServiceResult<OrderDto>.Failure("Invalid expiry date!");
            }

            if (!Enum.TryParse<PaymentOption>(orderDetails.PaymentOption, true, out PaymentOption paymentOption))
            {
                return ServiceResult<OrderDto>.Failure("Invalid payment option!");
            }

            expiryDate = DateTime.SpecifyKind(expiryDate, DateTimeKind.Utc);

            User user = _context.Users.First(x => x.Username == username);

            List<OrderProduct> products = new List<OrderProduct>();

            foreach (CreateOrderProductDto product in orderDetails.Products)
            {
                Product dbProduct = _context.Products.First(x => x.ProductId == product.ProductId);

                products.Add(new OrderProduct()
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                    Name = dbProduct.Name,
                    Price = (decimal)dbProduct.Price
                });
            }

            Order order = new Order()
            {
                CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                Id = Guid.NewGuid().ToString(),
                Notes = orderDetails.Notes,
                OrderNumber = GenerateOrderMumber(),
                PaymentDetails = new PaymentDetails()
                {
                    Id = Guid.NewGuid().ToString(),
                    CardHolder = orderDetails.PaymentDetails.CardHolder,
                    CardNumber = orderDetails.PaymentDetails.CardNumber,
                    CVC = orderDetails.PaymentDetails.CVC,
                    ExpiryDate = expiryDate
                },
                PaymentOption = paymentOption,
                ShippingAddress = orderDetails.ShippinAddress,
                Status = OrderStatus.Pending,
                UpdatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                UserId = user.UserId,
                Products = products,
                TotalAmount = products.Sum(x => x.Quantity * x.Price)
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            List<OrderProductDto> productsDto = new List<OrderProductDto>();

            foreach (OrderProduct product in order.Products)
            {
                productsDto.Add(new OrderProductDto()
                {
                    Id= product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity
                });
            }

            OrderDto orderDto = new OrderDto()
            {
                CreatedAt = order.CreatedAt.ToString("yyyy-MM-dd"),
                Id = order.Id,
                Notes = order.Notes,
                OrderNumber = order.OrderNumber,
                PaymentDetails = new PaymentDetailsDto()
                {
                    CardHolder = order.PaymentDetails.CardHolder,
                    CardNumber = order.PaymentDetails.CardNumber,
                    CVC = order.PaymentDetails.CVC,
                    ExpiryDate = order.PaymentDetails.ExpiryDate.ToString("MM-yy"),
                    Id = order.PaymentDetails.Id
                },
                PaymentOption = order.PaymentOption.ToString(),
                Products = productsDto,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                Status = order.Status.ToString(),
                UpdatedAt = order.UpdatedAt.ToString("yyyy-MM-dd"),
                UserId = order.UserId
            };

            return ServiceResult<OrderDto>.Success(orderDto, $"Order {order.OrderNumber} created successfully for user {user.Username}!");
        }

        private string GenerateOrderMumber()
        {
            string lastOrderNumber = _context.Orders.Any() ? _context.Orders.Last().OrderNumber : "0";

            ulong digits = ulong.Parse(lastOrderNumber);
            digits++;

            return digits.ToString("D19");
        }

        public async Task HandleProductCreatedEvent(ProductCreatedEvent productCreatedEvent)
        {
            var product = new Product
            {
                Category = productCreatedEvent.Category,
                CreationDate = productCreatedEvent.CreationDate,
                Description = productCreatedEvent.Description,
                Discount = productCreatedEvent.Discount,
                ImageURLs = productCreatedEvent.ImageURLs,
                IsActive = productCreatedEvent.IsActive,
                Name = productCreatedEvent.Name,
                Price = productCreatedEvent.Price,
                ProductId = productCreatedEvent.ProductId,
                Quantity = productCreatedEvent.Quantity,
                Rating = productCreatedEvent.Rating,
                SKU = productCreatedEvent.SKU,
                Tags = productCreatedEvent.Tags,
                UpdatedDate = productCreatedEvent.UpdatedDate
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task HandleUserCreatedEvent(UserCreatedEvent userCreatedEvent)
        {
            var user = new User
            {
                City = userCreatedEvent.City,
                Country = userCreatedEvent.Country,
                DateOfBirth = userCreatedEvent.DateOfBirth,
                Email = userCreatedEvent.Email,
                FirstName = userCreatedEvent.FirstName,
                LastName = userCreatedEvent.LastName,
                LoyaltyPoints = userCreatedEvent.LoyaltyPoints,
                MembershipLevel = userCreatedEvent.MembershipLevel,
                PhoneNumber = userCreatedEvent.PhoneNumber,
                PostalCode = userCreatedEvent.PostalCode,
                PreferredCurrency = userCreatedEvent.PreferredCurrency,
                PreferredLanguage = userCreatedEvent.PreferredLanguage,
                Role = userCreatedEvent.Role,
                State = userCreatedEvent.State,
                Street = userCreatedEvent.Street,
                UserId = userCreatedEvent.UserId,
                Username = userCreatedEvent.Username
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<ServiceResult<OrderDto>> GetOrderById(string id, string username)
        {
            if (!_context.Users.Any(x => x.Username == username))
            {
                return ServiceResult<OrderDto>.Failure("No such user exists!");
            }

            if (!_context.Orders.Any(x => x.Id == id))
            {
                return ServiceResult<OrderDto>.Failure("No such order exists!");
            }

            if (_context.Users.First(x => x.Username == username).UserId != _context.Orders.First(x => x.Id == id).UserId)
            {
                return ServiceResult<OrderDto>.Failure("No such order exists!");
            }

            Order order = await _context.Orders.Include(x => x.PaymentDetails).Include(x => x.Products).FirstAsync(x => x.Id == id);
            List<OrderProductDto> products = new List<OrderProductDto>();

            foreach (OrderProduct product in order.Products)
            {
                products.Add(new OrderProductDto()
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity
                });
            }

            OrderDto orderDto = new OrderDto()
            {
                CreatedAt = order.CreatedAt.ToString("yyyy-MM-dd"),
                Id = order.Id,
                Notes = order.Notes,
                OrderNumber = order.OrderNumber,
                PaymentDetails = new PaymentDetailsDto()
                {
                    CardHolder = order.PaymentDetails.CardHolder,
                    CardNumber = order.PaymentDetails.CardNumber,
                    CVC = order.PaymentDetails.CVC,
                    ExpiryDate = order.PaymentDetails.ExpiryDate.ToString("yyyy-MM-dd"),
                    Id = order.PaymentDetails.Id
                },
                PaymentOption = order.PaymentOption.ToString(),
                Products = products,
                ShippingAddress = order.ShippingAddress,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                UpdatedAt = order.UpdatedAt.ToString("yyyy-MM-dd"),
                UserId = order.UserId
            };

            return ServiceResult<OrderDto>.Success(orderDto, "Order retrieved successfully!");
        }

        public async Task<ServiceResult<List<OrderDto>>> GetOrders(string username)
        {
            if (!_context.Users.Any(x => x.Username == username))
            {
                return ServiceResult<List<OrderDto>>.Failure("No such user exists!");
            }

            if (!_context.Orders.Any(x => x.UserId == _context.Users.First(y => y.Username == username).UserId))
            {
                return ServiceResult<List<OrderDto>>.Failure($"The user {username} has no orders!");
            }

            List<Order> orders = await _context.Orders.Include(x => x.PaymentDetails).Include(x => x.Products).Where(x => x.UserId == _context.Users.First(y => y.Username == username).UserId).ToListAsync();
            List<OrderDto> orderDtos = new List<OrderDto>();

            foreach (Order order in orders)
            {
                List<OrderProductDto> productDtos = new List<OrderProductDto>();

                foreach (OrderProduct product in order.Products)
                {
                    productDtos.Add(new OrderProductDto()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        ProductId = product.ProductId,
                        Quantity = product.Quantity
                    });
                }

                orderDtos.Add(new OrderDto()
                {
                    CreatedAt = order.CreatedAt.ToString("yyyy-MM-dd"),
                    Id = order.Id,
                    Notes = order.Notes,
                    OrderNumber = order.OrderNumber,
                    PaymentDetails = new PaymentDetailsDto()
                    {
                        CardHolder = order.PaymentDetails.CardHolder,
                        CardNumber = order.PaymentDetails.CardNumber,
                        CVC = order.PaymentDetails.CVC,
                        ExpiryDate = order.PaymentDetails.ExpiryDate.ToString("yyyy-MM-dd"),
                        Id = order.PaymentDetails.Id
                    },
                    PaymentOption = order.PaymentOption.ToString(),
                    Products = productDtos,
                    ShippingAddress = order.ShippingAddress,
                    Status = order.Status.ToString(),
                    TotalAmount = order.TotalAmount,
                    UpdatedAt = order.UpdatedAt.ToString("yyyy-MM-dd"),
                    UserId = order.UserId
                });
            }

            return ServiceResult<List<OrderDto>>.Success(orderDtos, "Orders retrieved successfully!");
        }
    }
}
