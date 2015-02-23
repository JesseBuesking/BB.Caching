using System.Threading.Tasks;

namespace BB.Caching.Compression
{
    public static partial class Compress
    {
        public static class Compression
        {
            public static Task<byte[]> CompressAsync(byte[] value)
            {
                return SmartCompressor.Instance.CompressAsync(value);
            }

            public static Task<byte[]> CompressAsync(string value)
            {
                return SmartCompressor.Instance.CompressAsync(value);
            }

            public static Task<string> DecompressStringAsync(byte[] value)
            {
                return SmartCompressor.Instance.DecompressStringAsync(value);
            }

            public static Task<byte[]> DecompressByteArrayAsync(byte[] value)
            {
                return SmartCompressor.Instance.DecompressAsync(value);
            }

            public static byte[] Compress(byte[] value)
            {
                return SmartCompressor.Instance.Compress(value);
            }

            public static byte[] Compress(string value)
            {
                return SmartCompressor.Instance.Compress(value);
            }

            public static string DecompressString(byte[] value)
            {
                return SmartCompressor.Instance.DecompressString(value);
            }

            public static byte[] DecompressByteArray(byte[] value)
            {
                return SmartCompressor.Instance.Decompress(value);
            }
        }
    }
}