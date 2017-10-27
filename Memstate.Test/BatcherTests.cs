using System.Collections.Concurrent;
using System.Collections.Generic;
using Xunit;

namespace Memstate.Tests
{
    public class BatcherTests
    {
        [Fact]
        public void Batcher_terminates_when_disposed()
        {
            var config = new Settings();
            var batcher = new Batcher<int>(config, 200);
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