using System.IO;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace BB.Caching.Serialization
{
    public static class ProtoBufSerializer
    {
        /// <summary>
        /// Serializes the object into a byte array.
        /// <para>
        /// Uses protobuf to serialize the object supplied.
        /// </para>
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
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
        /// Deserializes the object from the byte array.
        /// <para>
        /// Uses protobuf to deserialize the byte array supplied.
        /// </para>
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TType Deserialize<TType>(byte[] value)
        {
            using (MemoryStream ms = new MemoryStream(value))
            {
                TType res = Serializer.Deserialize<TType>(ms);
                return res;
            }
        }
    }
}