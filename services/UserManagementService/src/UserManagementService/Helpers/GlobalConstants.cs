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

        public static string PasswordsDoNotMatch = string.Format(LoggingFormat, LoggingSeparator, LogLevelError, RegistrationFailed, "Password and Confirm Password do not match!");
        public static string UsernameAlreadyExists = string.Format(LoggingFormat, LoggingSeparator, LogLevelError, RegistrationFailed, "User with this username already exists!");
        public static string EmailAlreadyExists = string.Format(LoggingFormat, LoggingSeparator, LogLevelError, RegistrationFailed, "User with this email address already exists!");
        public static string PasswordsDoNotMeetRequirements = string.Format(LoggingFormat, LoggingSeparator, LogLevelError, RegistrationFailed, "{0}");
        public static string UserCreatedSuccessfully = string.Format(LoggingFormat, LoggingSeparator, LogLevelInfo, "Registration completed successfully.", "User {0} created successfully!");

        public const string DateTimeFormat = "dd/MM/yyyy";
        public const string CommaSeparator = ", ";
    }
}
