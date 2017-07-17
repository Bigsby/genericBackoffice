using System.Collections.Generic;

namespace GenericBackoffice.models
{
    public class GenericItem
    {
        public string id { get; set; }

        public IDictionary<string, object> DynamicProperties { get; set; }
    }
}