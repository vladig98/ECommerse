namespace UserManagementService.Helpers
{
    public static class GlobalConstants
    {
        public const string LogLevelError = "ERROR: ";
        public const string LogLevelWarning = "WARN: ";
        public const string LogLevelInfo = "INFO: ";
        public const string LogLevelTrace = "TRACE: ";

        public const string PasswordsDoNotMatch = LogLevelError + "Registration failed. Password and Confirm Password do not match!";
        public const string UsernameAlreadyExists = LogLevelError + "Registration failed. User with this username already exists!";
        public const string EmailAlreadyExists = LogLevelError + "Registration failed. User with this email address already exists!";
        public const string PasswordsDoNotMeetRequirements = LogLevelError + "Registration failed. {0}!";
        public const string UserCreatedSuccessfully = LogLevelInfo + "Registration completed successfully. User {0} created successfully!";

        public const string DateTimeFormat = "dd/MM/yyyy";
        public const string CommaSeparator = ", ";
    }
}
