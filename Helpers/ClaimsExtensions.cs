using System.Security.Claims;

namespace Calendar.Helpers
{
    public static class ClaimsExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new UnauthorizedAccessException("Token inválido");

            return int.Parse(claim.Value);
        }
    }
}
