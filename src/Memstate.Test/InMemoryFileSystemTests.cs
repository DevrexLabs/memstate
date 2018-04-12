using System;
using System.IO;
using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixture]
    public class InMemoryFileSystemTests
    {
        private InMemoryFileSystem _sut;

        private string _fileName;

        [SetUp]
        public void Setup()
        {
            _sut = new InMemoryFileSystem();
            _fileName = Guid.NewGuid().ToString();
        }

        [Test]
        public void OpenNonExistingFileThrows()
        {
            Assert.Throws<FileNotFoundException>(() => _sut.OpenRead(_fileName));
        }

        [Test]
        public void OpenLockedFileThrows()
        {
            var stream = _sut.OpenAppend(_fileName);
            Assert.Throws<IOException>(() => _sut.OpenRead(_fileName));
        }

        [Test]
        public void BytesReadEqualBytesWritten()
        {
            const int NumBytes = 200;

            var writeStream = _sut.OpenAppend(_fileName);
            WriteRandomBytes(writeStream, NumBytes, out var bytesWritten);
            writeStream.Dispose();

            var readStream = _sut.OpenRead(_fileName);
            var bytesRead = new byte[NumBytes];
            readStream.Read(bytesRead, 0, NumBytes);

            Assert.AreEqual(bytesWritten, bytesRead);
            Assert.AreEqual(NumBytes, readStream.Length);
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
