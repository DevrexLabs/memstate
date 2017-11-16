namespace Memstate.Tests
{
    using System;
    using System.IO;

    using Xunit;

    public class InMemoryFileSystemTests
    {
        private readonly InMemoryFileSystem _sut;

        private readonly string _fileName;

        public InMemoryFileSystemTests()
        {
            _sut = new InMemoryFileSystem();
            _fileName = Guid.NewGuid().ToString();
        }

        [Fact]
        public void OpenNonExistingFileThrows()
        {
            Assert.Throws<FileNotFoundException>(() => _sut.OpenRead(_fileName));
        }

        [Fact]
        public void OpenLockedFileThrows()
        {
            var stream = _sut.OpenAppend(_fileName);
            Assert.ThrowsAny<Exception>(() => _sut.OpenRead(_fileName));
        }

        [Fact]
        public void BytesReadEqualBytesWritten()
        {
            const int NumBytes = 200;

            var writeStream = _sut.OpenAppend(_fileName);
            WriteRandomBytes(writeStream, NumBytes, out var bytesWritten);
            writeStream.Dispose();

            var readStream = _sut.OpenRead(_fileName);
            var bytesRead = new byte[NumBytes];
            readStream.Read(bytesRead, 0, NumBytes);

            Assert.Equal(bytesWritten, bytesRead);
            Assert.Equal(NumBytes, readStream.Length);
            readStream.Dispose();
        }

        private void WriteRandomBytes(Stream stream, int count, out byte[] bytes)
        {
            var rand = new Random();
            bytes = new byte[count];
            rand.NextBytes(bytes);
            stream.Write(bytes, 0, count);
        }
    }
}
