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
            // may have side-effects if called twice
            Cache.Config.SetupSubscriptions();
            Cache.SubscribeCacheDeleteChannel();

            // free of side-effects
            RateLimiter.ScriptLoad();
            Statistics.ScriptLoad();
        }
    }
}