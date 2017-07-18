using System.Collections.Generic;

namespace GenericBackoffice.models
{
    public class GenericItem
    {
        public string id { get; set; }

        public IDictionary<string, object> DynamicProperties { get; set; }

        internal T GetPropertyValue<T>(string propertyName)
        {
            return GetPropertyValue(propertyName, default(T));
        }

        public T GetPropertyValue<T>(string propertyName, T defaultValue)
        {
            return DynamicProperties.ContainsKey(propertyName) ?
                (T)DynamicProperties[propertyName]
                :
                defaultValue;
        }
    }
}