namespace BB.Caching.Compression
{
    /// <summary>
    /// The compressor that should be used across all caching mechanisms.
    /// </summary>
    public static class Compressor
    {
        /// <summary>
        /// The instance of the compressor to use.
        /// </summary>
        public static readonly ICompress Instance = SmartCompressor.Instance;
    }
}