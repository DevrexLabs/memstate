using System;
using FakeItEasy;
using Xunit;

namespace Memstate.Tests
{
    public class EngineTests
    {
        private readonly IJournalSubscriptionSource _fakeSubscriptionSource;
        private readonly IJournalSubscription _fakeSubscription;
        private readonly IJournalWriter _fakeJournalWriter;
        private readonly Engine<Object> _engine;
        private readonly int _nextRecordNumber;
        
        public EngineTests()
        {
            _fakeSubscriptionSource = A.Fake<IJournalSubscriptionSource>();
            _fakeSubscription = A.Fake<IJournalSubscription>();
            _fakeJournalWriter = A.Fake<IJournalWriter>();

            _nextRecordNumber = DateTime.Now.Millisecond;

            A.CallTo(() => _fakeSubscriptionSource.Subscribe(_nextRecordNumber, A<Action<JournalRecord>>._))
                .Returns(_fakeSubscription);

            var config = new Settings();
            _engine = new Engine<Object>(config, new Object(), _fakeSubscriptionSource, _fakeJournalWriter, _nextRecordNumber);
        }


        [Fact]
        public void Constructor_subscribes_to_journal_records_from_correct_recordNumber()
        {
            A.CallTo(() => _fakeSubscriptionSource.Subscribe(_nextRecordNumber, A<Action<JournalRecord>>._))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Subscription_is_disposed_when_engine_is_disposed()
        {
            _engine.Dispose();

            A.CallTo(() => _fakeSubscription.Dispose())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Writer_is_disposed_when_engine_is_disposed()
        {
            _engine.Dispose();
            A.CallTo(() => _fakeJournalWriter.Dispose())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

    }
}