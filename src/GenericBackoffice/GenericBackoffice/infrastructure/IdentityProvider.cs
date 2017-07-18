using GenericBackoffice.models;
using GenericBackoffice.models.auth;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    item.DynamicProperties[nameof(User.username)] as string == username
                    &&
                    item.DynamicProperties["password"] as string == passwordHash);
            return BuildUser(user);
        }

        private static User BuildUser(GenericItem item)
        {
            if (item == null)
                return null;

            return new User
            {
                username = item.DynamicProperties[nameof(User.username)] as string,
                displayName = item.DynamicProperties[nameof(User.displayName)] as string,
                roles = item.DynamicProperties[nameof(User.roles)] as string[]
            };
        }

        private static string HashPassword(string password)
        {
            var sha1 = new SHA1CryptoServiceProvider();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }
    }
}