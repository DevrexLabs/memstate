namespace Memstate
{
    using Memstate.Wire;

    public class Serializers : Providers<ISerializer>
    {
        public Serializers()
        {
            Register("Wire", settings => new WireSerializerAdapter(settings));
        }
    }
}