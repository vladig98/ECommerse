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
            if (loginData == null)
            {
                _logger.LogError(GlobalConstants.LogError(GlobalConstants.FailedLogin, string.Format(GlobalConstants.InvalidData, typeof(LoginDto).Name)));
                return ServiceResult<TokenDto>.Failure(GlobalConstants.WrongCredentials);
            }

            User? user = await _userManager.FindByNameAsync(loginData.Username);

            if (user == null)
            {
                _logger.LogError(GlobalConstants.LogError(GlobalConstants.FailedLogin, string.Format(GlobalConstants.UserNotFound, loginData.Username)));
                return ServiceResult<TokenDto>.Failure(GlobalConstants.WrongCredentials);
            }

            bool correctPassword = await _userManager.CheckPasswordAsync(user, loginData.Password);

            if (!correctPassword)
            {
                _logger.LogError(GlobalConstants.LogError(GlobalConstants.FailedLogin, string.Format(GlobalConstants.UserEnteredWrongPassword, loginData.Username)));
                return ServiceResult<TokenDto>.Failure(GlobalConstants.WrongCredentials);
            }

            string token = await _tokenService.GenerateJWTToken(user);

            TokenDto tokenDto = new TokenDto
            {
                Token = token
            };

            string message = string.Format(GlobalConstants.UserLoggedInSuccessfully, user.UserName);
            _logger.LogInformation(GlobalConstants.LogInfo(GlobalConstants.Success, message));

            return ServiceResult<TokenDto>.Success(tokenDto, message);
        }
    }
}
