
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(UserManager<User> userManager, IMapper mapper, ILogger<ProfileService> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResult<UserDTO>> GetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError(GlobalConstants.UserNotFound);
                return ServiceResult<UserDTO>.Failure(GlobalConstants.UserNotFound);
            }

            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            string success = string.Format(GlobalConstants.UserRetrieved, user.UserName);
            _logger.LogInformation(success);

            return ServiceResult<UserDTO>.Success(userDto, success);
        }

        public Task<ServiceResult<UserDTO>> UpdateUser(CreateUserDTO updatedData)
        {
            throw new NotImplementedException();
        }
    }
}
