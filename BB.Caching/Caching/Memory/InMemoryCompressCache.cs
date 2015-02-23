using System;
using System.Text;
using System.Threading.Tasks;
using BB.Caching.Compression;

namespace BB.Caching.Caching.Memory
{
    public static partial class InMemoryCache
    {
        public static bool TryGetDecompress(string key, out byte[] value)
        {
            byte[] stored;
            if (!Cache.Memory.Strings.TryGet(key, out stored))
            {
                value = null;
                return false;
            }

            value = SmartCompressor.Instance.Decompress(stored);
            return true;
        }

        public static Wrapper<byte[], byte[]> TryGetByteArrayDecompressAsync(string key)
        {
            var result = new Wrapper<byte[], byte[]>();
            byte[] stored;
            if (!Cache.Memory.Strings.TryGet(key, out stored))
            {
                result.ValueAsync = Task.Run(() => (byte[]) null);
                result.IsNilAsync = Task.Run(() => true);
                return result;
            }

            result.ValueAsync = SmartCompressor.Instance.DecompressAsync(stored);
            result.IsNilAsync = Task.Run(() => false);
            return result;
        }

        public static bool TryGetDecompress(string key, out string value)
        {
            byte[] stored;
            if (!Cache.Memory.Strings.TryGet(key, out stored))
            {
                value = null;
                return false;
            }

            stored = SmartCompressor.Instance.Decompress(stored);
            value = Encoding.UTF8.GetString(stored);
            return true;
        }

        public static Wrapper<string, string> TryGetStringDecompressAsync(string key)
        {
            var result = new Wrapper<string, string>();
            byte[] stored;
            if (!Cache.Memory.Strings.TryGet(key, out stored))
            {
                result.ValueAsync = Task.Run(() => (string) null);
                result.IsNilAsync = Task.Run(() => true);
                return result;
            }

            result.ValueAsync = Task.Run(async () =>
                Encoding.UTF8.GetString(await SmartCompressor.Instance.DecompressAsync(stored)));
            result.IsNilAsync = Task.Run(() => false);
            return result;
        }

        public static byte[] SetCompress(string key, byte[] value)
        {
            byte[] compressed = SmartCompressor.Instance.Compress(value);
            Cache.Memory.Strings.Set(key, compressed);
            return compressed;
        }

        public static async Task<byte[]> SetCompressAsync(string key, byte[] value)
        {
            byte[] compressed = await SmartCompressor.Instance.CompressAsync(value);
            Cache.Memory.Strings.Set(key, compressed);
            return compressed;
        }

        public static byte[] SetCompress(string key, byte[] value, TimeSpan absoluteExpiration)
        {
            byte[] compressed = SmartCompressor.Instance.Compress(value);
            Cache.Memory.Strings.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public static async Task<byte[]> SetCompressAsync(string key, byte[] value, TimeSpan absoluteExpiration)
        {
            byte[] compressed = await SmartCompressor.Instance.CompressAsync(value);
            Cache.Memory.Strings.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public static byte[] SetCompressSliding(string key, byte[] value, TimeSpan slidingExpiration)
        {
            byte[] compressed = SmartCompressor.Instance.Compress(value);
            Cache.Memory.Strings.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }

        public static async Task<byte[]> SetCompressSlidingAsync(string key, byte[] value, TimeSpan slidingExpiration)
        {
            byte[] compressed = await SmartCompressor.Instance.CompressAsync(value);
            Cache.Memory.Strings.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }

        public static byte[] SetCompress(string key, string value)
        {
            byte[] compressed = SmartCompressor.Instance.Compress(value);
            Cache.Memory.Strings.Set(key, compressed);
            return compressed;
        }

        public static async Task<byte[]> SetCompressAsync(string key, string value)
        {
            byte[] compressed = await SmartCompressor.Instance.CompressAsync(value);
            Cache.Memory.Strings.Set(key, compressed);
            return compressed;
        }

        public static byte[] SetCompress(string key, string value, TimeSpan absoluteExpiration)
        {
            byte[] compressed = SmartCompressor.Instance.Compress(value);
            Cache.Memory.Strings.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public static async Task<byte[]> SetCompressAsync(string key, string value, TimeSpan absoluteExpiration)
        {
            byte[] compressed = await SmartCompressor.Instance.CompressAsync(value);
            Cache.Memory.Strings.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public static byte[] SetCompressSliding(string key, string value, TimeSpan slidingExpiration)
        {
            byte[] compressed = SmartCompressor.Instance.Compress(value);
            Cache.Memory.Strings.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }

        public static async Task<byte[]> SetCompressSlidingAsync(string key, string value, TimeSpan slidingExpiration)
        {
            byte[] compressed = await SmartCompressor.Instance.CompressAsync(value);
            Cache.Memory.Strings.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }
    }
}