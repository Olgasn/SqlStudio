using System.Security.Claims;
using SlqStudio.Application.Services.Models;

namespace SlqStudio.Application.Services;

public interface IJwtTokenHandler
{
    string GetEmailFromToken(string token);
    string GetEmailFromClaims(ClaimsPrincipal claimsPrincipal);
    (string Email, UserRole Role, string Name) GetClaimsFromToken(string token);
}
