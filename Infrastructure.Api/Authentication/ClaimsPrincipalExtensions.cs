using System.Security.Claims;

namespace Infrastructure.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static bool IsInAnyRole(this ClaimsPrincipal user, string roles)
        => roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Any(user.IsInRole);
}
