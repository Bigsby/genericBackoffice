using System.Linq;

namespace GenericBackoffice.models.auth
{
    public class Role
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public Permission[] permissions { get; set; }

        public static Role FromGenericItem(GenericItem item)
        {
            return new Role
            {
                id = item.id,
                displayName = item.GetPropertyValue<string>(nameof(displayName)),
                permissions = item.GetPropertyValue(nameof(permissions), new GenericItem[0])?
                    .Select(i => Permission.FromGenericItem(i)).ToArray()
            };
        }
    }
}