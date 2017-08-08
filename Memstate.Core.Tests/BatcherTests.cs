using System.Collections.Concurrent;
using Xunit;

namespace Memstate.Core.Tests
{
    public class BatcherTests
    {
        [Fact]
        public void Batcher_terminates_when_disposed()
        {
            var batcher = new Batcher<int>(200);
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