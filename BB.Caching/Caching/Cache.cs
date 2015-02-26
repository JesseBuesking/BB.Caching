using BB.Caching.Redis;

// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    /// <summary>
    /// Contains access to all available caching mechanisms. (In-memory, Shared)
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

            // free of side-effects
            RateLimiter.ScriptLoad();
            Statistics.ScriptLoad();
        }
    }
}