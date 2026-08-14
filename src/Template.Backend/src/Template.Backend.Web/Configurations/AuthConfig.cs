using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Template.Backend.Infrastructure.Auth;
using Template.Backend.Infrastructure.Configurations;
using Template.Backend.Web.Authorization;

namespace Template.Backend.Web.Configurations;

public static class AuthConfig
{
  public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, WebApplicationBuilder builder)
  {
    var authOptions = OptionsValidation.GetValidated(
      builder.Configuration,
      AuthOptions.SectionName,
      new AuthOptionsValidator());

    var jwtOptions = authOptions.Jwt;

    var signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateIssuerSigningKey = true,
          ValidateLifetime = true,
          ValidIssuer = jwtOptions.Issuer,
          ValidAudience = jwtOptions.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(signingKey),
          ClockSkew = TimeSpan.Zero,
          NameClaimType = JwtRegisteredClaimNames.Sub
        };

        options.Events = new JwtBearerEvents
        {
          OnMessageReceived = context =>
          {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs"))
              context.Token = accessToken;

            return Task.CompletedTask;
          }
        };
      });

    services.AddPermissionAuthorization();

    return services;
  }
}
