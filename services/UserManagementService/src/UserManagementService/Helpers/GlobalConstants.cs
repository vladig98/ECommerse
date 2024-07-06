namespace UserManagementService.Helpers
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

        private const string RegistrationFailed = "Registration failed.";

        public const string RabbitMQHostName = "localhost";

        public static string PasswordsDoNotMatch = LogError(RegistrationFailed, "Password and Confirm Password do not match!");
        public static string UsernameAlreadyExists = LogError(RegistrationFailed, "User with this username already exists!");
        public static string EmailAlreadyExists = LogError(RegistrationFailed, "User with this email address already exists!");
        public static string PasswordsDoNotMeetRequirements = LogError(RegistrationFailed, "{0}");
        public static string UserCreatedSuccessfully = LogInfo("Registration completed successfully.", "User {0} created successfully!");

        public const string FailedLogin = "Login failed!";
        public static string UserNotFound = LogError(FailedLogin, "User does not exist!");
        public static string UserEnteredWrongPassword = LogError(FailedLogin, "Incorrect password!");
        public static string UserLoggedInSuccessfully = LogInfo("Login Successful", "User {0} logged in successfully!");

        public static string UserRetrieved = LogInfo("User found!", "User {0} retrieved successfully!");
        public static string UserUpdated = LogInfo("User updated!", "User {0} updated successfully!");

        public static string JWTTokenSucces = LogInfo("JWT generated", "Token generated for user {0}");

        public const string JWT = "JWT";
        public const string LoginProvider = "Ecoomerse-Vladi";

        public const string JWTIssuer = "UserManagement:JWT:Issuer";
        public const string JWTKey = "UserManagement:JWT:Key";
        public const string ConnectionString = "ConnectionStrings:PostgreSQL";
        public const string InvalidConnectionString = "Connection string not found.";

        public const string DateTimeFormat = "dd/MM/yyyy";
        public const string CommaSeparator = ", ";

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
