using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text;
using System.Transactions;

namespace UserManagementService.Services
{
    public class UserManagement(UserManager<User> userManager, IDataFactory dataFactory, ILoggingFactory<IUserManagement> logger) : IUserManagement
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IDataFactory _dataFactory = dataFactory;
        private readonly ILoggingFactory<IUserManagement> _logger = logger;

        private const string UsernameAlreadyExists = "User with this username {0} already exists!";
        private const string EmailAlreadyExists = "User with this email address {0} already exists!";
        private const string PasswordsDoNotMeetRequirements = "{0}";
        private const string PasswordValidationErrorsFormat = "Error code: {0}, Error Message: {1}";
        private const string Failure = nameof(Failure);

        public async Task<UserManagementResult> CreateUserAsync(CreateUserDTO registerData, string roleName)
        {
            UserManagementResult result = new UserManagementResult();

            if (await UserExists(registerData, result))
            {
                return result;
            }

            using TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            User user = _dataFactory.CreateUserInstance(registerData);

            IdentityResult userCreated = await _userManager.CreateAsync(user, registerData.Password);
            await HandleIdentityResults(userCreated, result);

            if (!result.Succeeded)
            {
                return result;
            }

            IdentityResult addedToRole = await _userManager.AddToRoleAsync(user, roleName);
            await HandleIdentityResults(addedToRole, result);

            if (!result.Succeeded)
            {
                return result;
            }

            IdentityResult addedClaims = await _userManager.AddClaimAsync(user, claim: new Claim(ClaimTypes.Role.ToString(), roleName));
            await HandleIdentityResults(addedClaims, result);

            if (!result.Succeeded)
            {
                return result;
            }

            scope.Complete();

            result.User = user;
            result.Succeeded = true;

            return result;
        }

        private async Task<bool> UserExists(CreateUserDTO registerData, UserManagementResult result)
        {
            User? user = await _userManager.FindByNameAsync(registerData.Username);

            if (user != null)
            {
                GenerateError(result, UsernameAlreadyExists, registerData.Username);
                return true;
            }

            user = await _userManager.FindByEmailAsync(registerData.Email);

            if (user != null)
            {
                GenerateError(result, EmailAlreadyExists, registerData.Email);
                return true;
            }

            return false;
        }

        private async Task HandleIdentityResults(IdentityResult identityResult, UserManagementResult result)
        {
            if (identityResult.Succeeded)
            {
                return;
            }

            GenerateError(result, PasswordsDoNotMeetRequirements, ExtractErrorsFromIdentityResult(identityResult));
        }

        private string ExtractErrorsFromIdentityResult(IdentityResult result)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IdentityError error in result.Errors)
            {
                _logger.LogError(Failure, PasswordValidationErrorsFormat, error.Code, error.Description);
                sb.AppendLine(error.Description);
            }

            return sb.ToString();
        }

        private void GenerateError(UserManagementResult result, string message, params string[] messageParameters)
        {
            result.Succeeded = false;
            result.ErrorMessage = message;

            _logger.LogError(Failure, message, messageParameters);
        }

        public async Task<User?> FindByNameAsync(string name)
        {
            return await _userManager.FindByNameAsync(name);
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<User?> FindByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> UpdateAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> AddClaimsAsync(User user, Claim[] claims)
        {
            return await _userManager.AddClaimsAsync(user, claims);
        }

        public async Task<IdentityResult> SetAuthenticationTokenAsync(User user, string loginProvider, string tokenName, string tokenValue)
        {
            return await _userManager.SetAuthenticationTokenAsync(user, loginProvider, tokenName, tokenValue);
        }
    }
}
