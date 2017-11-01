namespace Memstate.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Jobs;

    /// <summary>
    /// This is the sample from the getting started guide of the benchmarkdotnet documentation
    /// at http://benchmarkdotnet.org/Guides/GettingStarted.htm
    /// </summary>
    [ClrJob, CoreJob]
    public class Md5VsSha256
    {
        private const int N = 10000;
        private readonly byte[] data;

        private readonly SHA256 sha256 = SHA256.Create();
        private readonly MD5 md5 = MD5.Create();

        public Md5VsSha256()
        {
            data = new byte[N];
            new Random(42).NextBytes(data);
        }

        [Benchmark]
        public byte[] Sha256() => sha256.ComputeHash(data);

        [Benchmark]
        public byte[] Md5() => md5.ComputeHash(data);

        [Benchmark]
        public async Task<byte[]> Md5Async()
        {
            return await Task.Run(() => md5.ComputeHash(data))
                .ConfigureAwait(false);
        }
    }
}