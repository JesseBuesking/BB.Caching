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
        private const string _value = "I am the string that we want to compress, but it's never smaller! :(";

        [Fact]
        public void SynchronousCompressionTest()
        {
            byte[] raw = Encoding.UTF8.GetBytes(_value);
            byte[] compress = GZipCompressor.Instance.Compress(_value);

            Assert.NotEqual(raw, compress);

            byte[] decompress = GZipCompressor.Instance.Decompress(compress);
            string actual = Encoding.UTF8.GetString(decompress);

            Assert.Equal(_value, actual);
        }

        [Fact]
        public void AsynchronousCompressionTest()
        {
            byte[] raw = Encoding.UTF8.GetBytes(_value);
            byte[] compress = GZipCompressor.Instance.CompressAsync(_value).Result;

            Assert.NotEqual(raw, compress);

            byte[] decompress = GZipCompressor.Instance.DecompressAsync(compress).Result;
            string actual = Encoding.UTF8.GetString(decompress);

            Assert.Equal(_value, actual);
        }

        [Fact(Skip = "Skipping")]
        public void AsyncFaster()
        {
            const int iterations = 100000;

// ReSharper disable TooWideLocalVariableScope
            byte[] compressSync;
#pragma warning disable 219
            byte[] decompressSync;
#pragma warning restore 219
// ReSharper restore TooWideLocalVariableScope
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                compressSync = GZipCompressor.Instance.Compress(_value);
// ReSharper disable RedundantAssignment
                decompressSync = GZipCompressor.Instance.Decompress(compressSync);
// ReSharper restore RedundantAssignment
            }
            long sync = sw.ElapsedTicks;

// ReSharper disable TooWideLocalVariableScope
            byte[] compressAsync;
#pragma warning disable 219
            byte[] decompressAsync;
#pragma warning restore 219
// ReSharper restore TooWideLocalVariableScope
            sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                Task.Run(async () =>
                    {
                        compressAsync = await GZipCompressor.Instance.CompressAsync(_value);
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