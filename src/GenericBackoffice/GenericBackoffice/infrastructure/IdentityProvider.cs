using GenericBackoffice.models.auth;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace GenericBackoffice.infrastructure
{
    internal static class IdentityProvider
    {
        public static User GetUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                return null;

            var passwordHash = HashPassword(password);
            var user = DataProvider.GetItems("admin", "users")
                .FirstOrDefault(item =>
                    item.GetPropertyValue<string>(nameof(User.username)) == username
                    &&
                    item.GetPropertyValue<string>("password") == passwordHash);
            return User.FromGenericItem(user);
        }

        public static Permission[] GetPermissions()
        {
            var user = User.FromIdentity(HttpContext.Current.GetOwinContext().Authentication.User.Identity);

            return user?.roles?
                .Select(r => Role.FromGenericItem(DataProvider.GetItem("admin", "roles", r)))
                .SelectMany(r => r.permissions).ToArray();
        }

        private static string HashPassword(string password)
        {
            var sha1 = new SHA1CryptoServiceProvider();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }
    }
}