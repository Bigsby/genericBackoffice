using GenericBackoffice.infrastructure;
using GenericBackoffice.models;
using Microsoft.OData.Edm;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing.Conventions;

[assembly: OwinStartup(typeof(GenericBackoffice.Startup))]

namespace GenericBackoffice
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "appAuth",
                CookieHttpOnly = true,
                ExpireTimeSpan = TimeSpan.FromDays(1),
                SlidingExpiration = true
            });


            var config = new HttpConfiguration();
            config.MessageHandlers.Add(new MethodOverrideHandler());
            config.Routes.MapHttpRoute("api", "api/{controller}/{action}");
            ConfigureOdata(config);
            app.UseWebApi(config);
        }

        static void ConfigureOdata(HttpConfiguration config)
        {
            var routePrefix = "data";
            var routeName = "odata";

            config.MapODataServiceRoute(
                routeName,
                routePrefix,
                GetModel(),
                new DynamicPathHandler(),
                ODataRoutingConventions.CreateDefault());
            config.Count().Filter().Select().OrderBy();
            config.AddODataQueryFilter();
        }

        static IEdmModel GetModel()
        {
            var builder = new ODataModelBuilder();
            var itemType = builder.EntityType<GenericItem>();
            itemType.HasKey(i => i.id);
            itemType.HasDynamicProperties(i => i.DynamicProperties);
            builder.EntitySet<GenericItem>("data");
            return builder.GetEdmModel();
        }
    }
}
