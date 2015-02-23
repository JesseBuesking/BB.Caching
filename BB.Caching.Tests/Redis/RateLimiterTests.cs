using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BB.Caching.Redis;
using StackExchange.Redis;
using Xunit;

namespace BB.Caching.Tests.Redis
{
    public class RateLimiterTestsFixture : IDisposable
    {
        public RateLimiterTestsFixture()
        {
            Cache.Prepare();
        }

        public void Dispose()
        {
        }
    }

    public class RateLimiterTests : IUseFixture<DefaultTestFixture>, IUseFixture<RateLimiterTestsFixture>, IDisposable
    {
        private const string KEY = "key1";

        public RateLimiterTests()
        {
            Cache.Shared.Keys.Remove(KEY).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(KEY).Wait();
        }

        [Fact]
        public void Performance()
        {
            const int asyncAmount = 50000;
            Get(1, KEY, true);
            var asyncMs = Get(asyncAmount, KEY, false);

            Console.WriteLine("Rate Limiter Ops:");
            Console.WriteLine("\t{0:#,##0.0#} aops/ms", (float)asyncAmount / asyncMs);
            Console.WriteLine("\t{0:#,##0.0#} aops/s", (float)asyncAmount * 1000 / asyncMs);
        }

        private static long Get(int amount, string key, bool assert)
        {
            var tasks = new Task<RedisResult>[amount];
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < amount; i++)
            {
                // ReSharper disable RedundantArgumentDefaultValue
                tasks[i] = RateLimiter.Increment(
                    key, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), amount, 1);
                // ReSharper restore RedundantArgumentDefaultValue
            }

            Task.WhenAll(tasks);

            long total = sw.ElapsedMilliseconds;

            long count = Cache.Shared.Hashes.GetAll(key).Result
                .Where(kvp => "L" != kvp.Name)
                .Select(kvp => long.Parse(Encoding.UTF8.GetString(kvp.Value)))
                .Sum();

            if (assert)
            {
                Assert.True(amount - (amount * .2) < count);
            }
            return total;
        }

        [Fact]
        public void Increment()
        {
            TimeSpan span = TimeSpan.FromSeconds(.4);
            TimeSpan bucketSize = TimeSpan.FromSeconds(.1);

            RedisResult result = RateLimiter.Increment(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(2, (long)result);

            Thread.Sleep(100);
            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(4, (long)result);

            Thread.Sleep(100);
            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(6, (long)result);

            Thread.Sleep(100);
            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(8, (long)result);

            Thread.Sleep(100);
// ReSharper disable RedundantArgumentDefaultValue
            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 1).Result;
            Assert.Equal(7, (long)result);

            Thread.Sleep(100);
            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 1).Result;
            Assert.Equal(6, (long)result);

            Thread.Sleep(100);
            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 1).Result;
            Assert.Equal(5, (long)result);

            Thread.Sleep(200);
            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 1).Result;
// ReSharper restore RedundantArgumentDefaultValue
            Assert.Equal(3, (long)result);
        }

        [Fact]
        public void Edge()
        {
            TimeSpan span = TimeSpan.FromSeconds(.2);
            TimeSpan bucketSize = TimeSpan.FromSeconds(.05);

            RedisResult result = RateLimiter.Increment(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(2, (long)result);
            Thread.Sleep(100);

            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(4, (long)result);
            Thread.Sleep(100);

// ReSharper disable RedundantArgumentDefaultValue
            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 1).Result;
            Assert.Equal(3, (long)result);
            Thread.Sleep(100);

            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 1).Result;
// ReSharper restore RedundantArgumentDefaultValue
            Assert.Equal(2, (long)result);
            Thread.Sleep(100);

            result = RateLimiter.Increment(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(3, (long)result);
            Thread.Sleep(100);
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }

        public void SetFixture(RateLimiterTestsFixture data)
        {
        }
    }
}