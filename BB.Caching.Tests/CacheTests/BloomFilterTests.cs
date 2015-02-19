using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class BloomFilterTests : TestBase
    {
        private const string _key = "key1";

        private const float _falsePositivePercentage = 0.001f;

        public BloomFilterTests()
        {
            try
            {
                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-0", new SafeRedisConnection(this.TestIp, this.TestPort1)));

                if (0 != this.TestPort2)
                {
                    Cache.Shared.AddRedisConnectionGroup(
                        new RedisConnectionGroup("node-1", new SafeRedisConnection(this.TestIp, this.TestPort2)));
                }

                Cache.PubSub.Configure(new SafeRedisConnection(this.TestIp, this.TestPort1));
                Cache.Shared.SetPubSubRedisConnection();
            }
            catch (Exception)
            {
            }

            Cache.Shared.Keys.Remove(_key).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(_key).Wait();
        }

        [Fact]
        public void Add()
        {
            var bloomFilter = new Cache.BloomFilter();
            bloomFilter.Add(_key, "test");

            var b = bloomFilter.IsSet(_key, "test").Result;
            Assert.True(b);

            b = bloomFilter.IsSet(_key, "again").Result;
            Assert.False(b);
        }

        [Fact]
        public void SetPerformance()
        {
            const int asyncAmount = 30000;
            var asyncMs = Set(asyncAmount, _key, "test");

            Console.WriteLine("BloomFilter Sets:");
            Console.WriteLine("\t{0:#,##0.0#} aops/ms", (float) asyncAmount/asyncMs);
            Console.WriteLine("\t{0:#,##0.0#} aops/s", (float) asyncAmount*1000/asyncMs);
        }

        [Fact]
        public void GetPerformance()
        {
            const int asyncAmount = 30000;
            var bloomFilter = new Cache.BloomFilter();
            bloomFilter.Add(_key, "test");
            var asyncMs = Get(asyncAmount, _key, "test");

            Console.WriteLine("BloomFilter Gets:");
            Console.WriteLine("\t{0:#,##0.0#} aops/ms", (float) asyncAmount/asyncMs);
            Console.WriteLine("\t{0:#,##0.0#} aops/s", (float) asyncAmount*1000/asyncMs);
        }

        private static long Set(int amount, string key, string value)
        {
// ReSharper disable RedundantArgumentDefaultValue
            var bloomFilter = new Cache.BloomFilter(amount, _falsePositivePercentage);
// ReSharper restore RedundantArgumentDefaultValue
            var tasks = new Task[amount];
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < amount; i++)
                tasks[i] = bloomFilter.Add(key, value);

            Task.WhenAll(tasks);

            return sw.ElapsedMilliseconds;
        }

        private static long Get(int amount, string key, string value)
        {
// ReSharper disable RedundantArgumentDefaultValue
            var bloomFilter = new Cache.BloomFilter(amount, _falsePositivePercentage);
// ReSharper restore RedundantArgumentDefaultValue
            var tasks = new Task<bool>[amount];
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < amount; i++)
                tasks[i] = bloomFilter.IsSet(key, value);

            Task.WhenAll(tasks);

            return sw.ElapsedMilliseconds;
        }

        [Fact]
        public void FalsePositiveTest()
        {
            const int stringLength = 3;
            int half = (int) Math.Pow(26, stringLength)/2;
// ReSharper disable RedundantArgumentDefaultValue
            var bloomFilter = new Cache.BloomFilter(half, _falsePositivePercentage);
// ReSharper restore RedundantArgumentDefaultValue
            float fpPercentage = BloomTest(bloomFilter, 3, _key);

            Console.WriteLine("Target false positive percentage: {0:#0.#%}", _falsePositivePercentage);
            Console.WriteLine("Actual: {0:#0.###%}, or 1 in {1:#,###.#}", fpPercentage, 1/fpPercentage);
            Assert.True(_falsePositivePercentage*2 >= fpPercentage);
        }

        private static float BloomTest(Cache.BloomFilter bloomFilter, int stringLength, string key)
        {
            var values = new List<string>();
            long dbl = bloomFilter.Options.NumberOfItems*2;
            for (int i = 0; i < dbl; i++)
            {
                int z = i;
                string s = "";
                for (int j = 1; j < stringLength + 1; ++j)
                {
                    s = (char) (z%26 + 65) + s;
                    z = z/26;
                }
                bloomFilter.Add(key, s);

                ++i;
                z = i;

                s = "";
                for (int j = 1; j < stringLength + 1; ++j)
                {
                    s = (char) (z%26 + 65) + s;
                    z = z/26;
                }
                values.Add(s);
            }

            int fpCount = values.Sum(value => bloomFilter.IsSet(key, value).Result ? 1 : 0);

            return ((float) fpCount)/bloomFilter.Options.NumberOfItems;
        }
    }
}