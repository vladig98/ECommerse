using System.Net;

namespace UserManagementService.Services
{
    public class LoginService(IUserManagement userManager, ILoggingFactory<ILoginService> logger, IDataFactory dataFactory) : ILoginService
    {
        private readonly IUserManagement _userManager = userManager;
        private readonly ILoggingFactory<ILoginService> _logger = logger;
        private readonly IDataFactory _dataFactory = dataFactory;

        private const string FailedLogin = "Login failed!";
        private const string WrongCredentials = "Invalid username or password.";
        private const string InvalidData = "The provided data was invalid!";
        private const string UserNotFound = "User {0} does not exist!";
        private const string UserEnteredWrongPassword = "Incorrect password for user {0}!";
        private const string UserLoggedInSuccessfully = "User {0} logged in successfully!";
        private const string Success = nameof(Success);

        public async Task<APIResponse<TokenDto>> LoginUser(LoginDto loginData)
        {
            if (loginData == null)
            {
                _logger.LogError(FailedLogin, InvalidData, typeof(LoginDto).Name);
                return new APIResponse<TokenDto>(HttpStatusCode.BadRequest, InvalidData);
            }

            User? user = await _userManager.FindByNameAsync(loginData.Username);

            if (user == null)
            {
                _logger.LogError(FailedLogin, UserNotFound, loginData.Username);
                return new APIResponse<TokenDto>(HttpStatusCode.BadRequest, WrongCredentials);
            }

            bool correctPassword = await _userManager.CheckPasswordAsync(user, loginData.Password);

            if (!correctPassword)
            {
                _logger.LogError(FailedLogin, UserEnteredWrongPassword, loginData.Username);
                return new APIResponse<TokenDto>(HttpStatusCode.BadRequest, WrongCredentials);
            }

            TokenDto tokenDto = await _dataFactory.CreateTokenDtoAsync(user);

            string message = string.Format(UserLoggedInSuccessfully, user.UserName);
            _logger.LogInfo(Success, message);

            return new APIResponse<TokenDto>(HttpStatusCode.OK, message, tokenDto);
        }
    }
}
