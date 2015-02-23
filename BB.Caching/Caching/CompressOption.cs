namespace BB.Caching.Caching
{
    public enum CompressOption
    {
        /// <summary>
        /// No compression logic will be used.
        /// </summary>
        None,

        /// <summary>
        /// Uses smart compression -- gzip if smaller or raw -- to store data in the cache.
        /// </summary>
        Compress,

        /// <summary>
        /// Uses compaction -- compression + protobuf serialization -- to store data in the cache.
        /// </summary>
        Compact
    }
}
