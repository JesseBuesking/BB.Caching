using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    internal class BloomFilterTests
    {
        public string Key1 = "key1";

        public BloomFilterTests()
        {
            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

            Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27", 6379));

            Cache.BloomFilter.Prepare();

            Cache.Shared.Keys.Remove(this.Key1).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(this.Key1).Wait();
        }

        [Fact]
        public void Add()
        {
            var bloomFilter = new Cache.BloomFilter();
            bloomFilter.Add(this.Key1, "test");

            var b = bloomFilter.IsSet(this.Key1, "test").Result;
            Assert.True(b);

            b = bloomFilter.IsSet(this.Key1, "again").Result;
            Assert.False(b);
        }

        [Fact]
        public void SetPerformance()
        {
            const int asyncAmount = 30000;
            var asyncMs = Set(asyncAmount, this.Key1, "test");

            Console.WriteLine("{0:#,##0.0#} async ops per ms", (float) asyncAmount/asyncMs);
            Console.WriteLine();
            Console.WriteLine("async ({0:#,##0}): {1:#,##0}ms", asyncAmount, asyncMs);
        }

        [Fact]
        public void GetPerformance()
        {
            const int asyncAmount = 30000;
            var bloomFilter = new Cache.BloomFilter();
            bloomFilter.Add(this.Key1, "test");
            var asyncMs = Get(asyncAmount, this.Key1, "test");

            Console.WriteLine("{0:#,##0.0#} async ops per ms", (float) asyncAmount/asyncMs);
            Console.WriteLine();
            Console.WriteLine("async ({0:#,##0}): {1:#,##0}ms", asyncAmount, asyncMs);
        }

        private static long Set(int amount, string key, string value)
        {
            var bloomFilter = new Cache.BloomFilter(amount, 0.001f);
            var tasks = new Task[amount];
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < amount; i++)
                tasks[i] = bloomFilter.Add(key, value);

            Task.WhenAll(tasks);

            return sw.ElapsedMilliseconds;
        }

        private static long Get(int amount, string key, string value)
        {
            var bloomFilter = new Cache.BloomFilter(amount, 0.001f);
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
            const float falsePositivePercentage = 0.001f;
            int half = (int) Math.Pow(26, stringLength)/2;
            var bloomFilter = new Cache.BloomFilter(half, falsePositivePercentage);
            float fpPercentage = BloomTest(bloomFilter, 3, this.Key1);

            Console.WriteLine("fp: {0:#0.####}, {0:#0.####%}, or 1 in {1:#,###.0#}", fpPercentage, 1/fpPercentage);
            Assert.True(falsePositivePercentage*2 >= fpPercentage);
        }

        private static float BloomTest(Cache.BloomFilter bloomFilter, int stringLength, string key)
        {
            var values = new List<string>();
            for (int i = 0; i < bloomFilter.Option.NumberOfItems*2; i++)
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

            int fpCount = 0;
            foreach (string value in values)
                fpCount += bloomFilter.IsSet(key, value).Result ? 1 : 0;

            return ((float) fpCount)/bloomFilter.Option.NumberOfItems;
        }
    }
}