using System;
using System.Collections.Generic;
using System.Test;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Models.Redis;
using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixtureSource(typeof(TestConfigurations),  nameof(TestConfigurations.All))]
    public class EngineStateTests
    {
        private Engine<RedisModel> _engine;
        private readonly Config _config;

        public EngineStateTests(Config config)
        {
            Console.WriteLine(config);
            _config = config;
        }

        [SetUp]
        public void Init()
        {
            _engine = Engine.Build<RedisModel>(_config);
        }

        [TearDown]
        public Task Teardown() => _engine.DisposeAsync();

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
            
            await _engine.Start();

            var expected = new[]
            {
                (EngineState.NotStarted, EngineState.Loading),
                (EngineState.Loading, EngineState.Running)
            };
            CollectionAssert.AreEquivalent(expected, transitions);
        }

    }
}