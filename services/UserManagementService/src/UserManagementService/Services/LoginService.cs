using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Services
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginService> _logger;

        public LoginService(UserManager<User> userManager, ITokenService tokenService, ILogger<LoginService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<ServiceResult<TokenDto>> LoginUser(LoginDto loginData)
        {
            var user = await _userManager.FindByNameAsync(loginData.Username);

            if (user == null)
            {
                _logger.LogError(GlobalConstants.UserNotFound);
                return ServiceResult<TokenDto>.Failure(GlobalConstants.UserNotFound);
            }

            var correctPassword = await _userManager.CheckPasswordAsync(user, loginData.Password);

            if (!correctPassword)
            {
                _logger.LogError(GlobalConstants.UserEnteredWrongPassword);
                return ServiceResult<TokenDto>.Failure(GlobalConstants.UserEnteredWrongPassword);
            }

            var token = await _tokenService.GenerateJWTToken(user);

            var tokenDto = new TokenDto
            {
                Token = token
            };

            var message = string.Format(GlobalConstants.UserLoggedInSuccessfully, user.UserName);
            _logger.LogInformation(message);
            return ServiceResult<TokenDto>.Success(tokenDto, message);
        }
    }
}
