using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Memstate.JsonNet
{
    using System.Reflection;

    public class PrivatePropertySetterContractResolver
        : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(nonPublic:true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }
}