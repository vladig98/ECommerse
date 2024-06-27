using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserManagementService.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TokenService> _logger;
        private readonly IConfiguration _configuration;

        public TokenService(UserManager<User> userManager, ILogger<TokenService> logger, IConfiguration configuration)
        {
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> GenerateJWTToken(User user)
        {
            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            string jwtIssuer = _configuration[GlobalConstants.JWTIssuer];
            string jwtKey = _configuration[GlobalConstants.JWTKey];

            byte[] jwtKeyBytes = Encoding.ASCII.GetBytes(jwtKey);
            SymmetricSecurityKey key = new SymmetricSecurityKey(jwtKeyBytes);
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(3600), // valid for 1 hour
                    audience: jwtIssuer,
                    signingCredentials: credentials
            );

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            await _userManager.AddClaimsAsync(user, claims);
            await _userManager.SetAuthenticationTokenAsync(user, GlobalConstants.LoginProvider, GlobalConstants.JWT, jwt);

            _logger.LogInformation(string.Format(GlobalConstants.JWTTokenSucces, user.UserName));

            return jwt;
        }
    }
}
