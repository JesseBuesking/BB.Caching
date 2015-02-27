using System.Threading.Tasks;
using BB.Caching.Serialization;

namespace BB.Caching.Compression
{
    public static class Compress
    {
        public static class Compression
        {
            public static byte[] Compress<TObject>(TObject value)
            {
                byte[] serialize = ProtoBufSerializer.Serialize(value);
                byte[] compressed = SmartCompressor.Instance.Compress(serialize);
                return compressed;
            }

            public static async Task<byte[]> CompressAsync<TObject>(TObject value)
            {
                // TODO fix
                //byte[] serialized = await ProtoBufSerializer.SerializeAsync(value);
                byte[] serialized = ProtoBufSerializer.Serialize(value);
                byte[] compressed = await SmartCompressor.Instance.CompressAsync(serialized);
                return compressed;
            }

            public static TObject Decompress<TObject>(byte[] value)
            {
                byte[] decompressed = SmartCompressor.Instance.Decompress(value);
                TObject deserialized = ProtoBufSerializer.Deserialize<TObject>(decompressed);
                return deserialized;
            }

            public static async Task<TObject> DecompressAsync<TObject>(byte[] value)
            {
                byte[] decompressed = await SmartCompressor.Instance.DecompressAsync(value);
                // TODO fix
                //TObject deserialized = await ProtoBufSerializer.DeserializeAsync<TObject>(decompressed);
                TObject deserialized = ProtoBufSerializer.Deserialize<TObject>(decompressed);
                return deserialized;
            }
        }
    }
}