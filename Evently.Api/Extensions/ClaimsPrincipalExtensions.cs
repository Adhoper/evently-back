using System.Security.Claims;

namespace Evently.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(
            this ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var id))
            {
                throw new UnauthorizedAccessException(
                    "No fue posible identificar al usuario.");
            }

            return id;
        }
    }
}