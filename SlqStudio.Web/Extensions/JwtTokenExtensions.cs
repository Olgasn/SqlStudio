using SlqStudio.Application.Services;
using SlqStudio.Application.Services.Implementation;
using SlqStudio.Application.Services.Models;

namespace SlqStudio.Extensions;

public static class JwtTokenExtensions
{
    public static void AddJwtTokenServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings
        {
            Key = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured."),
            Issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured."),
            Audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured."),
            ExpirationMinutes = 30
        };

        services.AddSingleton(jwtSettings);
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IJwtTokenHandler, JwtTokenHandler>();
    }
}
