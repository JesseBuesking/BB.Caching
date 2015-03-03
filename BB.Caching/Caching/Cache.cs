// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    using BB.Caching.Redis;

    /// <summary>
    /// Contains the core methods for caching data in memory, redis, or both.
    /// </summary>
    public static partial class Cache
    {
        /// <summary>
        /// Calls all sub-class preparation methods.
        /// </summary>
        public static void Prepare()
        {
            // free of side-effects
            Cache.SubscribeCacheDeleteChannel();
            RateLimiter.ScriptLoad();
            Statistics.ScriptLoad();
        }
    }
}