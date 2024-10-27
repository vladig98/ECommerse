using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Net;

namespace UserManagementService.Services
{
    public class LoginService(UserManager<User> userManager, LoggingFactory<LoginService> logger, IDataFactory dataFactory) : ILoginService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly LoggingFactory<LoginService> _logger = logger;
        private readonly IDataFactory _dataFactory = dataFactory;

        private const string FailedLogin = "Login failed!";
        private const string WrongCredentials = "Invalid username or password.";
        private const string InvalidData = "The provided data was invalid!";
        private const string UserNotFound = "User {0} does not exist!";
        private const string UserEnteredWrongPassword = "Incorrect password for user {0}!";
        private const string UserLoggedInSuccessfully = "User {0} logged in successfully!";
        private const string Success = nameof(Success);
        private const string LoginServiceName = nameof(LoginService);

        public async Task<APIResponse<TokenDto>> LoginUser(LoginDto loginData)
        {
            _logger.LogTrace(LoginServiceName, "Inside of the Login User method");
            _logger.LogTrace(LoginServiceName, "Checking if the login data is valid!");
            if (loginData == null)
            {
                _logger.LogTrace(LoginServiceName, "The login data was null");
                _logger.LogDebug(LoginServiceName, "The login data was null");
                _logger.LogError(FailedLogin, InvalidData, typeof(LoginDto).Name);
                return new APIResponse<TokenDto>(HttpStatusCode.BadRequest, InvalidData);
            }
            _logger.LogDebug(LoginServiceName, JsonConvert.SerializeObject(loginData));

            _logger.LogTrace(LoginServiceName, "About to check if the user exists!");
            User? user = await _userManager.FindByNameAsync(loginData.Username);
            _logger.LogTrace(LoginServiceName, "Received a response from the user exist check!");

            if (user == null)
            {
                _logger.LogTrace(LoginServiceName, "The user does not exist!");
                _logger.LogDebug(LoginServiceName, "The user does not exist!");
                _logger.LogError(FailedLogin, UserNotFound, loginData.Username);
                return new APIResponse<TokenDto>(HttpStatusCode.BadRequest, WrongCredentials);
            }
            _logger.LogTrace(LoginServiceName, JsonConvert.SerializeObject(user));
            _logger.LogDebug(LoginServiceName, JsonConvert.SerializeObject(user));

            _logger.LogTrace(LoginServiceName, "About to check if the password matches");
            bool correctPassword = await _userManager.CheckPasswordAsync(user, loginData.Password);
            _logger.LogTrace(LoginServiceName, "Checking the password complete!");

            if (!correctPassword)
            {
                _logger.LogTrace(LoginServiceName, "Invalid password");
                _logger.LogDebug(LoginServiceName, "Invalid password");
                _logger.LogError(FailedLogin, UserEnteredWrongPassword, loginData.Username);
                return new APIResponse<TokenDto>(HttpStatusCode.BadRequest, WrongCredentials);
            }

            _logger.LogTrace(LoginServiceName, "About to genereta a JWT token for the user");
            TokenDto tokenDto = await _dataFactory.CreateTokenDtoAsync(user);
            _logger.LogTrace(LoginServiceName, "JWT generation completed");

            string message = string.Format(UserLoggedInSuccessfully, user.UserName);
            _logger.LogInfo(Success, message);

            _logger.LogTrace(LoginServiceName, "About to generate success response");
            return GetSuccessResponse(tokenDto, user.UserName);
        }

        private APIResponse<TokenDto> GetSuccessResponse(TokenDto tokenDto, string username)
        {
            string successMessage = string.Format(UserLoggedInSuccessfully, username);
            var response = new APIResponse<TokenDto>(HttpStatusCode.OK, successMessage, tokenDto);
            _logger.LogTrace(LoginServiceName, "Success response generated!");
            _logger.LogTrace(LoginServiceName, JsonConvert.SerializeObject(response));
            _logger.LogDebug(LoginServiceName, JsonConvert.SerializeObject(response));

            return response;
        }
    }
}
