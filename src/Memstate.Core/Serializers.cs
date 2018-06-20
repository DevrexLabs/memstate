using System;

namespace Memstate
{
    public class Serializers : Providers<ISerializer>
    {
        public Serializers()
        {
            Register("Wire", Wire());
            Register("NewtonSoft.Json", Newtonsoft());
        }

        private Func<MemstateSettings,ISerializer> Wire()
        {
            var type = Type.GetType("Memstate.Wire.WireSerializerAdapter, Memstate.Wire");
            return s => (ISerializer) Activator.CreateInstance(type, s);
        }
        private Func<MemstateSettings, ISerializer> Newtonsoft()
        {
            var type = Type.GetType("Memstate.JsonNet.JsonSerializerAdapter, Memstate.JsonNet");
            return s => (ISerializer)Activator.CreateInstance(type, s);

        }
    }
}