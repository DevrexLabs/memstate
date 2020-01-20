using System;
using System.Collections.Generic;
using Memstate.Configuration;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Memstate.Test.EngineBuilderTests
{
    [TestFixture]
    class BuildTests
    {
        internal class TestSerializers : Serializers
        {
            internal TestSerializers()
            {
                RegisteredProviders.Remove("Wire");
                RegisteredProviders.Remove("NewtonSoft.Json");
            }
        }

        [Test]
        public async Task WhenNoSerializers_BuildShould_ReturnErrorMessageContainingSuggestedPackagesToReference()
        {
            // pretend this is a new users project and no defaults could be loaded
            // remove the default storage providers that will be picked up 
            // because this test project  references Wire and JsonNet,
            var config = Config.Reset();
            config.Serializers = new TestSerializers();

            try
            {
                var engine = await new EngineBuilder().Build<List<int>>();
                Assert.Fail("Should not get here");
            }
            catch (Exception ex)
            {
                StringAssert.Contains("Please check to see if you need to add a reference to 'Memstate.Wire', or 'Memstate.JsonNet'. Adding any of these two nuget packages will automatically use either package for serialisation.", ex.Message);
            }
        }
    }
}
