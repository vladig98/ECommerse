using OrderManagementService.Services.Contracts;
using System.IdentityModel.Tokens.Jwt;

namespace OrderManagementService.Services
{
    public class TokenManager : ITokenManager
    {
        public string ExtractUserNameFromJWT(string jwt)
        {
            JwtSecurityToken token = new JwtSecurityToken(jwt.Split("Bearer ")[1]);

            string username = token.Claims.First(x => x.Type == "sub").Value;

            return username;
        }
    }
}
