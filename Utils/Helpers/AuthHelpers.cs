using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Utils.Exceptions;

namespace Utils.Helpers
{
    public static class AuthHelpers
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value
                     ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (id == null || !int.TryParse(id, out var userId))
                throw new UnauthorizedError("No se pudo identificar al usuario autenticado.");

            return userId;
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
            => user.IsInRole("Admin");

        public static void EnsureOwnership(this ClaimsPrincipal user, int resourceProfesionalId)
        {
            if (user.GetUserId() != resourceProfesionalId && !user.IsAdmin())
                throw new ForbiddenError("No tiene permisos para acceder a este recurso.");
        }
    }
}