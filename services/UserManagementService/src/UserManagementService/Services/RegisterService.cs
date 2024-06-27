using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Security.Claims;

namespace UserManagementService.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<RegisterService> _logger;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public RegisterService(UserManager<User> userManager, RoleManager<Role> roleManager, ILogger<RegisterService> logger, IMapper mapper, ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        private bool DoPasswordsMatch(string password, string confirmPassword)
        {
            return password == confirmPassword;
        }

        private async Task<bool> DoesUserExistByUsername(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user != null;
        }

        private async Task<bool> DoesUserExistByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        private async Task<bool> DoesRoleExists(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        private Role CreateRole(string roleName)
        {
            return new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName
            };
        }

        private User CreateUser(CreateUserDTO data)
        {
            var validDOB = DateTime.TryParseExact(data.DateOfBirth, GlobalConstants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob);

            var user = _mapper.Map<User>(data);
            user.LoyaltyPoints = 0;
            user.MembershipLevel = MembershipLevels.Silver.ToString();
            user.Id = Guid.NewGuid().ToString();
            user.DateOfBirth = validDOB ? DateTime.SpecifyKind(dob, DateTimeKind.Utc) : (DateTime?)null;

            return user;
        }

        private async Task<UserDTO> GetUserDTO(User user)
        {
            var userDto = _mapper.Map<UserDTO>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Role = roles.FirstOrDefault();

            return userDto;
        }

        public async Task<ServiceResult<RegisterDto>> RegisterUser(CreateUserDTO registerData)
        {
            if (!DoPasswordsMatch(registerData.Password, registerData.ConfirmPassword))
            {
                _logger.LogError(GlobalConstants.PasswordsDoNotMatch);
                return ServiceResult<RegisterDto>.Failure(GlobalConstants.PasswordsDoNotMatch);
            }

            if (await DoesUserExistByUsername(registerData.Username))
            {
                _logger.LogError(GlobalConstants.UsernameAlreadyExists);
                return ServiceResult<RegisterDto>.Failure(GlobalConstants.UsernameAlreadyExists);
            }

            if (await DoesUserExistByEmail(registerData.Email))
            {
                _logger.LogError(GlobalConstants.EmailAlreadyExists);
                return ServiceResult<RegisterDto>.Failure(GlobalConstants.EmailAlreadyExists);
            }

            var role = CreateRole(RoleName.User.ToString());

            if (!await DoesRoleExists(role.Name))
            {
                await _roleManager.CreateAsync(role);
            }

            var user = CreateUser(registerData);

            var userCreated = await _userManager.CreateAsync(user, registerData.Password);

            if (!userCreated.Succeeded)
            {
                string error = string.Format(GlobalConstants.PasswordsDoNotMeetRequirements, string.Join(Environment.NewLine, userCreated.Errors.Select(e => e.Description)));
                _logger.LogError(error);
                return ServiceResult<RegisterDto>.Failure(error);
            }

            await _userManager.AddToRoleAsync(user, role.Name);
            await _userManager.AddClaimAsync(user, claim: new Claim(ClaimTypes.Role.ToString(), role.Name));

            string successMessage = string.Format(GlobalConstants.UserCreatedSuccessfully, user.UserName);

            _logger.LogInformation(successMessage);

            var userDto = await GetUserDTO(user);

            var token = await _tokenService.GenerateJWTToken(user);

            var registerDto = new RegisterDto
            {
                Token = token,
                UserData = userDto
            };

            return ServiceResult<RegisterDto>.Success(registerDto, successMessage);
        }
    }
}
