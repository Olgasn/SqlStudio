using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SlqStudio.Application.Services.Implementation;
using SlqStudio.Application.Services.Models;

namespace SlqStudio.Tests;

public sealed class JwtTokenHandlerTests
{
    private readonly JwtTokenHandler _handler = new();

    [Fact]
    public void GetEmailFromToken_ReturnsEmailClaim()
    {
        var token = CreateToken(
            new Claim(ClaimTypes.Email, "student@example.com"),
            new Claim(ClaimTypes.Role, "EditingTeacher"));

        var email = _handler.GetEmailFromToken(token);

        Assert.Equal("student@example.com", email);
    }

    [Fact]
    public void GetEmailFromToken_WhenClaimMissing_ReturnsEmptyString()
    {
        var token = CreateToken(new Claim(ClaimTypes.Name, "Student"));

        var email = _handler.GetEmailFromToken(token);

        Assert.Equal(string.Empty, email);
    }

    [Fact]
    public void GetEmailFromClaims_ReturnsEmailClaim()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Email, "teacher@example.com") },
                authenticationType: "TestAuth"));

        var email = _handler.GetEmailFromClaims(principal);

        Assert.Equal("teacher@example.com", email);
    }

    [Fact]
    public void GetClaimsFromToken_ReturnsEmailRoleAndName()
    {
        var token = CreateToken(
            new Claim(ClaimTypes.Email, "teacher@example.com"),
            new Claim(ClaimTypes.Role, "EditingTeacher"),
            new Claim(ClaimTypes.Name, "Teacher Name"));

        var (email, role, name) = _handler.GetClaimsFromToken(token);

        Assert.Equal("teacher@example.com", email);
        Assert.Equal(UserRole.EditingTeacher, role);
        Assert.Equal("Teacher Name", name);
    }

    private static string CreateToken(params Claim[] claims)
    {
        var token = new JwtSecurityToken(claims: claims);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
