using System.Collections.Generic;

namespace Memstate
{
    internal class Serializers : Providers<ISerializer>
    {
        /// <summary>
        /// Take the first available serializer in the order: Newtonsoft.Json, Wire.
        /// </summary>
        public const string AUTO = "Auto";

        /// <summary>
        /// Wire binary serializer
        /// </summary>
        public const string WIRE = "Wire";

        /// <summary>
        /// NewtonSoft JSON serializer
        /// </summary>
        public const string NEWTONSOFT_JSON = "Newtonsoft.Json";

        private const string Wire = "Memstate.Wire.WireSerializerAdapter, Memstate.Wire";
        private const string NewtonSoftJson = "Memstate.JsonNet.JsonSerializerAdapter, Memstate.JsonNet";

        public Serializers()
        {
            Register("Auto", AutoResolve);
            Register("BinaryFormatter", () => InstanceFromTypeName(nameof(BinaryFormatterAdapter)));
            Register("Wire", () => InstanceFromTypeName(Wire));
            Register("NewtonSoft.Json", () => InstanceFromTypeName(NewtonSoftJson));
        }

        protected override IEnumerable<string> AutoResolutionCandidates()
        {
            yield return "BinaryFormatter";
            yield return "Newtonsoft.Json";
            yield return "Wire";
        }
    }
}