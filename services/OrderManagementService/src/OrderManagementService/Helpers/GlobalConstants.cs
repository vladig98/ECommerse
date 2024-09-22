namespace OrderManagementService.Helpers
{
    public static class GlobalConstants
    {
        private const string LoggingFormat = "{0} {1} {0}\r\n{2}\r\n{3}";
        private const string LoggingSeparator = "====================";

        private const string LogLevelError = "ERROR";
        private const string LogLevelWarning = "WARN";
        private const string LogLevelInfo = "INFO";
        private const string LogLevelTrace = "TRACE";
        private const string LogLevelDebug = "DEBUG";

        public const string JWTIssuer = "OrderManagement:JWT:Issuer";
        public const string JWTKey = "OrderManagement:JWT:Key";
        public const string BearerFormat = "JWT";
        public const string JWTSchemeName = "JWT Authentication";
        public const string JWTSchemeDescription = "Put **_ONLY_** your JWT Bearer token on textbox below!";

        public const string ConnectionString = "ConnectionStrings:PostgreSQL";
        public const string InvalidConnectionString = "Connection string not found.";

        public const string RabbitMQHostName = "localhost";
        public const string RabbitMQUserCreatedEventName = "UserCreatedEvent";
        public const string RabbitMQProductCreatedEventName = "ProductCreatedEvent";

        public const string SubClaim = "sub";
        public const string BearerString = "Bearer ";
        public const int JWTIndex = 1;

        public const string CommaSeparator = ", ";
        public const string DateFormat = "yyyy-MM-dd";
        public const string DateFormatCardExpiry = "MM-yy";

        public const string StartingOrderNumber = "0";
        public const string OrderNumberLength = "D19";

        public const string NoUserFound = "No such user exists!";
        public const string NoSuchProducts = "Products with IDs {0} do not exist!";
        public const string ProductsOutOfQuantity = "Products with IDs {0} do not have sufficient quantity!";
        public const string ExpiryDateInvalid = "Invalid expiry date!";
        public const string InvalidPaymentOption = "Invalid payment option!";
        public const string OrderCreated = "Order {0} created successfully for user {1}!";
        public const string OrderDoesNotExist = "No such order exists!";
        public const string OrderFoundAndReturned = "Order retrieved successfully!";
        public const string OrddersFound = "Orders retrieved successfully!";
        public const string UserHasNoOrders = "The user {0} has no orders!";

        private static string GenerateLogMessage(string level, string header, string message)
        {
            return string.Format(LoggingFormat, LoggingSeparator, level, header, message);
        }

        private static string LogError(string header, string message)
        {
            return GenerateLogMessage(LogLevelError, header, message);
        }

        private static string LogInfo(string header, string message)
        {
            return GenerateLogMessage(LogLevelInfo, header, message);
        }

        private static string LogWarning(string header, string message)
        {
            return GenerateLogMessage(LogLevelWarning, header, message);
        }

        private static string LogTrace(string header, string message)
        {
            return GenerateLogMessage(LogLevelTrace, header, message);
        }

        private static string LogDebug(string header, string message)
        {
            return GenerateLogMessage(LogLevelDebug, header, message);
        }
    }
}
