using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace UserManagementService.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<IRegisterService> _logger;
        private readonly IEventBus _eventBus;
        private readonly IDataFactory _dataFactory;
        private readonly IRoleManagement _roleManagement;

        public RegisterService(UserManager<User> userManager, ILogger<IRegisterService> logger,
            IEventBus eventBus, IDataFactory dataFactory, IRoleManagement roleManagement)
        {
            _userManager = userManager;
            _logger = logger;
            _eventBus = eventBus;
            _dataFactory = dataFactory;
            _roleManagement = roleManagement;
        }

        public async Task<ServiceResult<RegisterDto>> RegisterUserAsync(CreateUserDTO registerData)
        {
            // Validations
            // Same usersname validation
            if (await DoesUserExistByUsernameAsync(registerData.Username))
            {
                _logger.LogError(string.Format(GlobalConstants.UsernameAlreadyExists, registerData.Username));
                return ServiceResult<RegisterDto>.Failure(string.Format(GlobalConstants.UsernameAlreadyExists, registerData.Username));
            }

            // Same email address validation
            if (await DoesUserExistByEmailAsync(registerData.Email))
            {
                _logger.LogError(string.Format(GlobalConstants.EmailAlreadyExists, registerData.Email));
                return ServiceResult<RegisterDto>.Failure(string.Format(GlobalConstants.EmailAlreadyExists, registerData.Email));
            }

            // Creationg logic
            // Role
            string userRole = RoleName.User.ToString();
            await _roleManagement.ManageRoleAsync(userRole);

            // User
            User user = _dataFactory.CreateUserInstance(registerData);
            IdentityResult userCreated = await _userManager.CreateAsync(user, registerData.Password);

            // Password Validation
            if (!userCreated.Succeeded)
            {
                string error = string.Format(GlobalConstants.PasswordsDoNotMeetRequirements, string.Join(Environment.NewLine, userCreated.Errors.Select(e => e.Description)));
                _logger.LogError(error);
                return ServiceResult<RegisterDto>.Failure(error);
            }

            // User-Role table
            await AssignUserToRoleAsync(user, userRole);

            // Messaging queue events
            SendMessageToMessageBrokerAndSubscribers(user);

            // Results
            string successMessage = string.Format(GlobalConstants.UserCreatedSuccessfully, user.UserName);
            _logger.LogInformation(successMessage);

            RegisterDto response = await GenerateDtoResponseAsync(user);

            return ServiceResult<RegisterDto>.Success(response, successMessage);
        }

        private async Task<bool> DoesUserExistByUsernameAsync(string username)
        {
            return await _userManager.FindByNameAsync(username) != null;
        }

        private async Task<bool> DoesUserExistByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        private async Task AssignUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
            await AddRoleAsAClaim(user, roleName);
        }

        private async Task AddRoleAsAClaim(User user, string roleName)
        {
            await _userManager.AddClaimAsync(user, claim: new Claim(ClaimTypes.Role.ToString(), roleName));
        }

        private async Task<RegisterDto> GenerateDtoResponseAsync(User user)
        {
            return await _dataFactory.CreateRegisterDtoAsync(user);
        }

        private void SendMessageToMessageBrokerAndSubscribers(User user)
        {
            UserCreatedEvent userCreatedEvent = _dataFactory.CreateSubscribeMessageEvent(user);
            _eventBus.Publish(userCreatedEvent);
        }
    }
}
