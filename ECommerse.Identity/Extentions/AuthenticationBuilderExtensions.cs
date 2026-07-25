namespace ECommerse.Identity.Extentions;

public static class AuthenticationBuilderExtensions
{
    extension(AuthenticationBuilder builder)
    {
        public AuthenticationBuilder SetJwtToken(JwtConfigSettings jwtSettings, bool isDev)
        {
            string scheme = string.IsNullOrWhiteSpace(jwtSettings.AuthenticationScheme)
                ? JwtBearerDefaults.AuthenticationScheme
                : jwtSettings.AuthenticationScheme;

            builder.AddJwtBearer(scheme, jwtOptions =>
            {
                jwtOptions.Authority = jwtSettings.Authority;
                jwtOptions.Audience = jwtSettings.Audience;
                jwtOptions.MetadataAddress = jwtSettings.MetadataAddress;
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudiences = jwtSettings.ValidAudiences,
                    ValidIssuers = jwtSettings.ValidIssuers
                };
                jwtOptions.MapInboundClaims = false;
                jwtOptions.RequireHttpsMetadata = !isDev;
            });

            return builder;
        }

        public AuthenticationBuilder SetOpenIDConnect(OpenIDConfigSettings openIdSettings, bool isDev)
        {
            builder.AddOpenIdConnect(options =>
            {
                options.Authority = openIdSettings.Authority;
                options.ClientId = openIdSettings.ClientId;
                options.ClientSecret = openIdSettings.ClientSecret;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                options.TokenValidationParameters.RoleClaimType = "roles";
                options.UsePkce = true;
                options.RequireHttpsMetadata = !isDev;
            });

            return builder;
        }
    }
}
