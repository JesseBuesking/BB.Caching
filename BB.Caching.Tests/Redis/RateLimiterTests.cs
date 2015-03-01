using System;
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
            try
            {
                Cache.Prepare();
            }
            catch (PubSub.ChannelAlreadySubscribedException)
            { }
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
            Cache.Shared.Keys.DeleteAsync(KEY).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.DeleteAsync(KEY).Wait();
        }

        [Fact]
        public void CountCheckTest()
        {
            const int amount = 1000;
            
            var tasks = new Task<RedisResult>[amount];
            for (int i = 0; i < amount; i++)
            {
                // ReSharper disable RedundantArgumentDefaultValue
                tasks[i] = RateLimiter.IncrementAsync(
                    KEY, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), amount, 1);
                // ReSharper restore RedundantArgumentDefaultValue
            }

            Task.WhenAll(tasks);

            long count = Cache.Shared.Hashes.GetAllAsync(KEY).Result
                .Where(kvp => "L" != kvp.Name)
                .Select(kvp => long.Parse(Encoding.UTF8.GetString(kvp.Value)))
                .Sum();

            Assert.True(amount - (amount * .2) < count);
        }

        [Fact]
        public void Increment()
        {
            TimeSpan span = TimeSpan.FromSeconds(.4);
            TimeSpan bucketSize = TimeSpan.FromSeconds(.1);

            RedisResult result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(2, (long)result);

            Thread.Sleep(100);
            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(4, (long)result);

            Thread.Sleep(100);
            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(6, (long)result);

            Thread.Sleep(100);
            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(8, (long)result);

            Thread.Sleep(100);
// ReSharper disable RedundantArgumentDefaultValue
            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 1).Result;
            Assert.Equal(7, (long)result);

            Thread.Sleep(100);
            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 1).Result;
            Assert.Equal(6, (long)result);

            Thread.Sleep(100);
            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 1).Result;
            Assert.Equal(5, (long)result);

            Thread.Sleep(200);
            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 1).Result;
// ReSharper restore RedundantArgumentDefaultValue
            Assert.Equal(3, (long)result);
        }

        [Fact]
        public void Edge()
        {
            TimeSpan span = TimeSpan.FromSeconds(.2);
            TimeSpan bucketSize = TimeSpan.FromSeconds(.05);

            RedisResult result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(2, (long)result);
            Thread.Sleep(100);

            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(4, (long)result);
            Thread.Sleep(100);

// ReSharper disable RedundantArgumentDefaultValue
            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 1).Result;
            Assert.Equal(3, (long)result);
            Thread.Sleep(100);

            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 1).Result;
// ReSharper restore RedundantArgumentDefaultValue
            Assert.Equal(2, (long)result);
            Thread.Sleep(100);

            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 10, 2).Result;
            Assert.Equal(3, (long)result);
            Thread.Sleep(100);
        }

        [Fact]
        public void Overage()
        {
            TimeSpan span = TimeSpan.FromSeconds(.2);
            TimeSpan bucketSize = TimeSpan.FromSeconds(.05);

            RedisResult result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 4, 2).Result;
            Assert.Equal(2, (long)result);
            Thread.Sleep(100);

            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 4, 2).Result;
            Assert.Equal(4, (long)result);
            Thread.Sleep(100);

            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 4, 3).Result;
            Assert.Equal(-1, (long)result);
            Thread.Sleep(100);

            result = RateLimiter.IncrementAsync(KEY, span, bucketSize, 4, 3).Result;
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