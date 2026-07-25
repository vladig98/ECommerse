namespace ShoppingCartService.Helpers
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

        public const string ConnectionString = "ConnectionStrings:PostgreSQL";
        public const string InvalidConnectionString = "Connection string not found.";

        public const string RabbitMQHostName = "localhost";
        public const string RabbitMQUserCreatedEventName = "UserCreatedEvent";
        public const string RabbitMQProductCreatedEventName = "ProductCreatedEvent";

        public const string UserDoesNotExist = "User does not exist!";
        public const string ProductDoesNotExist = "Product does not exist!";
        public const string ProductNotEnough = "The product does not have enough quantity to add to the cart!";
        public const string CartNotExist = "The has not added any products to the cart!";
        public const string CartFound = "The cart was retrieved successfully!";
        public const string ProductNotFound = "This product is not in the cart!";
        public const string ProductDeleted = "The product was removed from the cart!";
        public const string ProductAddedSuccessfully = "Product added to cart!";

        public const string DateFormat = "dd/MM/yyyy";

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
