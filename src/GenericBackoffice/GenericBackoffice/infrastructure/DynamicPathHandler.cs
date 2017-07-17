using System;
using System.Text.RegularExpressions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Template;

namespace GenericBackoffice.infrastructure
{
    public class DynamicPathHandler : DefaultODataPathHandler
    {
        private static Regex _pathRegex = new Regex(@"^(?:([^/]+)\/)?([^/(\\?%]+)", RegexOptions.Compiled);

        public override ODataPath Parse(string serviceRoot, string odataPath, IServiceProvider requestContainer)
        {
            return base.Parse(serviceRoot, _pathRegex.Replace(odataPath, "data"), requestContainer);
        }

        public override ODataPathTemplate ParseTemplate(string odataPathTemplate, IServiceProvider requestContainer)
        {
            return base.ParseTemplate(odataPathTemplate, requestContainer);
        }

        internal static Tuple<string, string> GetCollection(string path)
        {
            var match = _pathRegex.Match(path);
            var database = string.IsNullOrEmpty(match.Groups[1].Value) ? "data" : match.Groups[1].Value;
            var collection = match.Groups[2].Value;
            return new Tuple<string, string>(database, collection);
        }
    }
}