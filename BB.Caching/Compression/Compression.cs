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

            public static TObject Decompress<TObject>(byte[] value)
            {
                byte[] decompressed = SmartCompressor.Instance.Decompress(value);
                TObject deserialized = ProtoBufSerializer.Deserialize<TObject>(decompressed);
                return deserialized;
            }
        }
    }
}