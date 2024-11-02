using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserManagementService.Services
{
    public class TokenService(IUserManagement userManager, ILoggingFactory<ITokenService> logger, IConfiguration configuration) : ITokenService
    {
        private readonly IUserManagement _userManager = userManager;
        private readonly ILoggingFactory<ITokenService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

        private const string JWT = nameof(JWT);
        private const string LoginProvider = "Ecoomerse-Vladi";
        private const string JWTIssuer = "UserManagement:JWT:Issuer";
        private const string JWTKey = "UserManagement:JWT:Key";
        private const string JWTTokenSucces = "Token generated for user {0}";

        public async Task<string> GenerateJWTToken(User user)
        {
            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!)
            };

            string jwtIssuer = _configuration[JWTIssuer]!;
            string jwtKey = _configuration[JWTKey]!;

            byte[] jwtKeyBytes = Encoding.ASCII.GetBytes(jwtKey);
            SymmetricSecurityKey key = new SymmetricSecurityKey(jwtKeyBytes);
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    audience: jwtIssuer,
                    signingCredentials: credentials
            );

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            await _userManager.AddClaimsAsync(user, claims);
            await _userManager.SetAuthenticationTokenAsync(user, LoginProvider, JWT, jwt);

            _logger.LogInfo(nameof(TokenService), JWTTokenSucces, user.UserName);

            return jwt;
        }
    }
}
