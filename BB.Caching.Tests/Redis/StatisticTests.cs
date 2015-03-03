namespace BB.Caching.Tests.Redis
{
    using System;

    using BB.Caching.Redis;

    using Xunit;

    public class StatisticTests : IUseFixture<DefaultTestFixture>, IDisposable
    {
        private const string KEY = "StatisticTests.Key";

        public StatisticTests()
        {
            Cache.Shared.Keys.DeleteAsync(KEY).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.DeleteAsync(KEY).Wait();
        }

        [Fact]
        public void SetAndGet()
        {
            Statistics.SetStatisticAsync(KEY, 1.0).Wait();
            Statistics.SetStatisticAsync(KEY, 2.0).Wait();
            Statistics.SetStatisticAsync(KEY, 3.0).Wait();
            Statistics.SetStatisticAsync(KEY, 4.0).Wait();
            Statistics.SetStatisticAsync(KEY, 5.0).Wait();

            var stat = Statistics.GetStatisticsAsync(KEY).Result;

            Assert.Equal(5.0, stat.MaximumValue);
            Assert.Equal(3.0, stat.Mean);
            Assert.Equal(1.0, stat.MinimumValue);
            Assert.Equal(5, stat.NumberOfValues);
            Assert.Equal(Math.Sqrt(5.0d / 2.0d), stat.PopulationStandardDeviation);
            Assert.Equal(5.0d / 2.0d, stat.PopulationVariance);

            stat = Statistics.GetStatisticsAsync(KEY).Result;

            Assert.Equal(5.0, stat.MaximumValue);
            Assert.Equal(3.0, stat.Mean);
            Assert.Equal(1.0, stat.MinimumValue);
            Assert.Equal(5, stat.NumberOfValues);
            Assert.Equal(Math.Sqrt(5.0d / 2.0d), stat.PopulationStandardDeviation);
            Assert.Equal(5.0d / 2.0d, stat.PopulationVariance);
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }
    }
}