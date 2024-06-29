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
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError(GlobalConstants.UserNotFound);
                return ServiceResult<UserDTO>.Failure(GlobalConstants.UserNotFound);
            }

            var userDto = await GenerateUserDto(user);

            string success = string.Format(GlobalConstants.UserRetrieved, user.UserName);
            _logger.LogInformation(success);

            return ServiceResult<UserDTO>.Success(userDto, success);
        }

        public async Task<ServiceResult<UserDTO>> UpdateUser(string userId, EditUserDto updatedData)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError(GlobalConstants.UserNotFound);
                return ServiceResult<UserDTO>.Failure(GlobalConstants.UserNotFound);
            }

            var userEmail = await _userManager.FindByEmailAsync(updatedData.Email!);

            if (userEmail != null)
            {
                _logger.LogError(GlobalConstants.EmailAlreadyExists);
                return ServiceResult<UserDTO>.Failure(GlobalConstants.EmailAlreadyExists);
            }

            user = UpdateUserData(user, updatedData);
            await _userManager.UpdateAsync(user);

            var userDto = await GenerateUserDto(user);

            string success = string.Format(GlobalConstants.UserUpdated, user.UserName);
            _logger.LogInformation(success);

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

            return user;
        }

        private async Task<UserDTO> GenerateUserDto(User user)
        {
            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()!;

            return userDto;
        }
    }
}