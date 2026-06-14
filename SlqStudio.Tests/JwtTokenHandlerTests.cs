using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SlqStudio.Application.Services.JwtServices.Implementation;
using SlqStudio.Application.Services.JwtServices.Models;

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
    public void GetEmailFromClaims_WhenClaimMissing_ReturnsEmptyString()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "No Email") },
                authenticationType: "TestAuth"));

        var email = _handler.GetEmailFromClaims(principal);

        Assert.Equal(string.Empty, email);
    }

    [Fact]
    public void GetEmailFromClaims_WhenNoIdentity_ReturnsEmptyString()
    {
        var principal = new ClaimsPrincipal();

        var email = _handler.GetEmailFromClaims(principal);

        Assert.Equal(string.Empty, email);
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

    [Fact]
    public void GetClaimsFromToken_RoleParsingIsCaseInsensitive()
    {
        var token = CreateToken(
            new Claim(ClaimTypes.Email, "student@example.com"),
            new Claim(ClaimTypes.Role, "student"));

        var (_, role, _) = _handler.GetClaimsFromToken(token);

        Assert.Equal(UserRole.Student, role);
    }

    [Fact]
    public void GetClaimsFromToken_WhenRoleMissing_ReturnsDefaultRole()
    {
        var token = CreateToken(new Claim(ClaimTypes.Email, "student@example.com"));

        var (email, role, name) = _handler.GetClaimsFromToken(token);

        Assert.Equal("student@example.com", email);
        Assert.Equal(default, role);
        Assert.Equal(string.Empty, name);
    }

    [Fact]
    public void GetClaimsFromToken_WhenRoleIsUnrecognized_ReturnsDefaultRole()
    {
        var token = CreateToken(
            new Claim(ClaimTypes.Email, "student@example.com"),
            new Claim(ClaimTypes.Role, "NotARealRole"));

        var (_, role, _) = _handler.GetClaimsFromToken(token);

        Assert.Equal(default, role);
    }

    private static string CreateToken(params Claim[] claims)
    {
        var token = new JwtSecurityToken(claims: claims);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
