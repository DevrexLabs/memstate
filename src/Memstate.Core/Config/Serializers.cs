using System;

namespace Memstate
{
    public class Serializers : Providers<ISerializer>
    {
        public const string Wire = "Memstate.Wire.WireSerializerAdapter, Memstate.Wire";
        public const string NewtonSoftJson = "Memstate.JsonNet.JsonSerializerAdapter, Memstate.JsonNet";

        public Serializers()
        {
            Register("Wire", s => InstanceFromTypeName(Wire,s));
            Register("NewtonSoft.Json", s => InstanceFromTypeName(NewtonSoftJson,s));
        }
    }
}