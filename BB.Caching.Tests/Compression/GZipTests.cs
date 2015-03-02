namespace BB.Caching.Tests.Compression
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    using BB.Caching.Compression;

    using Xunit;

    public class GZipTests
    {
        private const string VALUE = "I am the string that we want to compress, but it's never smaller! :(";

        [Fact]
        public void SynchronousCompressionTest()
        {
            byte[] raw = Encoding.UTF8.GetBytes(VALUE);
            byte[] compress = GZipCompressor.Instance.Compress(VALUE);

            Assert.NotEqual(raw, compress);

            byte[] decompress = GZipCompressor.Instance.Decompress(compress);
            string actual = Encoding.UTF8.GetString(decompress);

            Assert.Equal(VALUE, actual);
        }

        [Fact]
        public void AsynchronousCompressionTest()
        {
            byte[] raw = Encoding.UTF8.GetBytes(VALUE);
            byte[] compress = GZipCompressor.Instance.CompressAsync(VALUE).Result;

            Assert.NotEqual(raw, compress);

            byte[] decompress = GZipCompressor.Instance.DecompressAsync(compress).Result;
            string actual = Encoding.UTF8.GetString(decompress);

            Assert.Equal(VALUE, actual);
        }

        [Fact(Skip = "Skipping")]
        public void AsyncFaster()
        {
            const int ITERATIONS = 100000;

            // ReSharper disable once TooWideLocalVariableScope
            byte[] compressSync;
#pragma warning disable 219
            byte[] decompressSync;
#pragma warning restore 219
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < ITERATIONS; i++)
            {
                compressSync = GZipCompressor.Instance.Compress(VALUE);
                // ReSharper disable once RedundantAssignment
                decompressSync = GZipCompressor.Instance.Decompress(compressSync);
            }

            long sync = sw.ElapsedTicks;

            // ReSharper disable once TooWideLocalVariableScope
            byte[] compressAsync;
#pragma warning disable 219
            byte[] decompressAsync;
#pragma warning restore 219
            sw = Stopwatch.StartNew();
            for (int i = 0; i < ITERATIONS; i++)
            {
                Task.Run(async () =>
                    {
                        compressAsync = await GZipCompressor.Instance.CompressAsync(VALUE);
                        decompressAsync = await GZipCompressor.Instance.DecompressAsync(compressAsync);
                    });
            }

            long async = sw.ElapsedTicks;

            string debugInfo = string.Empty;
            debugInfo += "async vs sync: " + (((float)async / sync) * 100) + "%\n";
            debugInfo += "\n";
            Console.WriteLine(debugInfo);

            Assert.True(sync > async);
        }
    }
}