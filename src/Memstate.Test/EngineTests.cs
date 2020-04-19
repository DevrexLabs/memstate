using System;
using FakeItEasy;
using System.Threading.Tasks;
using NUnit.Framework;
using Memstate.Configuration;

namespace Memstate.Test
{
    [TestFixture]
    public class EngineTests
    {
        private IJournalSubscriptionSource _fakeSubscriptionSource;
        private IJournalSubscription _fakeSubscription;
        private IJournalWriter _fakeJournalWriter;
        private Engine<Object> _engine;
        private int _nextRecordNumber;

        [SetUp]
        public void Setup()
        {
            _fakeSubscriptionSource = A.Fake<IJournalSubscriptionSource>();
            _fakeSubscription = A.Fake<IJournalSubscription>();
            _fakeJournalWriter = A.Fake<IJournalWriter>();

            _nextRecordNumber = DateTime.Now.Millisecond;

            A.CallTo(() => _fakeSubscriptionSource.Subscribe(_nextRecordNumber, A<Action<JournalRecord>>._))
                .Returns(_fakeSubscription);

            var settings = Config.Current.GetSettings<EngineSettings>();
            _engine = new Engine<Object>(settings, new Object(), _fakeSubscriptionSource, _fakeJournalWriter, _nextRecordNumber);
        }

        [Test]
        public void Constructor_subscribes_to_journal_records_from_correct_recordNumber()
        {
            A.CallTo(() => _fakeSubscriptionSource
                    .Subscribe(_nextRecordNumber, A<Action<JournalRecord>>._))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task Subscription_is_disposed_when_engine_is_disposed()
        {
            await _engine.DisposeAsync().ConfigureAwait(false);

            A.CallTo(() => _fakeSubscription.Dispose())
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task Writer_is_disposed_when_engine_is_disposed()
        {
            await _engine.DisposeAsync().ConfigureAwait(false);
            A.CallTo(() => _fakeJournalWriter.DisposeAsync())
                .MustHaveHappenedOnceExactly();
        }

    }
}