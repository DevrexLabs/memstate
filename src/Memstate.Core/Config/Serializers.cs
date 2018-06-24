using System;
using System.Collections.Generic;

namespace Memstate
{
    internal class Serializers : Providers<ISerializer>
    {
        public const string Wire = "Memstate.Wire.WireSerializerAdapter, Memstate.Wire";
        public const string NewtonSoftJson = "Memstate.JsonNet.JsonSerializerAdapter, Memstate.JsonNet";

        public Serializers()
        {
            Register("Auto", AutoResolve);
            Register("Wire", s => InstanceFromTypeName(Wire,s));
            Register("NewtonSoft.Json", s => InstanceFromTypeName(NewtonSoftJson,s));
        }

        protected override IEnumerable<string> AutoResolutionCandidates()
        {
            yield return "Newtonsoft.Json";
            yield return "Wire";
        }
    }
}