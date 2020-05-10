using System.Collections.Generic;

namespace Memstate
{
    internal class Serializers : Providers<ISerializer>
    {
        /// <summary>
        /// Take the first available serializer
        /// as determined by <see cref="AutoResolutionCandidates"/>
        /// </summary>
        public const string Auto = "Auto";

        /// <summary>
        /// Wire binary serializer
        /// </summary>
        public const string Wire = "Wire";

        /// <summary>
        /// NewtonSoft JSON serializer
        /// </summary>
        public const string NewtonsoftJson = "Newtonsoft.Json";

        private const string WireTypeName = "Memstate.Wire.WireSerializerAdapter, Memstate.Wire";
        private const string NewtonSoftJsonTypeName = "Memstate.JsonNet.JsonSerializerAdapter, Memstate.JsonNet";

        public Serializers()
        {
            Register("Auto", AutoResolve);
            Register("BinaryFormatter", () => InstanceFromTypeName(nameof(BinaryFormatterAdapter)));
            Register("Wire", () => InstanceFromTypeName(WireTypeName));
            Register("NewtonSoft.Json", () => InstanceFromTypeName(NewtonSoftJsonTypeName));
        }

        protected override IEnumerable<string> AutoResolutionCandidates()
        {
            yield return "Wire";
            yield return "Newtonsoft.Json";
            yield return "BinaryFormatter";
        }
    }
}