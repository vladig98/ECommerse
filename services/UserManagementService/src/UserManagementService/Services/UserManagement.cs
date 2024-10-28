using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;
using System.Text;

namespace UserManagementService.Services
{
    public class UserManagement(UserManager<User> userManager, IDataFactory dataFactory, LoggingFactory<UserManagement> logger, ECommerceDbContext context) : IUserManagement
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IDataFactory _dataFactory = dataFactory;
        private readonly LoggingFactory<UserManagement> _logger = logger;
        private readonly ECommerceDbContext _context = context;

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

            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            User user = _dataFactory.CreateUserInstance(registerData);

            IdentityResult userCreated = await _userManager.CreateAsync(user, registerData.Password);
            await HandleIdentityResults(userCreated, result, transaction);

            IdentityResult addedToRole = await _userManager.AddToRoleAsync(user, roleName);
            await HandleIdentityResults(addedToRole, result, transaction);

            IdentityResult addedClaims = await _userManager.AddClaimAsync(user, claim: new Claim(ClaimTypes.Role.ToString(), roleName));
            await HandleIdentityResults(addedClaims, result, transaction);

            await transaction.CommitAsync();

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

        private async Task HandleIdentityResults(IdentityResult identityResult, UserManagementResult result, IDbContextTransaction transaction)
        {
            if (identityResult.Succeeded)
            {
                return;
            }

            await transaction.RollbackAsync();

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
    }
}
