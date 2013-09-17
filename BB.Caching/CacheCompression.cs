using System.Threading.Tasks;
using BB.Caching.Compression;

namespace BB.Caching
{
    public static partial class Cache
    {
        public static class Compression
        {
            public static Task<byte[]> CompressAsync(byte[] value)
            {
                return Compressor.Instance.CompressAsync(value);
            }

            public static Task<byte[]> CompressAsync(string value)
            {
                return Compressor.Instance.CompressAsync(value);
            }

            public static Task<string> DecompressStringAsync(byte[] value)
            {
                return Compressor.Instance.DecompressStringAsync(value);
            }

            public static Task<byte[]> DecompressByteArrayAsync(byte[] value)
            {
                return Compressor.Instance.DecompressAsync(value);
            }

            public static byte[] Compress(byte[] value)
            {
                return Compressor.Instance.Compress(value);
            }

            public static byte[] Compress(string value)
            {
                return Compressor.Instance.Compress(value);
            }

            public static string DecompressString(byte[] value)
            {
                return Compressor.Instance.DecompressString(value);
            }

            public static byte[] DecompressByteArray(byte[] value)
            {
                return Compressor.Instance.Decompress(value);
            }
        }
    }
}