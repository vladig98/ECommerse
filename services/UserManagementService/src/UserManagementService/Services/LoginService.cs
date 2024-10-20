using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Services
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LoginService> _logger;
        private readonly IDataFactory _dataFactory;

        public LoginService(UserManager<User> userManager, ILogger<LoginService> logger, IDataFactory dataFactory)
        {
            _userManager = userManager;
            _logger = logger;
            _dataFactory = dataFactory;
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

            TokenDto tokenDto = await _dataFactory.CreateTokenDtoAsync(user);

            string message = string.Format(GlobalConstants.UserLoggedInSuccessfully, user.UserName);
            _logger.LogInformation(GlobalConstants.LogInfo(GlobalConstants.Success, message));

            return ServiceResult<TokenDto>.Success(tokenDto, message);
        }
    }
}
