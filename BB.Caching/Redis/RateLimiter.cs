using System;
using System.Threading.Tasks;
using BB.Caching.Redis.Lua;
using StackExchange.Redis;

namespace BB.Caching.Redis
{
    public static class RateLimiter
    {
        /// <summary>
        /// Loads the underlying Lua script(s) onto all necessary servers.
        /// </summary>
        public static void ScriptLoad()
        {
            var script = ScriptLoader.Instance["RateLimitIncrement"];
            var connections = SharedCache.Instance.GetAllWriteConnections();

            foreach (var connection in connections)
            {
                foreach (var endpoint in connection.GetEndPoints())
                {
                    RateLimiter._rateLimitIncrementHash = connection.GetServer(endpoint).ScriptLoad(script);
                }
            }
        }

        private static byte[] RateLimitIncrementHash
        {
            get { return RateLimiter._rateLimitIncrementHash; }
        }

        private static byte[] _rateLimitIncrementHash;

        /// <summary>
        /// Increments the rate limiter at the key specified by <param name="increment"/> unless the increment goes
        /// over the <param name="throttle"/>, in which case it returns the amount it's gone over as a negative value.
        /// </summary>
        /// <param name="key">The key where the rate limiter exists.</param>
        /// <param name="spanSize">The full span of time being rate limited.</param>
        /// <param name="bucketSize">The period of time that each bucket should track.</param>
        /// <param name="throttle">The total throttling size.</param>
        /// <param name="increment">How much to increment by.</param>
        /// <returns></returns>
        public static RedisResult Increment(string key, TimeSpan spanSize, TimeSpan bucketSize, long throttle,
            int increment = 1)
        {
            RedisKey[] keyArgs = { key };
            RedisValue[] valueArgs =
            {
                DateTime.UtcNow.Ticks/TimeSpan.TicksPerMillisecond,
                (long) spanSize.TotalMilliseconds,
                (long) bucketSize.TotalMilliseconds,
                increment,
                throttle
            };

            var connections = SharedCache.Instance.GetWriteConnections(key);
            RedisResult result = null;
            foreach (var connection in connections)
            {
                result = connection.GetDatabase(SharedCache.Instance.Db)
                    .ScriptEvaluate(RateLimiter.RateLimitIncrementHash, keyArgs, valueArgs);
            }
            return result;
        }

        /// <summary>
        /// Increments the rate limiter at the key specified by <param name="increment"/> unless the increment goes
        /// over the <param name="throttle"/>, in which case it returns the amount it's gone over as a negative value.
        /// </summary>
        /// <param name="key">The key where the rate limiter exists.</param>
        /// <param name="spanSize">The full span of time being rate limited.</param>
        /// <param name="bucketSize">The period of time that each bucket should track.</param>
        /// <param name="throttle">The total throttling size.</param>
        /// <param name="increment">How much to increment by.</param>
        /// <returns></returns>
        public static async Task<RedisResult> IncrementAsync(
            string key, TimeSpan spanSize, TimeSpan bucketSize, long throttle,
            int increment = 1)
        {
            RedisKey[] keyArgs = { key };
            RedisValue[] valueArgs =
            {
                DateTime.UtcNow.Ticks/TimeSpan.TicksPerMillisecond,
                (long) spanSize.TotalMilliseconds,
                (long) bucketSize.TotalMilliseconds,
                increment,
                throttle
            };

            var connections = SharedCache.Instance.GetWriteConnections(key);
            RedisResult result = null;
            foreach (var connection in connections)
            {
                result = await connection.GetDatabase(SharedCache.Instance.Db)
                    .ScriptEvaluateAsync(RateLimiter.RateLimitIncrementHash, keyArgs, valueArgs);
            }
            return result;
        }
    }
}