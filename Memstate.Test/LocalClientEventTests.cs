using System.Threading.Tasks;
using Memstate.Test.EventfulTestDomain;
using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixture]
    public class LocalClientEventTests
    {
        private MemstateSettings _settings;
        private LocalClient<UsersModel> _client;

        [SetUp]
        public void SetUp()
        {
            _settings = new MemstateSettings();
            
            _settings.WithInmemoryStorage();
            
            _client = new LocalClient<UsersModel>(() => new UsersModel(), _settings);
        }
        
        [Test]
        public async Task Can_raise_events()
        {
            var handledEvents = 0;

            _client.Events.Raised += e => handledEvents += 1;

            await _client.ExecuteAsync(new Create("Memstate"));

            Assert.AreEqual(1, handledEvents);
        }

        [Test]
        public async Task Subscribe_to_event()
        {
            var handledEvents = 0;

            _client.Events.Subscribe<Created>();

            _client.Events.Raised += e => handledEvents++;

            var user = await _client.ExecuteAsync(new Create("Memstate"));
            
            await _client.ExecuteAsync(new Delete(user.Id));
            
            Assert.AreEqual(1, handledEvents);
        }

        [Test]
        public async Task Subscribe_to_events()
        {
            var handledEvents = 0;

            _client.Events.Subscribe<Created>();
            _client.Events.Subscribe<Deleted>();

            _client.Events.Raised += e => handledEvents++;

            var user = await _client.ExecuteAsync(new Create("Memstate"));
            
            await _client.ExecuteAsync(new Delete(user.Id));
            
            Assert.AreEqual(2, handledEvents);
        }

        [Test]
        public async Task Unsubscribe_from_an_event()
        {
            var handledEvents = 0;
            
            _client.Events.Raised += e => handledEvents++;

            _client.Events.Subscribe<Created>();
            _client.Events.Subscribe<Deleted>();

            await _client.ExecuteAsync(new Create("Memstate"));
            
            _client.Events.Unsubscribe<Created>();

            await _client.ExecuteAsync(new Create("Origo"));
            
            Assert.AreEqual(1, handledEvents);
        }

        [Test]
        public async Task Subscribe_to_event_with_specific_handler()
        {
            var handledEvents = 0;
            
            _client.Events.Subscribe<Created>(e => handledEvents++);

            await _client.ExecuteAsync(new Create("Memstate"));
            
            Assert.AreEqual(1, handledEvents);
        }
        
        [Test]
        public async Task Subscribe_to_events_with_specific_handler()
        {
            var handledEvents = 0;
            
            _client.Events.Subscribe<Created>(e => handledEvents++);
            _client.Events.Subscribe<Deleted>(e => handledEvents++);

            var user = await _client.ExecuteAsync(new Create("Memstate"));

            await _client.ExecuteAsync(new Delete(user.Id));
            
            Assert.AreEqual(2, handledEvents);
        }
    }
}