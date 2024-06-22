using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Security.Claims;

namespace UserManagementService.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        public UserService(UserManager<User> userManager, RoleManager<Role> roleManager, ILogger<UserService> logger, IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ServiceResult<UserDTO>> RegisterUser(CreateUserDTO registerData)
        {
            string errorMessage = string.Empty;

            if (registerData.Password != registerData.ConfirmPassword)
            {
                errorMessage = GlobalConstants.PasswordsDoNotMatch;
                _logger.LogError(errorMessage);
                return ServiceResult<UserDTO>.Failure(errorMessage);
            }

            var dbUser = await _userManager.FindByNameAsync(registerData.Username);

            if (dbUser != null)
            {
                errorMessage = GlobalConstants.UsernameAlreadyExists;
                _logger.LogError(errorMessage);
                return ServiceResult<UserDTO>.Failure(errorMessage);
            }

            dbUser = await _userManager.FindByEmailAsync(registerData.Email);

            if (dbUser != null)
            {
                errorMessage = GlobalConstants.EmailAlreadyExists;
                _logger.LogError(errorMessage);
                return ServiceResult<UserDTO>.Failure(errorMessage);
            }

            var role = new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = RoleName.User.ToString()
            };

            var roleExists = await _roleManager.RoleExistsAsync(role.Name);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(role);
            }

            var validDOB = DateTime.TryParseExact(registerData.DateOfBirth, GlobalConstants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob);

            var user = _mapper.Map<User>(registerData);
            user.LoyaltyPoints = 0;
            user.MembershipLevel = MembershipLevels.Silver.ToString();
            user.Id = Guid.NewGuid().ToString();
            user.DateOfBirth = validDOB ? DateTime.SpecifyKind(dob, DateTimeKind.Utc) : (DateTime?)null;

            var userCreated = await _userManager.CreateAsync(user, registerData.Password);

            if (!userCreated.Succeeded)
            {
                errorMessage = string.Format(GlobalConstants.PasswordsDoNotMeetRequirements, string.Join(GlobalConstants.CommaSeparator, userCreated.Errors.Select(e => e.Description)));

                _logger.LogError(errorMessage);
                return ServiceResult<UserDTO>.Failure(errorMessage);
            }

            await _userManager.AddToRoleAsync(user, role.Name);
            await _userManager.AddClaimAsync(user, claim: new Claim(ClaimTypes.Role.ToString(), role.Name));

            _logger.LogInformation(string.Format(GlobalConstants.UserCreatedSuccessfully, user.UserName));

            var userDto = _mapper.Map<UserDTO>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Role = roles.FirstOrDefault();

            return ServiceResult<UserDTO>.Success(userDto);
        }
    }
}
