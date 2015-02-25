using System;
using BB.Caching.Redis;
using Xunit;

namespace BB.Caching.Tests.Redis
{
    public class StatisticTestsFixture : IDisposable
    {
        public StatisticTestsFixture()
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

    public class StatisticTests : IUseFixture<DefaultTestFixture>, IUseFixture<StatisticTestsFixture>, IDisposable
    {
        private const string KEY = "StatisticTests.Key";

        public StatisticTests()
        {
            Cache.Shared.Keys.Remove(KEY).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(KEY).Wait();
        }

        [Fact]
        public void SetAndGet()
        {
            Statistics.SetStatistic(KEY, 1.0).Wait();
            Statistics.SetStatistic(KEY, 2.0).Wait();
            Statistics.SetStatistic(KEY, 3.0).Wait();
            Statistics.SetStatistic(KEY, 4.0).Wait();
            Statistics.SetStatistic(KEY, 5.0).Wait();

            var stat = Statistics.GetStatistics(KEY).Result;

            Assert.Equal(5.0, stat.MaximumValue);
            Assert.Equal(3.0, stat.Mean);
            Assert.Equal(1.0, stat.MinimumValue);
            Assert.Equal(5, stat.NumberOfValues);
            Assert.Equal(Math.Sqrt(5.0d/2.0d), stat.PopulationStandardDeviation);
            Assert.Equal(5.0d/2.0d, stat.PopulationVariance);

            stat = Statistics.GetStatistics(KEY).Result;

            Assert.Equal(5.0, stat.MaximumValue);
            Assert.Equal(3.0, stat.Mean);
            Assert.Equal(1.0, stat.MinimumValue);
            Assert.Equal(5, stat.NumberOfValues);
            Assert.Equal(Math.Sqrt(5.0d/2.0d), stat.PopulationStandardDeviation);
            Assert.Equal(5.0d/2.0d, stat.PopulationVariance);
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }

        public void SetFixture(StatisticTestsFixture data)
        {
        }
    }
}