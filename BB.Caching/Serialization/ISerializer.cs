namespace BB.Caching.Serialization
{
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the object into a byte array.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        byte[] Serialize<TType>(TType value);

        /// <summary>
        /// Deserializes the object from the byte array.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        TType Deserialize<TType>(byte[] value);
    }
}