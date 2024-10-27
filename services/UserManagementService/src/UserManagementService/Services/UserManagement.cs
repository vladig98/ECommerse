using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;

namespace UserManagementService.Services
{
    public class UserManagement : IUserManagement
    {
        private readonly UserManager<User> _userManager;
        private readonly IDataFactory _dataFactory;
        private readonly ILogger<UserManagement> _logger;
        private readonly ECommerceDbContext _context;

        public UserManagement(UserManager<User> userManager, IDataFactory dataFactory, ILogger<UserManagement> logger, ECommerceDbContext context)
        {
            _userManager = userManager;
            _dataFactory = dataFactory;
            _logger = logger;
            _context = context;
        }

        public async Task<UserManagementResult> CreateUserAsync(CreateUserDTO registerData, string roleName)
        {
            UserManagementResult result = new UserManagementResult();

            if (await UserExists(registerData, result).ConfigureAwait(true))
            {
                return result;
            }

            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(true);

            User user = _dataFactory.CreateUserInstance(registerData);

            IdentityResult userCreated = await _userManager.CreateAsync(user, registerData.Password).ConfigureAwait(true);
            await HandleIdentityResults(userCreated, result, transaction).ConfigureAwait(true);

            IdentityResult addedToRole = await _userManager.AddToRoleAsync(user, roleName).ConfigureAwait(true);
            await HandleIdentityResults(addedToRole, result, transaction).ConfigureAwait(true);

            IdentityResult addedClaims = await _userManager.AddClaimAsync(user, claim: new Claim(ClaimTypes.Role.ToString(), roleName)).ConfigureAwait(true);
            await HandleIdentityResults(addedClaims, result, transaction).ConfigureAwait(true);

            await transaction.CommitAsync().ConfigureAwait(true);

            result.User = user;
            result.Succeeded = true;

            return result;
        }

        private async Task<bool> UserExists(CreateUserDTO registerData, UserManagementResult result)
        {
            User? user = await _userManager.FindByNameAsync(registerData.Username).ConfigureAwait(true);

            if (user != null)
            {
                HandleErrors(result, string.Format(GlobalConstants.UsernameAlreadyExists, registerData.Username));
                return true;
            }

            user = await _userManager.FindByEmailAsync(registerData.Email).ConfigureAwait(true);

            if (user != null)
            {
                HandleErrors(result, string.Format(GlobalConstants.EmailAlreadyExists, registerData.Email));
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

            await transaction.RollbackAsync().ConfigureAwait(true);

            string message = string.Format(GlobalConstants.PasswordsDoNotMeetRequirements, string.Join(Environment.NewLine, identityResult.Errors.Select(x => x.Description)));
            HandleErrors(result, message);
        }

        private void HandleErrors(UserManagementResult result, string message)
        {
            result.Succeeded = false;
            result.ErrorMessage = message;

            _logger.LogError(message);
        }
    }
}
