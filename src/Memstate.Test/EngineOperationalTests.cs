using FakeItEasy;
using Memstate.Models;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Memstate.Models.KeyValue;

namespace Memstate.Test
{
    [TestFixture]
    public class EngineOperationalTests
    {
        private readonly MemstateSettings _settings;
        private FakeSubscriptionSource _fakeSource;
        private DateTime _now;
        private Engine<KeyValueStore<int>> _engine;

        public EngineOperationalTests()
        {
            _settings = new MemstateSettings().WithInmemoryStorage();
        }

        [Test]
        public void Engine_halts_when_gap_in_record_sequence()
        {
            // Arrange
            _settings.AllowBrokenSequence = false;
            Initialize();

            // apply records with a gap in the sequence
            _fakeSource.RecordHandler.Invoke(new JournalRecord(0, _now, new Set<int>("key",42)));
            _fakeSource.RecordHandler.Invoke(new JournalRecord(2, _now, new Set<int>("a", 10)));

            // engine should now be stopped and throw if transactions are attempted
            Assert.Throws<Exception>(() => _engine.Execute(new Count<int>()));
        }

        [Test]
        public async Task Engine_accepts_gap_in_record_sequence_when_allowed()
        {
            // Arrange
            _settings.AllowBrokenSequence = true;
            Initialize();

            // apply records with a sequence in the gap
            _fakeSource.RecordHandler.Invoke(new JournalRecord(0, _now, new Set<int>("c", 200)));
            _fakeSource.RecordHandler.Invoke(new JournalRecord(2, _now, new Set<int>("d",300)));

            //Wait for the second record to be applied
            await _engine.EnsureVersionAsync(2).ConfigureAwait(false);

            //we should be able to execute a query
            var numKeys = await _engine.ExecuteAsync(new Count<int>()).ConfigureAwait(false);
            Assert.AreEqual(2, numKeys);
            Assert.AreEqual(2, _engine.LastRecordNumber);
        }

        private void Initialize()
        {
            _fakeSource = new FakeSubscriptionSource();
            var dummyModel = new KeyValueStore<int>();
            var dummyWriter = A.Fake<IJournalWriter>();
            _now = DateTime.Now;

            _engine = new Engine<KeyValueStore<int>>(_settings, dummyModel, _fakeSource, dummyWriter, 0);
        }

        private class FakeSubscriptionSource : IJournalSubscriptionSource
        {
            internal Action<JournalRecord> RecordHandler { get; private set; }

            public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
            {
                RecordHandler = handler;
                return new JournalSubscription(_ => { }, from, _ => { });
            }
        }
    }
}