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

        // Messaging Queue Constnats
        public const string RabbitMQHostName = "localhost";

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

        // Success Messages
        public const string UserRetrieved = "User {0} retrieved successfully!";
        public const string UserUpdated = "User {0} updated successfully!";
        public const string UserCreatedSuccessfully = "User {0} created successfully!";
        public const string UserLoggedInSuccessfully = "User {0} logged in successfully!";
        public const string JWTTokenSucces = "Token generated for user {0}";
        public const string Success = "Success";

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
            return string.Format(LoggingFormat, LoggingSeparator, DateTime.Now.ToString(LoggingTimeFormat), level, header, message);
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
