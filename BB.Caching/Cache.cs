using BB.Caching.InMemory;

namespace BB.Caching
{
    /// <summary>
    /// Contains access to all available caching mechanisms. (In-memory, Shared)
    /// </summary>
    public static partial class Cache
    {
        /// <summary>
        /// An in-memory cache specific to the current machine.
        /// </summary>
        public static InMemoryCache Memory
        {
            get { return InMemoryCache.Instance; }
        }

        /// <summary>
        /// Calls all sub-class preparation methods.
        /// </summary>
        public static void Prepare()
        {
            Cache.Config.Prepare();
            Cache.RateLimiter.Prepare();
            Cache.Stats.Prepare();
        }
    }
}