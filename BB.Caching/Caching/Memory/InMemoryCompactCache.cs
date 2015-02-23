using System;
using System.Threading.Tasks;
using BB.Caching.Compression;
using BB.Caching.Serialization;

namespace BB.Caching.Caching.Memory
{
    public static partial class InMemoryCache
    {
        public static bool TryGetDecompact<TObject>(string key, out TObject value)
        {
            byte[] stored;
            if (!Cache.Memory.Strings.TryGet(key, out stored))
            {
                value = default (TObject);
                return false;
            }

            stored = SmartCompressor.Instance.Decompress(stored);
            value = ProtoBufSerializer.Instance.Deserialize<TObject>(stored);
            return true;
        }

        public static Wrapper<TObject, TObject> TryGetDecompactAsync<TObject>(string key)
        {
            var result = new Wrapper<TObject, TObject>();
            byte[] stored;
            if (!Cache.Memory.Strings.TryGet(key, out stored))
            {
                result.ValueAsync = Task.Run(() => default (TObject));
                result.IsNilAsync = Task.Run(() => true);
                return result;
            }

            result.ValueAsync = Task.Run(async () =>
                ProtoBufSerializer.Instance.Deserialize<TObject>(await SmartCompressor.Instance.DecompressAsync(stored)));
            result.IsNilAsync = Task.Run(() => false);
            return result;
        }

        public static byte[] SetCompact<TObject>(string key, TObject value)
        {
            byte[] serialized = ProtoBufSerializer.Instance.Serialize(value);
            byte[] compressed = SmartCompressor.Instance.Compress(serialized);
            Cache.Memory.Strings.Set(key, compressed);
            return compressed;
        }

        public static async Task<byte[]> SetCompactAsync<TObject>(string key, TObject value)
        {
            byte[] serialized = ProtoBufSerializer.Instance.Serialize(value);
            byte[] compressed = await SmartCompressor.Instance.CompressAsync(serialized);
            Cache.Memory.Strings.Set(key, compressed);
            return compressed;
        }

        public static byte[] SetCompact<TObject>(string key, TObject value, TimeSpan absoluteExpiration)
        {
            byte[] serialized = ProtoBufSerializer.Instance.Serialize(value);
            byte[] compressed = SmartCompressor.Instance.Compress(serialized);
            Cache.Memory.Strings.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public static async Task<byte[]> SetCompactAsync<TObject>(string key, TObject value, TimeSpan absoluteExpiration)
        {
            byte[] serialized = ProtoBufSerializer.Instance.Serialize(value);
            byte[] compressed = await SmartCompressor.Instance.CompressAsync(serialized);
            Cache.Memory.Strings.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public static byte[] SetCompactSliding<TObject>(string key, TObject value, TimeSpan slidingExpiration)
        {
            byte[] serialized = ProtoBufSerializer.Instance.Serialize(value);
            byte[] compressed = SmartCompressor.Instance.Compress(serialized);
            Cache.Memory.Strings.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }

        public static async Task<byte[]> SetCompactSlidingAsync<TObject>(string key, TObject value, TimeSpan slidingExpiration)
        {
            byte[] serialized = ProtoBufSerializer.Instance.Serialize(value);
            byte[] compressed = await SmartCompressor.Instance.CompressAsync(serialized);
            Cache.Memory.Strings.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }
    }
}