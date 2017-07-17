# Generic Backoffice
A generic web site/application back-office. Using:
- Web API (OWIN).
- OData Open Type.
- Angular 4 (CLI).

This is a *natural* consequence of the [Generic OData Web API Controller](https://github.com/Bigsby/schemaless-Odata) project. So, after following the steps on that project, these were executed:

1. Add *databases* (folders) to OData Path.
   
   The purpose is to have different types of data:
   - Admin data: Users and roles
   - Backoffice data: Metadata of the editable data.
   - Actual data.

   For that, each type of data will be in its own folder, semantically, *database* like in [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/documentdb-introduction).

   1. Change **DynamicPathHandler** to accept an *extra* segment:
        ```csharp
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
        ```
        - *"data"* is the *default* database so it can be omitted.


    2. Change **DataProvider** to accept a *database* (folder):
        ```csharp
        using GenericBackoffice.models;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Linq;
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Web.Hosting;

        namespace GenericBackoffice.infrastructure
        {
            internal static class DataProvider
            {
                public static IEnumerable<GenericItem> GetItems(string database, string collection)
                {
                    var folderPath = HostingEnvironment.MapPath($"~/App_Data/{database}/{collection}");
                    if (!Directory.Exists(folderPath))
                        yield break;

                    foreach (var filePath in Directory.GetFiles(folderPath))
                        yield return ReadItem(filePath, Path.GetFileNameWithoutExtension(filePath));
                }

                public static GenericItem GetItem(string database, string collection, string id)
                {
                    var filePath = HostingEnvironment.MapPath($"~/App_Data/{database}/{collection}/{id}.json");
                    if (!File.Exists(filePath))
                        return null;

                    return ReadItem(filePath, id);
                }

                public static DataResult SaveItem(string database, string collection, GenericItem item)
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(item.DynamicProperties);
                        var filePath = HostingEnvironment.MapPath($"~/App_Data/{database}/{collection}/{item.id}.json");

                        if (File.Exists(filePath))
                            File.Delete(filePath);

                        File.WriteAllText(filePath, json);
                        return DataResult.Successul;
                    }
                    catch (Exception ex)
                    {
                        return DataResult.Fail(ex);
                    }
                }

                public static DataResult DeleteItem(string database, string collection, string id)
                {
                    try
                    {
                        var filePath = HostingEnvironment.MapPath($"~/App_Data/{database}/{collection}/{id}.json");
                        File.Delete(filePath);
                        return DataResult.Successul;
                    }
                    catch (Exception ex)
                    {
                        return DataResult.Fail(ex);
                    }
                }

                private static GenericItem ReadItem(string filePath, string id)
                {
                    var fileContent = File.ReadAllText(filePath);
                    return new GenericItem
                    {
                        id = id,
                        DynamicProperties = ConvertDynamicProperties(id, JObject.Parse(fileContent))
                    };
                }

                private static IDictionary<string, object> ConvertDynamicProperties(string id, JObject token)
                {
                    var result = new Dictionary<string, object>();
                    if (null == token)
                        return result;

                    foreach (var prop in token?.Properties())
                    {
                        if (null == prop.Value) continue;
                        result[prop.Name] = ConvertValue(prop.Name, id, prop.Value);
                    }

                    return result;
                }

                private static object ConvertValue(string propertyName, string parentId, JToken token)
                {
                    switch (token.Type)
                    {
                        case JTokenType.Comment:
                        case JTokenType.Property:
                        case JTokenType.Constructor:
                        case JTokenType.None:
                        case JTokenType.Undefined:
                        case JTokenType.Null:
                            return null;
                        case JTokenType.Object:
                            return BuildItemFromToken(token, $"{parentId}_{propertyName}");
                        case JTokenType.Array:
                            return ConvertArray(parentId, (JArray)token);
                        case JTokenType.Integer:
                            return token.ToObject<int>();
                        case JTokenType.Float:
                            return token.ToObject<double>();
                        case JTokenType.String:
                        case JTokenType.Uri:
                        case JTokenType.Raw:
                        default:
                            return token.ToObject<string>();
                        case JTokenType.Boolean:
                            return token.ToObject<bool>();
                        case JTokenType.Date:
                            return token.ToObject<DateTime>();
                        case JTokenType.Bytes:
                            return token.ToObject<byte[]>();
                        case JTokenType.Guid:
                            return token.ToObject<Guid>();
                        case JTokenType.TimeSpan:
                            return token.ToObject<TimeSpan>();
                    }
                }

                private static object ConvertArray(string parentId, JArray array)
                {
                    if (array.Count == 0)
                        return new string[0];

                    switch (array.First.Type)
                    {
                        case JTokenType.Integer:
                            return array.ToObject<int[]>();
                        case JTokenType.Float:
                            return array.ToObject<double[]>();
                        case JTokenType.Boolean:
                            return array.ToObject<bool[]>();
                        default:
                        case JTokenType.String:
                        case JTokenType.Undefined:
                        case JTokenType.Null:
                        case JTokenType.Raw:
                            return array.ToObject<string[]>();
                        case JTokenType.Date:
                            return array.ToObject<DateTime[]>();
                        case JTokenType.Bytes:
                            return array.ToObject<byte[]>();
                        case JTokenType.Guid:
                            return array.ToObject<Guid[]>();
                        case JTokenType.Uri:
                            return array.ToObject<Uri[]>();
                        case JTokenType.TimeSpan:
                            return array.ToObject<TimeSpan[]>();
                        case JTokenType.Object:
                            return ConvertObjectArray(parentId, array);
                    }
                }

                private static GenericItem BuildItemFromToken(JToken token, string computedId)
                {
                    var id = token.Value<string>("id") ?? computedId;
                    return new GenericItem
                    {
                        id = id,
                        DynamicProperties = ConvertDynamicProperties(id, (JObject)token)
                    };
                }

                private static IEnumerable<GenericItem> ConvertObjectArray(string parentId, JArray array)
                {
                    var count = 1;
                    foreach (var item in array)
                        yield return BuildItemFromToken(item, $"{parentId}_{count++}");
                }
            }

            internal class DataResult
            {
                public bool Success { get; private set; }
                public Exception Error { get; private set; }

                public static DataResult Successul => new DataResult { Success = true };

                public static DataResult Fail(Exception ex) => new DataResult { Error = ex };

                public static implicit operator bool(DataResult result)
                { return result.Success; }

                private DataResult() { }
            }
        }
        ```
    
    3. Change **DataController** to retrieve and pass *database* value:
        ```csharp
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
        ```
    4. Add folder structure in *App_Data* folder, e.g:
        - *admin*
            - *users*
                - *one.json*
                - *two.json*
        - *data*
            - *collection*
                - *one.json*
                - *two.json*

    5. Run and test, e.g.:
        1. Browse to [http://localhost:PORT/data/collection](http://localhost:60371/data/collection)
        2. Browse to [http://localhost:PORT/data/admin/users](http://localhost:60371/data/admin/users)

2. Add Authentication - WIP