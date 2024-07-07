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

        public const string ConnectionString = "ConnectionStrings:PostgreSQL";
        public const string InvalidConnectionString = "Connection string not found.";

        public const string RabbitMQHostName = "localhost";
        public const string RabbitMQUserCreatedEventName = "UserCreatedEvent";
        public const string RabbitMQProductCreatedEventName = "ProductCreatedEvent";

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
