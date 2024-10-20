namespace UserManagementService.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly IUserManagement _userManagement;
        private readonly ILogger<IRegisterService> _logger;
        private readonly IKafkaEventProducer<string, UserCreatedEvent> _producer;
        private readonly IDataFactory _dataFactory;
        private readonly IRoleManagement _roleManagement;
        private readonly CancellationToken _cancellationToken;

        public RegisterService(
            IUserManagement userManagement,
            ILogger<IRegisterService> logger,
            IKafkaEventProducer<string, UserCreatedEvent> producer,
            IDataFactory dataFactory,
            IRoleManagement roleManagement,
            CancellationToken cancellationToken)
        {
            _userManagement = userManagement;
            _logger = logger;
            _producer = producer;
            _dataFactory = dataFactory;
            _roleManagement = roleManagement;
            _cancellationToken = cancellationToken;
        }

        public async Task<ServiceResult<RegisterDto>> RegisterUserAsync(CreateUserDTO registerData)
        {
            string userRole = RoleName.User.ToString();
            await _roleManagement.EnsureRoleExistsAsync(userRole);

            UserManagementResult userCreated = await _userManagement.CreateUserAsync(registerData, userRole);

            if (!userCreated.Succeeded)
            {
                return ServiceResult<RegisterDto>.Failure(userCreated.ErrorMessage);
            }

            await SendMessageToMessageBrokerAndSubscribers(userCreated.User!);

            string successMessage = string.Format(GlobalConstants.UserCreatedSuccessfully, userCreated.User!.UserName);
            RegisterDto response = await GenerateDtoResponseAsync(userCreated.User!);

            _logger.LogInformation(successMessage);

            return ServiceResult<RegisterDto>.Success(response, successMessage);
        }

        private async Task<RegisterDto> GenerateDtoResponseAsync(User user)
        {
            return await _dataFactory.CreateRegisterDtoAsync(user);
        }

        private async Task SendMessageToMessageBrokerAndSubscribers(User user)
        {
            UserCreatedEvent userCreatedEvent = _dataFactory.CreateSubscribeMessageEvent(user);
            await _producer.SendEventAsync(GlobalConstants.KafkaTopic, GlobalConstants.UserCreatedKey, userCreatedEvent, _cancellationToken);
        }
    }
}
