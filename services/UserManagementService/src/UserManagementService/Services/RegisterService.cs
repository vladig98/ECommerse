using System.Net;

namespace UserManagementService.Services
{
    public class RegisterService(
        IUserManagement userManagement,
        ILogger<IRegisterService> logger,
        IKafkaEventProducer<string, UserCreatedEvent> producer,
        IDataFactory dataFactory,
        IRoleManagement roleManagement,
        CancellationToken cancellationToken) : IRegisterService
    {
        private readonly IUserManagement _userManagement = userManagement;
        private readonly ILogger<IRegisterService> _logger = logger;
        private readonly IKafkaEventProducer<string, UserCreatedEvent> _producer = producer;
        private readonly IDataFactory _dataFactory = dataFactory;
        private readonly IRoleManagement _roleManagement = roleManagement;
        private readonly CancellationToken _cancellationToken = cancellationToken;

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

            string successMessage = string.Format(GlobalConstants.UserCreatedSuccessfully, userCreated.User!.UserName);
            RegisterDto response = await GenerateDtoResponseAsync(userCreated.User!).ConfigureAwait(true);

            _logger.LogInformation(successMessage);

            return GetSuccessResponse(response, successMessage);
        }

        private static APIResponse<RegisterDto> GetSuccessResponse(RegisterDto registerDto, string message)
        {
            APIResponse<RegisterDto> response = new APIResponse<RegisterDto>(HttpStatusCode.OK, message, registerDto);

            return response;
        }

        private async Task<RegisterDto> GenerateDtoResponseAsync(User user)
        {
            return await _dataFactory.CreateRegisterDtoAsync(user).ConfigureAwait(true);
        }

        private async Task SendMessageToMessageBrokerAndSubscribers(User user)
        {
            UserCreatedEvent userCreatedEvent = _dataFactory.CreateUserCreatedEvent(user);
            await _producer.SendEventAsync(GlobalConstants.KafkaTopic, GlobalConstants.UserCreatedKey, userCreatedEvent, _cancellationToken).ConfigureAwait(true);
        }
    }
}
