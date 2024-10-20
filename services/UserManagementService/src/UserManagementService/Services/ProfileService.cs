using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ProfileService> _logger;
        private readonly IDataFactory _dataFactory;

        public ProfileService(UserManager<User> userManager, ILogger<ProfileService> logger, IDataFactory dataFactory)
        {
            _userManager = userManager;
            _logger = logger;
            _dataFactory = dataFactory;
        }

        public async Task<ServiceResult<UserDTO>> GetUser(string userId)
        {
            User? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError(GlobalConstants.LogError(GlobalConstants.Failure, string.Format(GlobalConstants.UserNotFound, userId)));
                return ServiceResult<UserDTO>.Failure(GlobalConstants.WrongCredentials);
            }

            UserDTO userDto = await _dataFactory.CreateUserDtoAsync(user);

            string success = string.Format(GlobalConstants.UserRetrieved, user.UserName);
            _logger.LogInformation(GlobalConstants.LogInfo(GlobalConstants.Success, success));

            return ServiceResult<UserDTO>.Success(userDto, success);
        }

        public async Task<ServiceResult<UserDTO>> UpdateUser(string userId, EditUserDto updatedData)
        {
            User? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError(GlobalConstants.LogError(GlobalConstants.Failure, string.Format(GlobalConstants.UserNotFound, userId)));
                return ServiceResult<UserDTO>.Failure(string.Format(GlobalConstants.UserNotFound, userId));
            }

            User? userEmail = await _userManager.FindByEmailAsync(updatedData.Email!);

            if (userEmail != null)
            {
                _logger.LogError(GlobalConstants.LogError(GlobalConstants.Failure, string.Format(GlobalConstants.EmailAlreadyExists, updatedData.Email)));
                return ServiceResult<UserDTO>.Failure(string.Format(GlobalConstants.EmailAlreadyExists, updatedData.Email));
            }

            user = _dataFactory.UpdateUser(updatedData);
            await _userManager.UpdateAsync(user);

            UserDTO userDto = await _dataFactory.CreateUserDtoAsync(user);

            string success = string.Format(GlobalConstants.UserUpdated, user.UserName);
            _logger.LogInformation(GlobalConstants.LogInfo(GlobalConstants.Success, success));

            return ServiceResult<UserDTO>.Success(userDto, success);
        }
    }
}