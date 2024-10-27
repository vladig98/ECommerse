using Microsoft.AspNetCore.Identity;
using System.Net;

namespace UserManagementService.Services
{
    public class ProfileService(UserManager<User> userManager, LoggingFactory<ProfileService> logger, IDataFactory dataFactory) : IProfileService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly LoggingFactory<ProfileService> _logger = logger;
        private readonly IDataFactory _dataFactory = dataFactory;

        private const string UserNotFound = "User {0} does not exist!";
        private const string EmailAlreadyExists = "User with this email address {0} already exists!";
        private const string Failure = nameof(Failure);
        private const string Success = nameof(Success);

        public async Task<APIResponse<UserDTO>> GetUser(string userId)
        {
            User? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError(Failure, UserNotFound, userId);
                return new APIResponse<UserDTO>(HttpStatusCode.BadRequest, "");
            }

            UserDTO userDto = _dataFactory.CreateUserDto(user);

            string success = string.Format(GlobalConstants.UserRetrieved, user.UserName);
            _logger.LogInfo(Success, success);

            return GetSuccessResponse(userDto, success);
        }

        public async Task<APIResponse<UserDTO>> UpdateUser(string userId, EditUserDto updatedData)
        {
            User? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError(Failure, UserNotFound, userId);
                return new APIResponse<UserDTO>(HttpStatusCode.BadRequest, string.Format(UserNotFound, userId));
            }

            User? userEmail = await _userManager.FindByEmailAsync(updatedData.Email!);

            if (userEmail != null)
            {
                _logger.LogError(Failure, EmailAlreadyExists, updatedData.Email);
                return new APIResponse<UserDTO>(HttpStatusCode.BadRequest, string.Format(EmailAlreadyExists, updatedData.Email));
            }

            user = _dataFactory.UpdateUser(updatedData);
            await _userManager.UpdateAsync(user);

            UserDTO userDto = _dataFactory.CreateUserDto(user);

            string success = string.Format(GlobalConstants.UserUpdated, user.UserName);
            _logger.LogInfo(Success, success);

            return GetSuccessResponse(userDto, success);
        }

        private static APIResponse<UserDTO> GetSuccessResponse(UserDTO userDto, string message)
        {
            APIResponse<UserDTO> response = new APIResponse<UserDTO>(HttpStatusCode.OK, message, userDto);

            return response;
        }
    }
}