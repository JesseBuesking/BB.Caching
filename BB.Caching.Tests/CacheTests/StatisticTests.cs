using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    internal class StatisticTests
    {
        public string Key1 = "key1";

        public StatisticTests()
        {
            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

            Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27", 6379));

            Cache.Statistic.Prepare();

            Cache.Shared.Keys.Remove(this.Key1).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(this.Key1).Wait();
        }

        [Fact]
        public void SetAndGet()
        {
            Cache.Statistic.SetStatistic(this.Key1, 1.0).Wait();
            Cache.Statistic.SetStatistic(this.Key1, 2.0).Wait();
            Cache.Statistic.SetStatistic(this.Key1, 3.0).Wait();
            Cache.Statistic.SetStatistic(this.Key1, 4.0).Wait();
            Cache.Statistic.SetStatistic(this.Key1, 5.0).Wait();

            var stat = Cache.Statistic.GetStatistic(this.Key1).Result;

            Assert.Equal(5.0, stat.MaximumValue);
            Assert.Equal(3.0, stat.Mean);
            Assert.Equal(1.0, stat.MinimumValue);
            Assert.Equal(5, stat.NumberOfValues);
            Assert.Equal(Math.Sqrt(5.0d/2.0d), stat.PopulationStandardDeviation);
            Assert.Equal(5.0d/2.0d, stat.PopulationVariance);

            stat = Cache.Statistic.GetStatistic(this.Key1).Result;

            Assert.Equal(5.0, stat.MaximumValue);
            Assert.Equal(3.0, stat.Mean);
            Assert.Equal(1.0, stat.MinimumValue);
            Assert.Equal(5, stat.NumberOfValues);
            Assert.Equal(Math.Sqrt(5.0d/2.0d), stat.PopulationStandardDeviation);
            Assert.Equal(5.0d/2.0d, stat.PopulationVariance);
        }

        [Fact]
        public void Performance()
        {
            const int asyncAmount = 30000;
            var asyncMs = Get(asyncAmount, this.Key1);

            Console.WriteLine("{0:#,##0.0#} async ops per ms", (float) asyncAmount/asyncMs);
            Console.WriteLine();
            Console.WriteLine("async ({0:#,##0}): {1:#,##0}ms", asyncAmount, asyncMs);
        }

        private static long Get(int amount, string key)
        {
            var tasks = new Task[amount];
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < amount; i++)
                tasks[i] = Cache.Statistic.SetStatistic(key, 1.0);

            Task.WhenAll(tasks);

            return sw.ElapsedMilliseconds;
        }
    }
}