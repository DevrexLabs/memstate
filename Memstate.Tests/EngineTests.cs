using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using Xunit;

namespace Memstate.Tests
{
    public class EngineTests
    {
        private readonly IJournalSubscriptionSource _fakeSubscriptionSource;
        private readonly IJournalSubscription _fakeSubscription;

        public EngineTests()
        {
            _fakeSubscriptionSource = A.Fake<IJournalSubscriptionSource>();
            _fakeSubscription = A.Fake<IJournalSubscription>();
        }

        private static IEnumerable<object[]> RecordNumbers()
        {
            return new [] {0, 1, 3, 3, 4, 5}
                .Select(i => new object[] {i});
        }

        [Theory, MemberData(nameof(RecordNumbers))]
        public void Constructor_subscribes_to_journal_records_from_correct_recordNumber(int nextRecordNumber)
        {
            var config = new Config();
            var engine  = new Engine<Object>(config, new Object(), _fakeSubscriptionSource, A.Fake<IJournalWriter>(), nextRecordNumber);
            A.CallTo(() => _fakeSubscriptionSource.Subscribe(nextRecordNumber, A<Action<JournalRecord>>._))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Subscription_is_disposed_when_engine_is_disposed()
        {
            int nextRecordNumber = 0;
            A.CallTo(() => _fakeSubscriptionSource.Subscribe(nextRecordNumber, A<Action<JournalRecord>>._))
                .Returns(_fakeSubscription);

            var config = new Config();
            var engine = new Engine<Object>(config, new Object(), _fakeSubscriptionSource, A.Fake<IJournalWriter>(), nextRecordNumber);
            engine.Dispose();

            A.CallTo(() => _fakeSubscription.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }

    }
}