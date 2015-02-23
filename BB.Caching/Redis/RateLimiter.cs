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

        public static Task<RedisResult> Increment(string key, TimeSpan spanSize, TimeSpan bucketSize, long throttle,
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
            Task<RedisResult> result = null;
            foreach (var connection in connections)
            {
                var task = connection.GetDatabase(SharedCache.Instance.Db)
                    .ScriptEvaluateAsync(RateLimiter.RateLimitIncrementHash, keyArgs, valueArgs);


                if (null == result)
                    result = task;
            }
            return result;
        }
    }
}