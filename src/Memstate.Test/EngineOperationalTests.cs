using Memstate.Models;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Memstate.Models.KeyValue;
using Memstate.Configuration;

namespace Memstate.Test
{
    [TestFixture]
    public class EngineOperationalTests
    {
        private EngineSettings _settings;
        private DateTime _now;
        private Engine<IKeyValueStore<int>> _engine;

        [SetUp]
        public void Setup()
        {
            var cfg = Config.CreateDefault();
            _settings = cfg.GetSettings<EngineSettings>();
            cfg.FileSystem = new InMemoryFileSystem();
        }

        [Test, Ignore("")]
        public async Task Engine_halts_when_gap_in_record_sequence()
        {
            // Arrange
            _settings.AllowBrokenSequence = false;
            await Initialize();

            // apply records with a gap in the sequence
            //_fakeSource.RecordHandler.Invoke(new JournalRecord(0, _now, new Set<int>("key",42)));
            //_fakeSource.RecordHandler.Invoke(new JournalRecord(2, _now, new Set<int>("a", 10)));

            // engine should now be stopped and throw if transactions are attempted
            Assert.Throws<Exception>(() => _engine.ExecuteUntyped(new Count<int>()));
            Assert.Fail("Fix after redesign");
        }

        [Test, Ignore("")]
        public async Task Engine_accepts_gap_in_record_sequence_when_allowed()
        {
            // Arrange
            _settings.AllowBrokenSequence = true;
            await Initialize();

            // apply records with a sequence in the gap
            //_fakeSource.RecordHandler.Invoke(new JournalRecord(0, _now, new Set<int>("c", 200)));
            //_fakeSource.RecordHandler.Invoke(new JournalRecord(2, _now, new Set<int>("d",300)));

            //Wait for the second record to be applied
            await _engine.EnsureVersion(2).ConfigureAwait(false);

            //we should be able to execute a query
            var numKeys = await _engine.Execute(new Count<int>()).NotOnCapturedContext();
            Assert.AreEqual(2, numKeys);
            Assert.AreEqual(2, _engine.LastRecordNumber);
            
            Assert.Fail("Fix after redesign");
        }

        private async Task Initialize()
        {
            _now = DateTime.Now;
            _engine = await Engine.Start<IKeyValueStore<int>>(new KeyValueStore<int>()); 
        }
    }
}