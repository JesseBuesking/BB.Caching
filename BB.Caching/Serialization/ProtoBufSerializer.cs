namespace BB.Caching.Serialization
{
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using ProtoBuf;

    /// <summary>
    /// Helper methods when using the protobuf serializer.
    /// </summary>
    public static class ProtoBufSerializer
    {
        /// <summary>
        /// Serializes the object into a byte array.
        /// <para>
        /// Uses protobuf to serialize the object supplied.
        /// </para>
        /// </summary>
        /// <typeparam name="TType">
        /// The type of the value being serialized.
        /// </typeparam>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// A byte array containing the serialized results.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Serialize<TType>(TType value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(ms, value);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serializes the object into a byte array.
        /// <para>
        /// Uses protobuf to serialize the object supplied.
        /// </para>
        /// </summary>
        /// <typeparam name="TType">
        /// The type of the value being serialized.
        /// </typeparam>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// A byte array containing the serialized results.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<byte[]> SerializeAsync<TType>(TType value)
        {
            return await Task.Run(() =>
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, value);
                    return ms.ToArray();
                }
            });
        }

        /// <summary>
        /// Deserializes the object from the byte array.
        /// <para>
        /// Uses protobuf to deserialize the byte array supplied.
        /// </para>
        /// </summary>
        /// <typeparam name="TType">
        /// The type of the object to deserialize into.
        /// </typeparam>
        /// <param name="value">
        /// A byte array of serialized results to be deserialized.
        /// </param>
        /// <returns>
        /// An instance of type <paramref>
        ///         <name>TType</name>
        ///     </paramref>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TType Deserialize<TType>(byte[] value)
        {
            using (MemoryStream ms = new MemoryStream(value))
            {
                TType res = Serializer.Deserialize<TType>(ms);
                return res;
            }
        }

        /// <summary>
        /// Deserializes the object from the byte array.
        /// <para>
        /// Uses protobuf to deserialize the byte array supplied.
        /// </para>
        /// </summary>
        /// <typeparam name="TType">
        /// The type of the object to deserialize into.
        /// </typeparam>
        /// <param name="value">
        /// A byte array of serialized results to be deserialized.
        /// </param>
        /// <returns>
        /// An instance of type <paramref>
        ///         <name>TType</name>
        ///     </paramref>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TType> DeserializeAsync<TType>(byte[] value)
        {
            return await Task.Run(() =>
            {
                using (MemoryStream ms = new MemoryStream(value))
                {
                    TType res = Serializer.Deserialize<TType>(ms);
                    return res;
                }
            });
        }
    }
}