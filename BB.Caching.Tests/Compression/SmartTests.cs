using System.Text;
using BB.Caching.Compression;
using Xunit;

namespace BB.Caching.Tests.Compression
{
    public class SmartTests
    {
        private const string _valueBadCompression =
            "I am the string that we want to compress, but it's never smaller! :(";

        private const string _valueGoodCompression =
            "I am the string that we want to compress, but it's never smaller! :(" +
                "I am the string that we want to compress, but it's never smaller! :(";

        [Fact]
        public void SyncBadCompressionTest()
        {
            byte[] raw = Encoding.UTF8.GetBytes(_valueBadCompression);
            byte[] compress = SmartCompressor.Instance.CompressAsync(_valueBadCompression).Result;

            Assert.NotEqual(raw, compress);

            byte[] decompress = SmartCompressor.Instance.DecompressAsync(compress).Result;
            string actual = Encoding.UTF8.GetString(decompress);

            Assert.Equal(_valueBadCompression, actual);
        }

        [Fact]
        public void SyncGoodCompressionTest()
        {
            byte[] raw = Encoding.UTF8.GetBytes(_valueGoodCompression);
            byte[] compress = SmartCompressor.Instance.CompressAsync(_valueGoodCompression).Result;

            Assert.NotEqual(raw, compress);

            byte[] decompress = SmartCompressor.Instance.DecompressAsync(compress).Result;
            string actual = Encoding.UTF8.GetString(decompress);

            Assert.Equal(_valueGoodCompression, actual);
        }
    }
}