namespace BB.Caching.Serialization
{
    /// <summary>
    /// The serializer that should be used across all caching mechanisms.
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// The instance of the serializer to use.
        /// </summary>
        public static readonly ISerializer Instance = ProtoBufSerializer.Instance;
    }
}