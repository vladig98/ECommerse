namespace ProductCatalogService.Helpers
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

        private const string InvalidDate = "Invalid date!";
        public static string InvalidCreatedDate = LogError(InvalidDate, "Created Date is invalid!");
        public static string InvalidUpdatedDate = LogError(InvalidDate, "Updated Date is invalid!");
        public static string ProductAlredyExists = LogError("Invalid name!", "The product already exists!");
        public static string ProductDoesNotExist = LogError("Invalid product id!", "The product cannot be found!");
        public static string InvalidCategory = LogError("Invalid category!", "The provided category is invalid!");
        public static string InvalidName = LogError("Invalid name!", "Product with the provided name already exists!");

        public static string ProductCreated = LogInfo("Product created!", "Product created successfully!");
        public static string ProductRetrieved = LogInfo("Product retrieved!", "Product retrieved successfully!");
        public static string AllProductsRetrieved = LogInfo("Products retrieved!", "All products were retrieved successfully!");
        public static string ProductUpdated = LogInfo("Product updated!", "The product was updated successfully!");
        public static string ProductDeleted = LogInfo("Product deleted!", "The product was deleted successfully!");

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
