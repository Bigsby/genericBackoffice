using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace GenericBackoffice.infrastructure
{
    public class GenericAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var (database, collection) = DynamicPathHandler.GetDatabaseCollection((string)actionContext.RequestContext.RouteData.Values["odataPath"]);
            var isWrite = actionContext.Request.Method.Method.ToUpperInvariant() != "GET";

            if (database == "public" && !isWrite)
                return true;

            var permissions = IdentityProvider.GetPermissions();

            var databasePermissions = permissions.Where(p => p.database == database && string.IsNullOrEmpty(p.collection));
            var collectionPersmissions = permissions.Where(p => p.database == database && p.collection == collection);

            return
                databasePermissions.Any(p => !isWrite || p.write)
                ||
                collectionPersmissions.Any(p => !isWrite || p.write);
        }
    }
}