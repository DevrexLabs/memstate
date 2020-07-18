using System;
using Memstate.Configuration;
using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void EngineSettingsAreBound()
        {
            int expected = 123;
            var key = "MEMSTATE_ENGINE_MAXBATCHSIZE";
            Environment.SetEnvironmentVariable(key, expected.ToString());
            var config = Config.CreateDefault();
            var engineSettings = config.GetSettings<EngineSettings>();
            Assert.AreEqual(expected, engineSettings.MaxBatchSize);
        }

        [Test]
        public void EnvironmentVariableWithUnderscoresAndMatchingCase()
        {
            string key = "Memstate.Postgres.Password";
            string varName = "Memstate_Postgres_Password";
            string value = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(varName, value);
            var config = Config.CreateDefault();
            Console.WriteLine(config.ConfigurationData.ToString());
            Assert.DoesNotThrow(() => config.ConfigurationData.Get(key));
        }

        [Test]
        public void EnvironmentVariableWithUnderscoresAndUpperCase()
        {
            string key = "Memstate.Postgres.Password";
            string varName = "MEMSTATE_POSTGRES_PASSWORD";
            string value = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(varName, value);
            var config = Config.CreateDefault();
            Console.WriteLine(config.ConfigurationData.ToString());
            Assert.DoesNotThrow(() => config.ConfigurationData.Get(key));
        }
    }
}