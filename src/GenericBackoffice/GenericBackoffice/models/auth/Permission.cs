namespace GenericBackoffice.models.auth
{
    public class Permission
    {
        public string database { get; set; }
        public string collection { get; set; }
        public bool write { get; set; }

        public static Permission FromGenericItem(GenericItem item)
        {
            return new Permission
            {
                database = item.GetPropertyValue<string>(nameof(database)),
                collection = item.GetPropertyValue<string>(nameof(collection)),
                write = item.GetPropertyValue(nameof(write), false)
            };
        }
    }
}