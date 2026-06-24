using System;
using System.Security.Claims;

namespace MasterApp.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetMasterId(this ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
        if (idClaim != null && Guid.TryParse(idClaim.Value, out var guid))
        {
            return guid;
        }
        
        throw new UnauthorizedAccessException("Master ID not found in token");
    }
}
