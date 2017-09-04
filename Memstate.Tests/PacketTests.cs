using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Tcp;
using Xunit;

namespace Memstate.Tests
{
    public class PacketTests
    {
        [Fact]
        public void IsTerminal_is_default_when_calling_constructor()
        {
            var packet = new Packet();
            Assert.True(packet.IsTerminal);
        }

        [Fact]
        public void IsTerminal_is_default_when_creating_from_factory_method()
        {
            var packet = Packet.Create(new byte[42], 42);
            Assert.True(packet.IsTerminal);
        }

        private static IEnumerable<object[]> PayloadSizes()
        {
            yield return new object[] { 0 };
            yield return new object[] { 127 };
            yield return new object[] { 4523 };
        }

        [MemberData(nameof(PayloadSizes))]
        [Theory]
        public async Task When_writing_to_stream_Size_property_corresponds_to_bytes_written(int payloadSize)
        {
            var packet = Packet.Create(new byte[payloadSize], 1);

            var memoryStream = new MemoryStream();
            await packet.WriteToAsync(memoryStream);
            Assert.Equal(memoryStream.Length, packet.Size);
        }

        [MemberData(nameof(PayloadSizes))]
        [Theory]
        public async void ReadAsync_returns_identical_packet(int payloadSize)
        {
            //Arrange
            var packet = Packet.Create(new byte[payloadSize], 1);
            var stream = new MemoryStream();
            await packet.WriteToAsync(stream);
            stream.Position = 0;
            var token = new CancellationToken();

            //Act
            var copy = await Packet.ReadAsync(stream, token);

            //Assert
            Assert.Equal(packet.Size, copy.Size);
            Assert.Equal(packet.MessageId, copy.MessageId);
            Assert.Equal(packet.Info, copy.Info);
            Assert.Equal(packet.Payload.Length, copy.Payload.Length);
        }

        [MemberData(nameof(PayloadSizes))]
        [Theory(Skip = "skip")]
        public async void When_stream_is_blocked_cancelling_throws_a_TaskCancelledException(int payloadSize)
        {
            //Arrange
            var packet = Packet.Create(new byte[payloadSize], 1);
            var stream = new MemoryStream();
            await packet.WriteToAsync(stream);
            stream.SetLength(stream.Length - 3);
            var cancellationSource = new CancellationTokenSource();
            stream.Position = 0;

            //Act
            var task = Packet.ReadAsync(stream, cancellationSource.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(5));

            //Assert
            Assert.False(task.IsCompleted);
            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                cancellationSource.Cancel();
                await task;
            });

            Assert.True(task.IsCompleted);
            Assert.True(task.IsFaulted);
            
        }

    }
}