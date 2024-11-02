using System.Net;

namespace UserManagementService.Services
{
    public class RegisterService(
        IUserManagement userManagement,
        ILoggingFactory<IRegisterService> logger,
        IKafkaEventProducer<string, UserCreatedEvent> producer,
        IDataFactory dataFactory,
        IRoleManagement roleManagement,
        CancellationToken cancellationToken) : IRegisterService
    {
        private readonly IUserManagement _userManagement = userManagement;
        private readonly LoggingFactory<IRegisterService> _logger = logger;
        private readonly IKafkaEventProducer<string, UserCreatedEvent> _producer = producer;
        private readonly IDataFactory _dataFactory = dataFactory;
        private readonly IRoleManagement _roleManagement = roleManagement;
        private readonly CancellationToken _cancellationToken = cancellationToken;

        private const string UserCreatedSuccessfully = "User {0} created successfully!";
        private const string KafkaTopic = "UserManagementService";
        private const string UserCreatedKey = "UserCreated";
        private const string Success = nameof(Success);

        public async Task<APIResponse<RegisterDto>> RegisterUserAsync(CreateUserDTO registerData)
        {
            string userRole = RoleName.User.ToString();
            await _roleManagement.EnsureRoleExistsAsync(userRole);

            UserManagementResult userCreated = await _userManagement.CreateUserAsync(registerData, userRole);

            if (!userCreated.Succeeded)
            {
                return new APIResponse<RegisterDto>(HttpStatusCode.BadRequest, userCreated.ErrorMessage);
            }

            await SendMessageToMessageBrokerAndSubscribers(userCreated.User!);

            string successMessage = string.Format(UserCreatedSuccessfully, userCreated.User!.UserName);
            RegisterDto response = await GenerateDtoResponseAsync(userCreated.User!);

            _logger.LogInfo(Success, successMessage);

            return new APIResponse<RegisterDto>(HttpStatusCode.Created, successMessage, response);
        }

        private async Task<RegisterDto> GenerateDtoResponseAsync(User user)
        {
            return await _dataFactory.CreateRegisterDtoAsync(user);
        }

        private async Task SendMessageToMessageBrokerAndSubscribers(User user)
        {
            UserCreatedEvent userCreatedEvent = _dataFactory.CreateUserCreatedEvent(user);
            await _producer.SendEventAsync(KafkaTopic, UserCreatedKey, userCreatedEvent, _cancellationToken);
        }
    }
}
