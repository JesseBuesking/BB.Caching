using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using BB.Caching.Compression;
using Xunit;

namespace BB.Caching.Tests.Compression
{
    public class GZipTests
    {
        private const string Value = "I am the string that we want to compress, but it's never smaller! :(";

        [Fact]
        public void SynchronousCompressionTest()
        {
            byte[] raw = Encoding.UTF8.GetBytes(Value);
            byte[] compress = GZipCompressor.Instance.Compress(Value);

            Assert.NotEqual(raw, compress);

            byte[] decompress = GZipCompressor.Instance.Decompress(compress);
            string actual = Encoding.UTF8.GetString(decompress);

            Assert.Equal(Value, actual);
        }

        [Fact]
        public void AsynchronousCompressionTest()
        {
            byte[] raw = Encoding.UTF8.GetBytes(Value);
            byte[] compress = GZipCompressor.Instance.CompressAsync(Value).Result;

            Assert.NotEqual(raw, compress);

            byte[] decompress = GZipCompressor.Instance.DecompressAsync(compress).Result;
            string actual = Encoding.UTF8.GetString(decompress);

            Assert.Equal(Value, actual);
        }

        [Fact(Skip="Skipping")]
        public void AsyncFaster()
        {
            const int iterations = 100000;

            byte[] compressSync = new byte[0];
            byte[] decompressSync = new byte[0];
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                compressSync = GZipCompressor.Instance.Compress(Value);
                decompressSync = GZipCompressor.Instance.Decompress(compressSync);
            }
            long sync = sw.ElapsedTicks;

            byte[] compressAsync = new byte[0];
            byte[] decompressAsync = new byte[0];
            sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                Task.Run(async () =>
                    {
                        compressAsync = await GZipCompressor.Instance.CompressAsync(Value);
                        decompressAsync = await GZipCompressor.Instance.DecompressAsync(compressAsync);
                    });
            }
            long async = sw.ElapsedTicks;

//            compressAsync = GZip.Instance.CompressAsync(Value).Result;
//            decompressAsync = GZip.Instance.DecompressAsync(compressAsync).Result;

            string debugInfo = "";
            debugInfo += "async vs sync: " + ((float) async/sync)*100 + "%\n";
            debugInfo += "\n";
            Console.WriteLine(debugInfo);

            Assert.True(sync > async);
//            Assert.Equal(compressSync, compressAsync);
//            Assert.Equal(decompressSync, decompressAsync);
//            Assert.True(0 < compressSync.Length);
//            Assert.True(0 < compressAsync.Length);
//            Assert.True(0 < decompressSync.Length);
//            Assert.True(0 < decompressAsync.Length);
        }
    }
}