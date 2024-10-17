using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

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
            User? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError(GlobalConstants.LogError(GlobalConstants.Failure, string.Format(GlobalConstants.UserNotFound, userId)));
                return ServiceResult<UserDTO>.Failure(GlobalConstants.WrongCredentials);
            }

            UserDTO userDto = await GenerateUserDto(user);

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

            user = UpdateUserData(user, updatedData);
            await _userManager.UpdateAsync(user);

            UserDTO userDto = await GenerateUserDto(user);

            string success = string.Format(GlobalConstants.UserUpdated, user.UserName);
            _logger.LogInformation(GlobalConstants.LogInfo(GlobalConstants.Success, success));

            return ServiceResult<UserDTO>.Success(userDto, success);
        }

        private User UpdateUserData(User user, EditUserDto updatedData)
        {
            bool dobParsed = DateTime.TryParseExact(updatedData.DateOfBirth, GlobalConstants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob);

            user.State = updatedData.State ?? user.State;
            user.Street = updatedData.Street ?? user.Street;
            user.PreferredCurrency = updatedData.PreferredCurrency ?? user.PreferredCurrency;
            user.City = updatedData.City ?? user.City;
            user.Email = updatedData.Email ?? user.Email;
            user.Country = updatedData.Country ?? user.Country;
            user.PostalCode = updatedData.PostalCode ?? user.PostalCode;
            user.PhoneNumber = updatedData.PhoneNumber ?? user.PhoneNumber;
            user.PreferredLanguage = updatedData.PreferredLanguage ?? user.PreferredLanguage;
            user.DateOfBirth = dobParsed ? dob : user.DateOfBirth;
            user.FirstName = updatedData.FirstName ?? user.FirstName;
            user.LastName = updatedData.LastName ?? user.LastName;
            user.DateUpdated = DateTime.Now;

            return user;
        }

        private async Task<UserDTO> GenerateUserDto(User user)
        {
            UserDTO userDto = _mapper.Map<UserDTO>(user);
            userDto.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()!;

            return userDto;
        }
    }
}