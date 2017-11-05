namespace Memstate.Tests
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using Xunit;

    public class BatcherTests
    {
        [Fact]
        public async Task Batcher_terminates_when_disposed()
        {
            var config = new MemstateSettings();
            var batcher = new Batcher<int>(config, batch => { });
            await batcher.DisposeAsync().ConfigureAwait(false);
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