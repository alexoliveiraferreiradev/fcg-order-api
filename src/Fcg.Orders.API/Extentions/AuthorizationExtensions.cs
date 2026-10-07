using Fcg.Core.WebApi.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Fcg.Orders.API.Extentions
{
    public static class AuthorizationExtensions
    {
        public static WebApplicationBuilder AddAuthorizationExtension(this WebApplicationBuilder builder)
        {
            builder.AddPolicyExtension().AddJwtBearerExtension();
            return builder;
        }
        #region Policy Extension
        private static WebApplicationBuilder AddPolicyExtension(this WebApplicationBuilder builder) {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("PlayersOnly", policy => policy.RequireRole("PlayerRole"));
            });
            return builder; 
        }
        #endregion

        #region JWT Bearer Extension
        private static WebApplicationBuilder AddJwtBearerExtension(this WebApplicationBuilder builder)
        {
            var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
            builder.Services.Configure<JwtSettings>(jwtSettingsSection);
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
            builder.Services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience
                };
            });
            return builder;
        }
        #endregion
    }
}
