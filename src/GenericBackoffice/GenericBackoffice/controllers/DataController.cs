using GenericBackoffice.infrastructure;
using GenericBackoffice.models;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace GenericBackoffice.controllers
{
    public class DataController : ODataController
    {
        public IQueryable<GenericItem> Get()
        {
            var path = GetCollection();
            return DataProvider.GetItems(path.Item1, path.Item2).AsQueryable();
        }

        public GenericItem Get([FromODataUri]string key)
        {
            var path = GetCollection();
            return DataProvider.GetItem(path.Item1, path.Item2, key);
        }

        public IHttpActionResult Post(GenericItem item)
        {
            var path = GetCollection();

            var result = DataProvider.SaveItem(path.Item1, path.Item2, item);
            if (result)
                return Created($"data/{path.Item1}/{path.Item2}('{item.id}')", item);
            return InternalServerError(result.Error);
        }

        public IHttpActionResult Delete([FromODataUri]string key)
        {
            var path = GetCollection();
            var result = DataProvider.DeleteItem(path.Item1, path.Item2, key);
            if (result)
                return Ok();
            return InternalServerError(result.Error);
        }

        private Tuple<string, string> GetCollection()
        {
            return DynamicPathHandler.GetCollection((string)RequestContext.RouteData.Values["odataPath"]);
        }
    }
}