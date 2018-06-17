using System.Collections.Generic;
using System.Threading.Tasks;
using Memstate.Test.EventfulTestDomain;
using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixture]
    public partial class LocalClientEventTests
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
        public async Task Subscribe_with_filter()
        {
            var eventsReceived = new List<Event>();
            var kant = await _client.Execute(new Create("Kant"));
            var wittgenstein = await _client.Execute(new Create("Wittgenstein"));

            //Listen for Deleted event for the Kant user
            await _client.Subscribe<Deleted>(eventsReceived.Add, new UserDeletedEventFilter(kant.Id));

            await _client.Execute(new Delete(wittgenstein.Id));

            //we shouldn't see a Deleted event for user Wittgenstein
            Assert.AreEqual(0, eventsReceived.Count);

            await _client.Execute(new Delete(kant.Id));

            //we should now have 1 Deleted event for user Kant
            Assert.AreEqual(1, eventsReceived.Count);
            Assert.AreEqual(kant.Id, (eventsReceived[0] as Deleted)?.UserId);
        }

        [Test]
        public async Task Subscribe_to_event()
        {
            var handledEvents = 0;

            await _client.Subscribe<Created>(e => handledEvents++);
            var user = await _client.Execute(new Create("Memstate"));
            await _client.Execute(new Delete(user.Id));
            
            Assert.AreEqual(1, handledEvents);
        }

        [Test]
        public async Task Subscribe_to_multiple_events()
        {
            var handledEvents = 0;

            await _client.Subscribe<Created>(e => handledEvents++);
            await _client.Subscribe<Deleted>(e => handledEvents++);

            var user = await _client.Execute(new Create("Memstate"));
            Assert.AreEqual(1, handledEvents);

            await _client.Execute(new Delete(user.Id));
            Assert.AreEqual(2, handledEvents);
        }

        [Test]
        public async Task Unsubscribe_from_an_event()
        {
            var eventsReceived = new List<Event>();
            

            await _client.Subscribe<Created>(eventsReceived.Add);
            await _client.Subscribe<Deleted>(eventsReceived.Add);

            await _client.Execute(new Create("Memstate"));

            Assert.AreEqual(1, eventsReceived.Count);

            await _client.Unsubscribe<Created>();

            await _client.Execute(new Create("Origo"));
            
            Assert.AreEqual(1, eventsReceived.Count);
        }
    }
}