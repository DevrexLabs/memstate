using System.Collections.Generic;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Models.Redis;
using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixture]
    public class EngineStateTests
    {
        private Engine<RedisModel> _engine;

        [SetUp]
        public void Init()
        {
            Config.Reset().UseInMemoryFileSystem();
            _engine = Engine.Build<RedisModel>();
        }

        [TearDown]
        public Task Teardown()
        {
            return _engine.DisposeAsync();

        }

        [Test]
        public void Initial_state_is_NotStarted()
        {
            Assert.AreEqual(EngineState.NotStarted, _engine.State);
        }
        
        [Test]
        public async Task Starting_and_waiting_transitions_to_Running_state()
        {
            var transitions = new List<(EngineState, EngineState)>();
            
            _engine.StateChanged += (oldState, newState) 
                => transitions.Add((oldState, newState));
            
            await _engine.Start(waitUntilReady: true);

            var expected = new[]
            {
                (EngineState.NotStarted, EngineState.Loading),
                (EngineState.Loading, EngineState.Running)
            };
            CollectionAssert.AreEquivalent(expected, transitions);
        }

    }
}