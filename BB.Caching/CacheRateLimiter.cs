using System;
using System.Threading.Tasks;
using BB.Caching.Shared;
using BB.Caching.Utilities;

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
                var connections = SharedCache.Instance.GetAllWriteConnections();
                foreach (var connection in connections)
                    connection.Scripting.Prepare(new[] {RateLimiter.RateLimitIncrementScript});
            }

            private static string RateLimitIncrementScript
            {
                get { return Lua.Instance["RateLimitIncrement"]; }
            }

            public static Task<object> Increment(string key, TimeSpan spanSize, TimeSpan bucketSize, long throttle,
                int increment = 1)
            {
                string[] keyArgs = new[] {key};
                object[] valueArgs = new object[]
                    {
                        DateTime.UtcNow.Ticks/TimeSpan.TicksPerMillisecond,
                        (long) spanSize.TotalMilliseconds,
                        (long) bucketSize.TotalMilliseconds,
                        increment,
                        throttle
                    };

                var connections = SharedCache.Instance.GetWriteConnections(key);
                Task<object> result = null;
                foreach (var connection in connections)
                {
                    var task = connection.Scripting.Eval(SharedCache.Instance.Db, RateLimiter.RateLimitIncrementScript,
                        keyArgs, valueArgs, true, false, SharedCache.Instance.QueueJump);
                    if (null == result)
                        result = task;
                }
                return result;
            }
        }
    }
}