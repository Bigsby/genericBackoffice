using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace GenericBackoffice.models.auth
{
    public class User
    {
        public string username { get; private set; }
        public string displayName { get; private set; }
        public string[] roles { get; private set; }

        public ClaimsIdentity ToIdentity(string authorityType)
        {
            return new ClaimsIdentity(GetClaims(), authorityType);
        }

        public static User FromIdentity(IIdentity identity)
        {
            if (!identity.IsAuthenticated)
                return null;

            var claimsIdentity = identity as ClaimsIdentity;
            if (null == claimsIdentity) return null;

            return new User
            {
                displayName = GetClaimValue(claimsIdentity.Claims, ClaimTypes.Name),
                username = GetClaimValue(claimsIdentity.Claims, ClaimTypes.NameIdentifier),
                roles = claimsIdentity.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value).ToArray()
            };
        }

        public static User FromGenericItem(GenericItem item)
        {
            if (item == null)
                return null;

            return new User
            {
                username = item.GetPropertyValue<string>(nameof(username)),
                displayName = item.GetPropertyValue<string>(nameof(displayName)),
                roles = item.GetPropertyValue(nameof(roles), new string[0])
            };
        }

        private static string GetClaimValue(IEnumerable<Claim> claims, string type)
        {
            return claims.FirstOrDefault(c => c.Type == type)?.Value;
        }

        private IEnumerable<Claim> GetClaims()
        {
            yield return new Claim(ClaimTypes.NameIdentifier, username);

            if (!string.IsNullOrEmpty(displayName))
                yield return new Claim(ClaimTypes.Name, displayName);

            foreach (var role in roles)
                yield return new Claim(ClaimTypes.Role, role);
        }
    }
}