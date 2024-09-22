using System.IdentityModel.Tokens.Jwt;

namespace OrderManagementService.Services
{
    public class TokenManager : ITokenManager
    {
        public string ExtractUserNameFromJWT(string jwt)
        {
            JwtSecurityToken token = new JwtSecurityToken(jwt.Split(GlobalConstants.BearerString)[GlobalConstants.JWTIndex]);

            string username = token.Claims.First(x => x.Type == GlobalConstants.SubClaim).Value;

            return username;
        }
    }
}
