using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SlqStudio.Application.Services.JwtServices.Implementation;
using SlqStudio.Application.Services.JwtServices.Models;

namespace SlqStudio.Tests;

public sealed class JwtTokenServiceTests
{
    private static JwtSettings Settings(string key = "super-secret-signing-key-of-at-least-32-bytes!") => new()
    {
        Key = key,
        Issuer = "SlqStudio",
        Audience = "SlqStudioUsers",
        ExpirationMinutes = 30
    };

    [Fact]
    public void GenerateJwtToken_EmbedsClaimsIssuerAndAudience()
    {
        var settings = Settings();
        var service = new JwtTokenService(settings);

        var token = service.GenerateJwtToken("student@example.com", "EditingTeacher", "Иван Иванов");

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(settings.Issuer, parsed.Issuer);
        Assert.Contains(settings.Audience, parsed.Audiences);
        Assert.Equal("student@example.com", parsed.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal("EditingTeacher", parsed.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal("Иван Иванов", parsed.Claims.First(c => c.Type == ClaimTypes.Name).Value);
    }

    [Fact]
    public void GenerateJwtToken_SetsExpirationFromSettings()
    {
        var settings = Settings();
        var service = new JwtTokenService(settings);

        var token = service.GenerateJwtToken("a@b.com", "Student", "Name");

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var expectedExpiry = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes);
        Assert.True(parsed.ValidTo <= expectedExpiry.AddMinutes(1));
        Assert.True(parsed.ValidTo >= DateTime.UtcNow);
    }

    [Fact]
    public void ValidateJwtToken_WhenTokenSignedWithSameKey_ReturnsTrue()
    {
        var service = new JwtTokenService(Settings());
        var token = service.GenerateJwtToken("a@b.com", "Student", "Name");

        Assert.True(service.ValidateJwtToken(token));
    }

    [Fact]
    public void ValidateJwtToken_WhenSignedWithDifferentKey_Throws()
    {
        var issuingService = new JwtTokenService(Settings("first-signing-key-which-is-also-32-bytes-long!"));
        var token = issuingService.GenerateJwtToken("a@b.com", "Student", "Name");

        var validatingService = new JwtTokenService(Settings("second-different-key-which-is-also-32bytes!!"));

        Assert.Throws<SecurityTokenException>(() => validatingService.ValidateJwtToken(token));
    }

    [Fact]
    public void ValidateJwtToken_WhenTokenIsMalformed_Throws()
    {
        var service = new JwtTokenService(Settings());

        Assert.ThrowsAny<Exception>(() => service.ValidateJwtToken("not-a-real-token"));
    }
}
