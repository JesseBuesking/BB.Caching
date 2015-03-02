namespace BB.Caching.Compression
{
    using System.Threading.Tasks;

    using BB.Caching.Serialization;

    /// <summary>
    /// Contains classes for performing compression.
    /// </summary>
    public static class Compress
    {
        /// <summary>
        /// A static class containing methods for compressing data.
        /// </summary>
        public static class Compression
        {
            /// <summary>
            /// Compresses the object into a byte array.
            /// </summary>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <typeparam name="TObject">
            /// The type of the object being compressed.
            /// </typeparam>
            /// <returns>
            /// The <see><cref>byte[]</cref></see> containing compressed data.
            /// </returns>
            public static byte[] Compress<TObject>(TObject value)
            {
                byte[] serialize = ProtoBufSerializer.Serialize(value);
                byte[] compressed = SmartCompressor.Instance.Compress(serialize);
                return compressed;
            }

            /// <summary>
            /// Compresses the object into a byte array.
            /// </summary>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <typeparam name="TObject">
            /// The type of the object being compressed.
            /// </typeparam>
            /// <returns>
            /// The <see><cref>byte[]</cref></see> containing compressed data.
            /// </returns>
            public static async Task<byte[]> CompressAsync<TObject>(TObject value)
            {
                // TODO fix
                // byte[] serialized = await ProtoBufSerializer.SerializeAsync(value);
                byte[] serialized = ProtoBufSerializer.Serialize(value);
                byte[] compressed = await SmartCompressor.Instance.CompressAsync(serialized);
                return compressed;
            }

            /// <summary>
            /// Decompresses a byte array into an instance of <typeparam name="TObject"></typeparam>.
            /// </summary>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <typeparam name="TObject">
            /// The type of the object being compressed.
            /// </typeparam>
            /// <returns>
            /// An instance of a <see cref="TObject"/>.
            /// </returns>
            public static TObject Decompress<TObject>(byte[] value)
            {
                byte[] decompressed = SmartCompressor.Instance.Decompress(value);
                TObject deserialized = ProtoBufSerializer.Deserialize<TObject>(decompressed);
                return deserialized;
            }

            /// <summary>
            /// Decompresses a byte array into an instance of <typeparam name="TObject"></typeparam>.
            /// </summary>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <typeparam name="TObject">
            /// The type of the object being compressed.
            /// </typeparam>
            /// <returns>
            /// An instance of a <see cref="TObject"/>.
            /// </returns>
            public static async Task<TObject> DecompressAsync<TObject>(byte[] value)
            {
                byte[] decompressed = await SmartCompressor.Instance.DecompressAsync(value);

                // TODO fix
                // TObject deserialized = await ProtoBufSerializer.DeserializeAsync<TObject>(decompressed);
                TObject deserialized = ProtoBufSerializer.Deserialize<TObject>(decompressed);
                return deserialized;
            }
        }
    }
}