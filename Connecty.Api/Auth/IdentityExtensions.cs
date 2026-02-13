using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Connecty.Api.Auth;

public static class IdentityExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        string? claim = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

        bool exists = Guid.TryParse(claim, out Guid userId);

        return exists
            ? userId
            : null;
    }
}