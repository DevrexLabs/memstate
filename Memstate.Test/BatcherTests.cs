namespace Memstate.Tests
{
    using System.Collections.Concurrent;
    using Xunit;

    public class BatcherTests
    {
        [Fact]
        public void Batcher_terminates_when_disposed()
        {
            var config = new MemstateSettings();
            var batcher = new Batcher<int>(config);
            batcher.Dispose();
        }

        [Fact]
        public void TryTake_returns_false_when_marked_for_completion()
        {
            var blockingCollection = new BlockingCollection<int>();
            blockingCollection.CompleteAdding();
            var actual = blockingCollection.TryTake(out var item);
            Assert.False(actual);
        }
    }
}