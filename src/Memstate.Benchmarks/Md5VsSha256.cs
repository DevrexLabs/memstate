using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

namespace Memstate.Benchmarks
{
    /// <summary>
    /// This is the sample from the getting started guide of the benchmarkdotnet documentation
    /// at http://benchmarkdotnet.org/Guides/GettingStarted.htm
    /// </summary>
    [ClrJob, CoreJob]
    public class Md5VsSha256
    {
        private const int N = 10000;
        
        private readonly byte[] _data;

        private readonly SHA256 _sha256 = SHA256.Create();
        
        private readonly MD5 _md5 = MD5.Create();

        public Md5VsSha256()
        {
            _data = new byte[N];
            new Random(42).NextBytes(_data);
        }

        [Benchmark]
        public byte[] Sha256() => _sha256.ComputeHash(_data);

        [Benchmark]
        public byte[] Md5() => _md5.ComputeHash(_data);

        [Benchmark]
        public async Task<byte[]> Md5Async()
        {
            return await Task.Run(() => _md5.ComputeHash(_data))
                .ConfigureAwait(false);
        }
    }
}