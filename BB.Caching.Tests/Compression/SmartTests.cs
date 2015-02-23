using System.Text;
using BB.Caching.Compression;
using Xunit;

namespace BB.Caching.Tests.Compression
{
    public class SmartTests
    {
        private const string VALUE_BAD_COMPRESSION =
            "I am the string that we want to compress, but it's never smaller! :(";

        private const string VALUE_GOOD_COMPRESSION =
            "I am the string that we want to compress, but it's never smaller! :(" +
                "I am the string that we want to compress, but it's never smaller! :(";

        [Fact]
        public void SyncBadCompressionTest()
        {
            byte[] raw = Encoding.UTF8.GetBytes(VALUE_BAD_COMPRESSION);
            byte[] compress = SmartCompressor.Instance.CompressAsync(VALUE_BAD_COMPRESSION).Result;

            Assert.NotEqual(raw, compress);

            byte[] decompress = SmartCompressor.Instance.DecompressAsync(compress).Result;
            string actual = Encoding.UTF8.GetString(decompress);

            Assert.Equal(VALUE_BAD_COMPRESSION, actual);
        }

        [Fact]
        public void SyncGoodCompressionTest()
        {
            byte[] raw = Encoding.UTF8.GetBytes(VALUE_GOOD_COMPRESSION);
            byte[] compress = SmartCompressor.Instance.CompressAsync(VALUE_GOOD_COMPRESSION).Result;

            Assert.NotEqual(raw, compress);

            byte[] decompress = SmartCompressor.Instance.DecompressAsync(compress).Result;
            string actual = Encoding.UTF8.GetString(decompress);

            Assert.Equal(VALUE_GOOD_COMPRESSION, actual);
        }
    }
}