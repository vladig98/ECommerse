namespace UserManagementService.Helpers
{
    public static class GlobalConstants
    {
        #region Private Constants

        // Logging tokens
        private const string LoggingFormat = "{0} {1}-{2} {0}\r\n{3}\r\n{4}";
        private const string LoggingSeparator = "====================";

        // Logging Levels
        private const string LogLevelError = "ERROR";
        private const string LogLevelWarning = "WARN";
        private const string LogLevelInfo = "INFO";
        private const string LogLevelTrace = "TRACE";
        private const string LogLevelDebug = "DEBUG";

        // Misc
        private const string LoggingTimeFormat = "yyyy-MMM-dd HH:mm:ss.fff";

        #endregion

        #region Public Constants

        // Headers
        public const string KafkaHeader = "Kafka";

        // Messaging Queue Constnats
        public const string KafkaHost = "localhost:9092";
        public const string KafkaTopic = "UserManagementService";
        public const string UserCreatedKey = "UserCreated";

        // Failure Messages
        public const string RegistrationFailed = "Registration failed.";
        public const string PasswordsDoNotMatch = "Password and Confirm Password do not match!";
        public const string UsernameAlreadyExists = "User with this username {0} already exists!";
        public const string EmailAlreadyExists = "User with this email address {0} already exists!";
        public const string PasswordsDoNotMeetRequirements = "{0}";
        public const string FailedLogin = "Login failed!";
        public const string WrongCredentials = "Invalid username or password.";
        public const string InvalidData = "The provided data was invalid or did not exist (null) -> {0}.";
        public const string UserNotFound = "User {0} does not exist!";
        public const string UserEnteredWrongPassword = "Incorrect password for user {0}!";
        public const string InvalidConnectionString = "Connection string not found.";
        public const string Failure = "Failure";
        public const string KafkaEventFailure = "Event was not delivered! Topic: {0}, Key: {1}, Value: {2}, Reason: '{3}'";

        // Warnings
        public const string KafkaEventDeliveredButNotAcknowledged = "Event was delivered but not acknowledged! Topic: {0}, Key: {1}, Value: {2}";

        // Success Messages
        public const string UserRetrieved = "User {0} retrieved successfully!";
        public const string UserUpdated = "User {0} updated successfully!";
        public const string UserCreatedSuccessfully = "User {0} created successfully!";
        public const string UserLoggedInSuccessfully = "User {0} logged in successfully!";
        public const string JWTTokenSucces = "Token generated for user {0}";
        public const string Success = "Success";
        public const string KafkaEventDelivered = "Event was successfully delivered! Topic: {0}, Key: {1}, Value: {2}";

        // Configurations
        public const string JWT = "JWT";
        public const string LoginProvider = "Ecoomerse-Vladi";
        public const string JWTIssuer = "UserManagement:JWT:Issuer";
        public const string JWTKey = "UserManagement:JWT:Key";
        public const string ConnectionString = "ConnectionStrings:PostgreSQL";

        // Misc
        public const string DateTimeFormat = "dd/MM/yyyy";
        public const string CommaSeparator = ", ";
        public const string DefaultRole = "User";

        #endregion

        private static string GenerateLogMessage(string level, string header, string message)
        {
            return string.Format(LoggingFormat, LoggingSeparator, DateTime.UtcNow.ToString(LoggingTimeFormat), level, header, message);
        }

        #region Logging methods

        public static string LogError(string header, string message)
        {
            return GenerateLogMessage(LogLevelError, header, message);
        }

        public static string LogInfo(string header, string message)
        {
            return GenerateLogMessage(LogLevelInfo, header, message);
        }

        public static string LogWarning(string header, string message)
        {
            return GenerateLogMessage(LogLevelWarning, header, message);
        }

        public static string LogTrace(string header, string message)
        {
            return GenerateLogMessage(LogLevelTrace, header, message);
        }

        public static string LogDebug(string header, string message)
        {
            return GenerateLogMessage(LogLevelDebug, header, message);
        }
        #endregion
    }
}
