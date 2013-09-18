using System.Threading.Tasks;
using BB.Caching.Compression;
using BB.Caching.Serialization;

namespace BB.Caching
{
    public static partial class Cache
    {
// ReSharper disable MemberCanBePrivate.Global
        public static class Compaction
// ReSharper restore MemberCanBePrivate.Global
        {
            public static async Task<byte[]> CompactAsync<TObject>(TObject value)
            {
                byte[] serialize = Serializer.Instance.Serialize(value);
                byte[] compressed = await Compressor.Instance.CompressAsync(serialize);
                return compressed;
            }

            public static async Task<TObject> DecompactAsync<TObject>(byte[] value)
            {
                byte[] decompressed = await Compressor.Instance.DecompressAsync(value);
                TObject deserialized = Serializer.Instance.Deserialize<TObject>(decompressed);
                return deserialized;
            }

            public static byte[] Compact<TObject>(TObject value)
            {
                byte[] serialize = Serializer.Instance.Serialize(value);
                byte[] compressed = Compressor.Instance.Compress(serialize);
                return compressed;
            }

            public static TObject Decompact<TObject>(byte[] value)
            {
                byte[] decompressed = Compressor.Instance.Decompress(value);
                TObject deserialized = Serializer.Instance.Deserialize<TObject>(decompressed);
                return deserialized;
            }
        }
    }
}