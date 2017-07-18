using System.Security.Claims;
using System.Security.Principal;

namespace GenericBackoffice.models.auth
{
    public class User
    {
        public string username { get; set; }
        public string displayName { get; set; }
        public string[] roles { get; set; }
        public ClaimsIdentity ToIdentity(string authorityType)
        {
            return new ClaimsIdentity(
                        new[] {
                            new Claim(ClaimTypes.Name, displayName),
                            new Claim(ClaimTypes.NameIdentifier, username)
                            // TODO add roles
                        },
                        authorityType);
        }

        public static User FromIdentity(IIdentity identity)
        {
            if (!identity.IsAuthenticated)
                return null;

            var claimsIdentity = identity as ClaimsIdentity;
            var user = new User();
            foreach (var claim in claimsIdentity.Claims)
                switch (claim.Type)
                {
                    case ClaimTypes.Name:
                        user.displayName = claim.Value;
                        break;

                    case ClaimTypes.NameIdentifier:
                        user.username = claim.Value;
                        break;
                }

            return user;
        }
    }
}