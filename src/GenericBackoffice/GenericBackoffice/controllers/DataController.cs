using GenericBackoffice.infrastructure;
using GenericBackoffice.models;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace GenericBackoffice.controllers
{
    public class DataController : ODataController
    {
        [GenericAuthorize]
        public IQueryable<GenericItem> Get()
        {
            var (database, collection) = GetDatabaseCollection();
            return DataProvider.GetItems(database, collection).AsQueryable();
        }

        [GenericAuthorize]
        public GenericItem Get([FromODataUri]string key)
        {
            var (database, collection) = GetDatabaseCollection();
            return DataProvider.GetItem(database, collection, key);
        }

        [GenericAuthorize]
        public IHttpActionResult Post(GenericItem item)
        {
            var (database, collection) = GetDatabaseCollection();

            var result = DataProvider.SaveItem(database, collection, item);
            if (result)
                return Created($"data/{database}/{collection}('{item.id}')", item);
            return InternalServerError(result.Error);
        }

        [GenericAuthorize]
        public IHttpActionResult Delete([FromODataUri]string key)
        {
            var (database, collection) = GetDatabaseCollection();
            var result = DataProvider.DeleteItem(database, collection, key);
            if (result)
                return Ok();
            return InternalServerError(result.Error);
        }

        private (string, string) GetDatabaseCollection()
        {
            return DynamicPathHandler.GetDatabaseCollection((string)RequestContext.RouteData.Values["odataPath"]);
        }
    }
}