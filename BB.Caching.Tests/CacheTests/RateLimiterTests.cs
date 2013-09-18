﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class RateLimiterTests : IDisposable
    {
        private const string _key = "key1";

        public RateLimiterTests()
        {
            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27")));

            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

            Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27"));

            Cache.Prepare();

            Cache.Shared.Keys.Remove(_key).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(_key).Wait();
        }

        [Fact]
        public void Performance()
        {
            const int asyncAmount = 30000;
            var asyncMs = Get(asyncAmount, _key);

            Console.WriteLine("{0:#,##0.0#} async ops per ms", (float) asyncAmount/asyncMs);
            Console.WriteLine();
            Console.WriteLine("async ({0:#,##0}): {1:#,##0}ms", asyncAmount, asyncMs);
        }

        private static long Get(int amount, string key)
        {
            var tasks = new Task<object>[amount];
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < amount; i++)
            {
// ReSharper disable RedundantArgumentDefaultValue
                tasks[i] = Cache.RateLimiter.Increment(
                    key, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), amount, 1);
// ReSharper restore RedundantArgumentDefaultValue
            }

            Task.WhenAll(tasks);

            long total = sw.ElapsedMilliseconds;

#pragma warning disable 168
            long count = Cache.Shared.Hashes.GetAll(key).Result
#pragma warning restore 168
                .Where(kvp => "L" != kvp.Key)
                .Select(kvp => long.Parse(Encoding.UTF8.GetString(kvp.Value)))
                .Sum();

//                Assert.True(amount - (amount * .2) < count);
            return total;
        }

        [Fact]
        public void Increment()
        {
            TimeSpan span = TimeSpan.FromSeconds(5);
            TimeSpan bucketSize = TimeSpan.FromSeconds(1);

            object result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 2).Result;
            Assert.Equal(2, (long) result);

            Thread.Sleep(1000);
            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 2).Result;
            Assert.Equal(4, (long) result);

            Thread.Sleep(1000);
            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 2).Result;
            Assert.Equal(6, (long) result);

            Thread.Sleep(1000);
            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 2).Result;
            Assert.Equal(8, (long) result);

            Thread.Sleep(2000);
// ReSharper disable RedundantArgumentDefaultValue
            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 1).Result;
// ReSharper restore RedundantArgumentDefaultValue
            Assert.Equal(7, (long) result);

            Thread.Sleep(2000);
// ReSharper disable RedundantArgumentDefaultValue
            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 1).Result;
// ReSharper restore RedundantArgumentDefaultValue
            Assert.Equal(4, (long) result);

            Thread.Sleep(2000);
// ReSharper disable RedundantArgumentDefaultValue
            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 1).Result;
// ReSharper restore RedundantArgumentDefaultValue
            Assert.Equal(3, (long) result);

            Thread.Sleep(2000);
// ReSharper disable RedundantArgumentDefaultValue
            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 1).Result;
// ReSharper restore RedundantArgumentDefaultValue
            Assert.Equal(3, (long) result);
        }

        [Fact]
        public void Edge()
        {
            TimeSpan span = TimeSpan.FromSeconds(4);
            TimeSpan bucketSize = TimeSpan.FromSeconds(1);

            object result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 2).Result;
            Assert.Equal(2, (long) result);
            Thread.Sleep(2000);

            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 2).Result;
            Assert.Equal(4, (long) result);
            Thread.Sleep(2000);

            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 2).Result;
            Assert.Equal(4, (long) result);
            Thread.Sleep(2000);

            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 2).Result;
            Assert.Equal(4, (long) result);
            Thread.Sleep(2000);

            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 2).Result;
            Assert.Equal(4, (long) result);
            Thread.Sleep(2000);

            result = Cache.RateLimiter.Increment(_key, span, bucketSize, 10, 2).Result;
            Assert.Equal(4, (long) result);
            Thread.Sleep(2000);
        }
    }
}