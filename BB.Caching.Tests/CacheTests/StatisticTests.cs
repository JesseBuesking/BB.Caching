using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class StatisticTestsFixture : IDisposable
    {
        public StatisticTestsFixture()
        {
            Cache.Prepare();
        }

        public void Dispose()
        {
        }
    }

    public class StatisticTests : IUseFixture<DefaultTestFixture>, IUseFixture<StatisticTestsFixture>, IDisposable
    {
        private const string _key = "key1";

        public StatisticTests()
        {
            Cache.Shared.Keys.Remove(_key).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(_key).Wait();
        }

        [Fact]
        public void SetAndGet()
        {
            Cache.Stats.SetStatistic(_key, 1.0).Wait();
            Cache.Stats.SetStatistic(_key, 2.0).Wait();
            Cache.Stats.SetStatistic(_key, 3.0).Wait();
            Cache.Stats.SetStatistic(_key, 4.0).Wait();
            Cache.Stats.SetStatistic(_key, 5.0).Wait();

            var stat = Cache.Stats.GetStatistics(_key).Result;

            Assert.Equal(5.0, stat.MaximumValue);
            Assert.Equal(3.0, stat.Mean);
            Assert.Equal(1.0, stat.MinimumValue);
            Assert.Equal(5, stat.NumberOfValues);
            Assert.Equal(Math.Sqrt(5.0d/2.0d), stat.PopulationStandardDeviation);
            Assert.Equal(5.0d/2.0d, stat.PopulationVariance);

            stat = Cache.Stats.GetStatistics(_key).Result;

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
            var asyncMs = Get(asyncAmount, _key);

            Console.WriteLine("Statistics Ops:");
            Console.WriteLine("\t{0:#,##0.0#} aops/ms", (float) asyncAmount/asyncMs);
            Console.WriteLine("\t{0:#,##0.0#} aops/s", (float) asyncAmount*1000/asyncMs);
        }

        private static long Get(int amount, string key)
        {
            var tasks = new Task[amount];
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < amount; i++)
                tasks[i] = Cache.Stats.SetStatistic(key, 1.0);

            Task.WhenAll(tasks);

            return sw.ElapsedMilliseconds;
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }

        public void SetFixture(StatisticTestsFixture data)
        {
        }
    }
}