using System.Net;

namespace UserManagementService.Services
{
    public class ProfileService(IUserManagement userManager, ILoggingFactory<IProfileService> logger, IDataFactory dataFactory) : IProfileService
    {
        private readonly IUserManagement _userManager = userManager;
        private readonly ILoggingFactory<IProfileService> _logger = logger;
        private readonly IDataFactory _dataFactory = dataFactory;

        private const string UserNotFound = "User with Id {0} does not exist!";
        private const string EmailAlreadyExists = "User with this email address {0} already exists!";
        private const string UserRetrieved = "User {0} retrieved successfully!";
        private const string UserUpdated = "User {0} updated successfully!";
        private const string Failure = nameof(Failure);
        private const string Success = nameof(Success);

        public async Task<APIResponse<UserDTO>> GetUser(string userId)
        {
            User? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError(Failure, UserNotFound, userId);
                return new APIResponse<UserDTO>(HttpStatusCode.BadRequest, UserNotFound, null, userId);
            }

            UserDTO userDto = _dataFactory.CreateUserDto(user);

            string success = string.Format(UserRetrieved, user.UserName);
            _logger.LogInfo(Success, success);

            return new APIResponse<UserDTO>(HttpStatusCode.OK, success, userDto);
        }

        public async Task<APIResponse<UserDTO>> UpdateUser(string userId, EditUserDto updatedData)
        {
            User? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError(Failure, UserNotFound, userId);
                return new APIResponse<UserDTO>(HttpStatusCode.BadRequest, UserNotFound, null, userId);
            }

            User? userEmail = await _userManager.FindByEmailAsync(updatedData.Email!);

            if (userEmail != null)
            {
                _logger.LogError(Failure, EmailAlreadyExists, updatedData.Email);
                return new APIResponse<UserDTO>(HttpStatusCode.BadRequest, EmailAlreadyExists, null, updatedData.Email);
            }

            user = _dataFactory.UpdateUser(updatedData);
            await _userManager.UpdateAsync(user);

            UserDTO userDto = _dataFactory.CreateUserDto(user);

            string success = string.Format(UserUpdated, user.UserName);
            _logger.LogInfo(Success, success);

            return new APIResponse<UserDTO>(HttpStatusCode.OK, success, userDto);
        }
    }
}