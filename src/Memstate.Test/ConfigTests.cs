using System;
using Memstate.Configuration;
using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void CanGetDefaultSerializer()
        {
            var config = Config.Reset();
            var serializer = config.CreateSerializer();
            Assert.NotNull(serializer);
        }

        [Test]
        public void EnvironmentVariableWithUnderscoresAndMatchingCase()
        {
            string key = "Memstate.Postgres.Password";
            string varName = "Memstate_Postgres_Password";
            string value = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(varName, value);
            var config = Config.Reset();
            Console.WriteLine(config.Settings.ToString());
            Assert.DoesNotThrow(() => config.Settings.Get(key));
        }

        [Test]
        public void EnvironmentVariableWithUnderscoresAndUpperCase()
        {
            string key = "Memstate.Postgres.Password";
            string varName = "MEMSTATE_POSTGRES_PASSWORD";
            string value = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(varName, value);
            var config = Config.Reset();
            Console.WriteLine(config.Settings.ToString());
            Assert.DoesNotThrow(() => config.Settings.Get(key));
        }
    }
}