using NUnit.Framework;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Memstate.Test
{
    [TestFixture]
    public class BatcherTests
    {
        [Test]
        public async Task Batcher_terminates_when_disposed()
        {
            var batcher = new Batcher<int>(batch => Task.CompletedTask, 10, 10);
            await batcher.DisposeAsync();
        }

        [Test]
        public void TryTake_returns_false_when_marked_for_completion()
        {
            var blockingCollection = new BlockingCollection<int>();
            blockingCollection.CompleteAdding();
            var actual = blockingCollection.TryTake(out var _);
            Assert.False(actual);
        }
    }
}