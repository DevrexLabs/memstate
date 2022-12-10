using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Memstate;
using Memstate.Configuration;
using Serilog;
using SqlStreamStore;

namespace System.Test
{
    public class TestConfigurations : IEnumerable<object[]>
    {

        static TestConfigurations()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}]{Message:lj} {SourceContext}{NewLine}{Exception}")
                .MinimumLevel.Verbose()
                .CreateLogger();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return GetConfigurations()
                .Select(ToObjectArray)
                .GetEnumerator();
        }

        public IEnumerable<Config> GetConfigurations()
        {
            foreach (var serializerName in Serializers())
            {
                foreach (var providerName in ProviderNames())
                {
                    var cfg = Config.Reset();
                    cfg.SerializerName = serializerName;
                    cfg.UseInMemoryFileSystem();
                    var settings = cfg.GetSettings<EngineSettings>();
                    settings.WithRandomSuffixAppendedToStreamName();
                    cfg.StorageProviderName = providerName;
                    yield return cfg;
                }
            }
        }

        private IEnumerable<string> Serializers()
        {
            yield return "newtonsoft.json";
            yield return "wire";
        }

        protected virtual IEnumerable<string> ProviderNames()
        {
            yield return "file";
            yield return "eventstore";
        }

        private object[] ToObjectArray(object o)
        {
            return new[] { o };
        }

        public class Cluster : TestConfigurations
        {
            protected override IEnumerable<string> ProviderNames()
            {
                yield return "eventstore";
            }
        }

    }
}