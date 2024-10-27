namespace UserManagementService.Helpers
{
    public static class GlobalConstants
    {
        // Headers
        public const string KafkaHeader = "Kafka";

        // Messaging Queue Constnats
        public const string KafkaHost = "localhost:9092";
        public const string KafkaTopic = "UserManagementService";
        public const string UserCreatedKey = "UserCreated";

        // Failure Messages
        public const string RegistrationFailed = "Registration failed.";
        public const string PasswordsDoNotMatch = "Password and Confirm Password do not match!";
        public const string PasswordsDoNotMeetRequirements = "{0}";
        public const string InvalidConnectionString = "Connection string not found.";
        public const string EmailAlreadyExists = "User with this email address {0} already exists!";
        public const string Failure = "Failure";
        public const string KafkaEventFailure = "Event was not delivered! Topic: {0}, Key: {1}, Value: {2}, Reason: '{3}'";
        public const string UsernameAlreadyExists = "User with this username {0} already exists!";
        public const string InvalidJWT = "Invalid JWT configuration";

        // Warnings
        public const string KafkaEventDeliveredButNotAcknowledged = "Event was delivered but not acknowledged! Topic: {0}, Key: {1}, Value: {2}";

        // Success Messages
        public const string UserRetrieved = "User {0} retrieved successfully!";
        public const string UserUpdated = "User {0} updated successfully!";
        public const string UserCreatedSuccessfully = "User {0} created successfully!";
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
    }
}
