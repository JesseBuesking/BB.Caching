namespace BB.Caching.Tests.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using BB.Caching.Redis;

    using StackExchange.Redis;

    using Xunit;

    public sealed class BitwiseAnalyticsTests : IUseFixture<DefaultTestFixture>
    {
        private static readonly List<RedisKey> _Keys = new List<RedisKey>
        {
            "BitwiseAnalyticsTests.Key1",
            "BitwiseAnalyticsTests.Key2",
            "BitwiseAnalyticsTests.Key3"
        };

        public BitwiseAnalyticsTests()
        {
            Cache.Shared.Keys.Delete(_Keys.ToArray());
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Delete(_Keys.ToArray());
        }

        public IDatabase GetConnection()
        {
            return SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);
        }

        [Fact]
        public void BitwiseNot()
        {
            RedisKey firstKey = _Keys.ElementAt(0);
            RedisKey secondKey = _Keys.ElementAt(1);

            Cache.Shared.Strings.SetBit(firstKey, 7, true);
            BitwiseAnalytics.BitwiseNot(this.GetConnection(), secondKey, firstKey);

            Assert.Equal(7, Cache.Shared.Strings.CountSetBits(secondKey));
        }

        [Fact]
        public void BitwiseAndNotAligned()
        {
            RedisKey firstKey = _Keys.ElementAt(0);
            RedisKey secondKey = _Keys.ElementAt(1);
            RedisKey thirdKey = _Keys.ElementAt(2);

            Cache.Shared.Strings.SetBit(firstKey, 0, true);
            Cache.Shared.Strings.SetBit(secondKey, 1, true);
            BitwiseAnalytics.BitwiseAnd(this.GetConnection(), thirdKey, new[] { firstKey, secondKey });

            Assert.Equal(0, Cache.Shared.Strings.CountSetBits(thirdKey));
        }

        [Fact]
        public void BitwiseAndOverlap()
        {
            RedisKey firstKey = _Keys.ElementAt(0);
            RedisKey secondKey = _Keys.ElementAt(1);
            RedisKey thirdKey = _Keys.ElementAt(2);

            Cache.Shared.Strings.SetBit(firstKey, 1, true);
            Cache.Shared.Strings.SetBit(firstKey, 0, true);
            Cache.Shared.Strings.SetBit(secondKey, 1, true);
            BitwiseAnalytics.BitwiseAnd(this.GetConnection(), thirdKey, new[] { firstKey, secondKey });

            Assert.Equal(1, Cache.Shared.Strings.CountSetBits(thirdKey));
        }

        [Fact]
        public void BitwiseOrNotAligned()
        {
            RedisKey firstKey = _Keys.ElementAt(0);
            RedisKey secondKey = _Keys.ElementAt(1);
            RedisKey thirdKey = _Keys.ElementAt(2);

            Cache.Shared.Strings.SetBit(firstKey, 0, true);
            Cache.Shared.Strings.SetBit(secondKey, 1, true);
            BitwiseAnalytics.BitwiseOr(this.GetConnection(), thirdKey, new[] { firstKey, secondKey });

            Assert.Equal(2, Cache.Shared.Strings.CountSetBits(thirdKey));
        }

        [Fact]
        public void BitwiseOrOverlap()
        {
            RedisKey firstKey = _Keys.ElementAt(0);
            RedisKey secondKey = _Keys.ElementAt(1);
            RedisKey thirdKey = _Keys.ElementAt(2);

            Cache.Shared.Strings.SetBit(firstKey, 1, true);
            Cache.Shared.Strings.SetBit(secondKey, 1, true);
            BitwiseAnalytics.BitwiseOr(this.GetConnection(), thirdKey, new[] { firstKey, secondKey });

            Assert.Equal(1, Cache.Shared.Strings.CountSetBits(thirdKey));
        }

        [Fact]
        public void BitwiseXorNotAligned()
        {
            RedisKey firstKey = _Keys.ElementAt(0);
            RedisKey secondKey = _Keys.ElementAt(1);
            RedisKey thirdKey = _Keys.ElementAt(2);

            Cache.Shared.Strings.SetBit(firstKey, 0, true);
            Cache.Shared.Strings.SetBit(secondKey, 1, true);
            BitwiseAnalytics.BitwiseXOr(this.GetConnection(), thirdKey, new[] { firstKey, secondKey });

            Assert.Equal(2, Cache.Shared.Strings.CountSetBits(thirdKey));
        }

        [Fact]
        public void BitwiseXorOverlap()
        {
            RedisKey firstKey = _Keys.ElementAt(0);
            RedisKey secondKey = _Keys.ElementAt(1);
            RedisKey thirdKey = _Keys.ElementAt(2);

            Cache.Shared.Strings.SetBit(firstKey, 1, true);
            Cache.Shared.Strings.SetBit(secondKey, 1, true);
            BitwiseAnalytics.BitwiseXOr(this.GetConnection(), thirdKey, new[] { firstKey, secondKey });

            Assert.Equal(0, Cache.Shared.Strings.CountSetBits(thirdKey));
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }

        public class DateTimteUtilTests
        {
            [Fact]
            public void FifteenMinutesMiddleOfInterval()
            {
                DateTime dateTime = new DateTime(2000, 1, 1, 12, 7, 0);
                string actual = BitwiseAnalytics.DateTimeUtil.FifteenMinutes(dateTime);
                
                Assert.Equal("200001011200", actual);
            }

            [Fact]
            public void FifteenMinutesStartOfInterval()
            {
                DateTime dateTime = new DateTime(2000, 1, 1, 12, 0, 0);
                string actual = BitwiseAnalytics.DateTimeUtil.FifteenMinutes(dateTime);
                
                Assert.Equal("200001011200", actual);
            }

            [Fact]
            public void FifteenMinutesEndOfInterval()
            {
                DateTime dateTime = new DateTime(2000, 1, 1, 12, 14, 0);
                string actual = BitwiseAnalytics.DateTimeUtil.FifteenMinutes(dateTime);
                
                Assert.Equal("200001011200", actual);
            }

            [Fact]
            public void OneHour()
            {
                DateTime dateTime = new DateTime(2000, 1, 1, 13, 16, 0);
                string actual = BitwiseAnalytics.DateTimeUtil.OneHour(dateTime);
                
                Assert.Equal("2000010113", actual);
            }

            [Fact]
            public void OneDay()
            {
                DateTime dateTime = new DateTime(2000, 1, 1, 13, 16, 0);
                string actual = BitwiseAnalytics.DateTimeUtil.OneDay(dateTime);
                
                Assert.Equal("20000101", actual);
            }

            [Fact]
            public void OneMonth()
            {
                DateTime dateTime = new DateTime(2000, 1, 1, 13, 16, 0);
                string actual = BitwiseAnalytics.DateTimeUtil.OneMonth(dateTime);
                
                Assert.Equal("200001", actual);
            }

            [Fact]
            public void WeekNumberSingleDigit()
            {
                DateTime dateTime = new DateTime(2000, 1, 1);
                string actual = BitwiseAnalytics.DateTimeUtil.WeekNumber(dateTime);
                
                Assert.Equal("2000W001", actual);
            }

            [Fact]
            public void WeekNumberDoubleDigit()
            {
                DateTime dateTime = new DateTime(2000, 12, 30);
                string actual = BitwiseAnalytics.DateTimeUtil.WeekNumber(dateTime);
                
                Assert.Equal("2000W053", actual);
            }

            [Fact]
            public void Quarter1()
            {
                for (int i = 0; i < 3; ++i)
                {
                    DateTime dateTime = new DateTime(2000, i + 1, 1);
                    string actual = BitwiseAnalytics.DateTimeUtil.QuarterNumber(dateTime);

                    Assert.Equal("2000Q1", actual);
                }
            }

            [Fact]
            public void Quarter2()
            {
                for (int i = 0; i < 3; ++i)
                {
                    DateTime dateTime = new DateTime(2000, i + 4, 1);
                    string actual = BitwiseAnalytics.DateTimeUtil.QuarterNumber(dateTime);

                    Assert.Equal("2000Q2", actual);
                }
            }

            [Fact]
            public void Quarter3()
            {
                for (int i = 0; i < 3; ++i)
                {
                    DateTime dateTime = new DateTime(2000, i + 7, 1);
                    string actual = BitwiseAnalytics.DateTimeUtil.QuarterNumber(dateTime);

                    Assert.Equal("2000Q3", actual);
                }
            }

            [Fact]
            public void Quarter4()
            {
                for (int i = 0; i < 3; ++i)
                {
                    DateTime dateTime = new DateTime(2000, i + 10, 1);
                    string actual = BitwiseAnalytics.DateTimeUtil.QuarterNumber(dateTime);

                    Assert.Equal("2000Q4", actual);
                }
            }
        }
    }
}