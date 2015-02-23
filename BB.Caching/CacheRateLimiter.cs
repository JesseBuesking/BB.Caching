using System;
using System.Threading.Tasks;
using BB.Caching.Shared;
using BB.Caching.Utilities;
using StackExchange.Redis;

namespace BB.Caching
{
    public static partial class Cache
    {
        public static class RateLimiter
        {
// ReSharper disable MemberHidesStaticFromOuterClass
            public static void Prepare()
// ReSharper restore MemberHidesStaticFromOuterClass
            {
                var script = Lua.Instance["RateLimitIncrement"];
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
                RedisKey[] keyArgs = {key};
                RedisValue[] valueArgs = new RedisValue[]
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
                        .ScriptEvaluateAsync(
                            RateLimiter.RateLimitIncrementHash,
                            keys: keyArgs,
                            values: valueArgs,
                            flags: CommandFlags.None
                        );


                    if (null == result)
                        result = task;
                }
                return result;
            }
        }
    }
}