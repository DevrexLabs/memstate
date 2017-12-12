using Memstate.Wire;

namespace Memstate
{
    public class Serializers : Providers<ISerializer>
    {
        public Serializers()
        {
            Register("Wire", settings => new WireSerializerAdapter(settings));
        }
    }
}