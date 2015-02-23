using System.Threading.Tasks;
using BB.Caching.Serialization;

namespace BB.Caching.Compression
{
    public static partial class Compress
    {
// ReSharper disable MemberCanBePrivate.Global
        public static class Compaction
// ReSharper restore MemberCanBePrivate.Global
        {
            public static async Task<byte[]> CompactAsync<TObject>(TObject value)
            {
                byte[] serialize = ProtoBufSerializer.Instance.Serialize(value);
                byte[] compressed = await SmartCompressor.Instance.CompressAsync(serialize);
                return compressed;
            }

            public static async Task<TObject> DecompactAsync<TObject>(byte[] value)
            {
                byte[] decompressed = await SmartCompressor.Instance.DecompressAsync(value);
                TObject deserialized = ProtoBufSerializer.Instance.Deserialize<TObject>(decompressed);
                return deserialized;
            }

            public static byte[] Compact<TObject>(TObject value)
            {
                byte[] serialize = ProtoBufSerializer.Instance.Serialize(value);
                byte[] compressed = SmartCompressor.Instance.Compress(serialize);
                return compressed;
            }

            public static TObject Decompact<TObject>(byte[] value)
            {
                byte[] decompressed = SmartCompressor.Instance.Decompress(value);
                TObject deserialized = ProtoBufSerializer.Instance.Deserialize<TObject>(decompressed);
                return deserialized;
            }
        }
    }
}