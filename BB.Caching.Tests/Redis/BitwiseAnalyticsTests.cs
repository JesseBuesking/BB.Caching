namespace BB.Caching.Tests.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using BB.Caching.Redis.Analytics;

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

        private DateTime _now = new DateTime(2000, 1, 1);

        public BitwiseAnalyticsTests()
        {
            this.Cleanup();
        }

        public void Cleanup()
        {
            Cache.Shared.Keys.Delete(_Keys.ToArray());
            for (int month = 1; month <= 3; ++month)
            {
                var monthDt = this._now.AddMonths(month - 1);

                for (int day = 0; day < 30; ++day)
                {
                    var dayDt = monthDt.AddDays(-monthDt.Day).AddDays(day);

                    for (int hour = 0; hour < 24; ++hour)
                    {
                        var hourDt = dayDt.AddHours(hour);

                        // delete hour
                        BitwiseAnalytics.Delete("video", "watch", BitwiseAnalytics.DateTimeUtil.OneHour(hourDt));

                        for (int fifteen = 0; fifteen < 4; ++fifteen)
                        {
                            // delete fifteen
                            BitwiseAnalytics.Delete(
                                "video",
                                "watch",
                                BitwiseAnalytics.DateTimeUtil.FifteenMinutes(hourDt.AddMinutes(15 * fifteen)));
                        }
                    }

                    BitwiseAnalytics.Delete("video", "watch", BitwiseAnalytics.DateTimeUtil.OneDay(dayDt));
                }

                BitwiseAnalytics.Delete("video", "watch", BitwiseAnalytics.DateTimeUtil.OneMonth(monthDt));
            }

            BitwiseAnalytics.Delete("video", "watch", BitwiseAnalytics.DateTimeUtil.QuarterNumber(this._now));
        }

        public IDatabase GetConnection()
        {
            return SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);
        }

        [Fact]
        public void SimpleCohort()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1);
            BitwiseAnalytics.TrackEvent("video", "watch", 2);
            BitwiseAnalytics.TrackEvent("video", "watch", 3);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3);

            long actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", DateTime.UtcNow, DateTime.UtcNow),
                    new Event("anything", "purchase", DateTime.UtcNow, DateTime.UtcNow)));

            Assert.Equal(2, actual);
        }

        [Fact]
        public void GetFifteenMinutes()
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            foreach (int i in new[] { 0, 14 })
            {
                BitwiseAnalytics.TrackEvent("video", "watch", i, TimePrecision.FifteenMinutes, this._now);
            }

            long firstFifteen = BitwiseAnalytics.Count(
                database, BitwiseAnalytics.GetFifteenMinutes("video", "watch", this._now));
            Assert.Equal(2, firstFifteen);
        }

        [Fact]
        public void GetHour()
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            foreach (int i in new[] { 0, 23, 44 })
            {
                BitwiseAnalytics.TrackEvent("video", "watch", i, TimePrecision.FifteenMinutes, this._now.AddMinutes(i));
            }

            long oneHour = BitwiseAnalytics.Count(
                database,
                BitwiseAnalytics.GetHour(database, "video", "watch", this._now));

            Assert.Equal(3, oneHour);
        }

        [Fact]
        public void GetDay()
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            foreach (int i in new[] { 0, 14, 16 })
            {
                BitwiseAnalytics.TrackEvent("video", "watch", i, TimePrecision.FifteenMinutes, this._now.AddHours(i));
            }

            long oneDay = BitwiseAnalytics.Count(
                database,
                BitwiseAnalytics.GetDay(database, "video", "watch", this._now));

            Assert.Equal(3, oneDay);
        }

        [Fact]
        public void GetWeek()
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            // week is 2nd to the 8th, so we use the 2nd, 5th, and 9th
            foreach (int i in new[] { 1, 4, 8 })
            {
                BitwiseAnalytics.TrackEvent(
                    "video", "watch", i, TimePrecision.FifteenMinutes, this._now.AddDays(i));
            }

            // adding 2 days to make sure we're in the right week
            long oneWeek = BitwiseAnalytics.Count(
                database,
                BitwiseAnalytics.GetWeek(database, "video", "watch", this._now.AddDays(2)));

            // only 2 days should count, since one date is in the following week
            Assert.Equal(2, oneWeek);
        }

        [Fact]
        public void GetMonth()
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            foreach (int i in new[] { 0, 11, 16 })
            {
                BitwiseAnalytics.TrackEvent("video", "watch", i, TimePrecision.FifteenMinutes, this._now.AddDays(i));
            }

            long oneMonth = BitwiseAnalytics.Count(
                database,
                BitwiseAnalytics.GetMonth(database, "video", "watch", this._now));

            Assert.Equal(3, oneMonth);
        }

        [Fact]
        public void GetQuarter()
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            // week is 2nd to the 8th, so we use the 2nd, 5th, and 9th
            foreach (int i in new[] { 1, 2, 4 })
            {
                BitwiseAnalytics.TrackEvent(
                    "video", "watch", i, TimePrecision.FifteenMinutes, this._now.AddMonths(i - 1));
            }

            long oneQuarter = BitwiseAnalytics.Count(
                database,
                BitwiseAnalytics.GetQuarter(database, "video", "watch", this._now));

            // only 2 months should count, since one date is in the following quarter
            Assert.Equal(2, oneQuarter);
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
            public void FifteenMinutesInHour()
            {
                DateTime dateTime = new DateTime(2000, 1, 1, 12, 14, 0);
                string[] actual = BitwiseAnalytics.DateTimeUtil.FifteenMinutesInHour(dateTime);
                
                Assert.Equal("200001011200", actual[0]);
                Assert.Equal("200001011215", actual[1]);
                Assert.Equal("200001011230", actual[2]);
                Assert.Equal("200001011245", actual[3]);
            }

            [Fact]
            public void OneHour()
            {
                DateTime dateTime = new DateTime(2000, 1, 1, 13, 16, 0);
                string actual = BitwiseAnalytics.DateTimeUtil.OneHour(dateTime);
                
                Assert.Equal("2000010113", actual);
            }

            [Fact]
            public void HoursInDay()
            {
                DateTime dateTime = new DateTime(2000, 1, 1, 12, 14, 0);
                string[] actual = BitwiseAnalytics.DateTimeUtil.HoursInDay(dateTime);

                for (int i = 0; i < 24; ++i)
                {
                    Assert.Equal(string.Format("20000101{0}", i.ToString("D2")), actual[i]);
                }
            }

            [Fact]
            public void OneDay()
            {
                DateTime dateTime = new DateTime(2000, 1, 1, 13, 16, 0);
                string actual = BitwiseAnalytics.DateTimeUtil.OneDay(dateTime);
                
                Assert.Equal("20000101", actual);
            }

            [Fact]
            public void DaysInMonth()
            {
                DateTime dateTime = new DateTime(2000, 1, 3);
                string[] actual = BitwiseAnalytics.DateTimeUtil.DaysInMonth(dateTime);

                for (int i = 0; i < 31; ++i)
                {
                    Assert.Equal(string.Format("200001{0}", (i + 1).ToString("D2")), actual[i]);
                }
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
            public void DaysInWeek()
            {
                DateTime dateTime = new DateTime(2015, 03, 03);
                string[] actual = BitwiseAnalytics.DateTimeUtil.DaysInWeek(dateTime);
                
                Assert.Equal("20150301", actual[0]);
                Assert.Equal("20150302", actual[1]);
                Assert.Equal("20150303", actual[2]);
                Assert.Equal("20150304", actual[3]);
                Assert.Equal("20150305", actual[4]);
                Assert.Equal("20150306", actual[5]);
                Assert.Equal("20150307", actual[6]);
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

            [Fact]
            public void MonthsInQuarter()
            {
                for (int i = 0; i < 4; ++i)
                {
                    int month = (i * 3) + 1;
                    DateTime dateTime = new DateTime(2000, month, 1);
                    string[] actual = BitwiseAnalytics.DateTimeUtil.MonthsInQuarter(dateTime);

                    Assert.Equal(string.Format("2000{0}", month.ToString("D2")), actual[0]);
                    Assert.Equal(string.Format("2000{0}", (month + 1).ToString("D2")), actual[1]);
                    Assert.Equal(string.Format("2000{0}", (month + 2).ToString("D2")), actual[2]);
                }
            }

            public class MinKeysForRange
            {
                [Fact]
                public void StartLessThanEnd()
                {
                    Assert.Throws(
                        typeof(Exception),
                        () =>
                            {
                                BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 2), new DateTime(2000, 1, 1));
                    });
                }

                public class FifteenMinuteIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 0, 1, 0));

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.FifteenMinutes, "200001010000"),
                            keys[0]);
                    }

                    [Fact]
                    public void BackToBackFifteenMinuteBlocks()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 0, 16, 0));

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.FifteenMinutes, "200001010000"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.FifteenMinutes, "200001010015"),
                            keys[1]);
                    }

                    [Fact]
                    public void FifteenMinuteBlocksAtBothEndRequiringFullHour()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 1, 0, 0));

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010100"),
                            keys[0]);
                    }

                    [Fact]
                    public void OneHourPlusAnotherFifteenMinutes()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 1, 1, 0));

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010100"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.FifteenMinutes, "200001010100"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneHourMinusAnotherFifteenMinutes()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 59, 0),
                            new DateTime(2000, 1, 1, 2, 0, 0));

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.FifteenMinutes, "200001010045"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010101"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneHour()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 1, 0, 0),
                            new DateTime(2000, 1, 1, 2, 0, 0));

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010101"),
                            keys[0]);
                    }

                    [Fact]
                    public void ThreeFullHours()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 2, 59, 0));

                        Assert.Equal(3, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010100"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010101"),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010102"),
                            keys[2]);
                    }

                    [Fact]
                    public void OneFullDay()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 2, 0, 0, 0));

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneDay, "20000101"),
                            keys[0]);
                    }

                    [Fact]
                    public void OneFullDayPlusFifteenMinutes()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 2, 0, 1, 0));

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneDay, "20000101"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.FifteenMinutes, "200001020000"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneFullMonthPlusFifteenMinutes()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 2, 1, 0, 1, 0));

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.FifteenMinutes, "200002010000"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneFullYearPlusFifteenMinutes()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2001, 1, 1, 0, 1, 0));

                        Assert.Equal(13, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200002"),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200003"),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200004"),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200005"),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200006"),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200007"),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200008"),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200009"),
                            keys[8]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200010"),
                            keys[9]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200011"),
                            keys[10]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200012"),
                            keys[11]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.FifteenMinutes, "200101010000"),
                            keys[12]);
                    }

                    [Fact]
                    public void OneFullYearMinusFifteenMinutes()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 12, 31, 23, 59, 0),
                            new DateTime(2002, 1, 1, 0, 0, 0));

                        Assert.Equal(13, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.FifteenMinutes, "200012312345"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200101"),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200102"),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200103"),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200104"),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200105"),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200106"),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200107"),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200108"),
                            keys[8]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200109"),
                            keys[9]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200110"),
                            keys[10]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200111"),
                            keys[11]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200112"),
                            keys[12]);
                    }
                }

                public class OneHourIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 0, 1, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010100"),
                            keys[0]);
                    }

                    [Fact]
                    public void OneHourPlusOneMinuteRequiresTwoFullHours()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 1, 1, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010100"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010101"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneDayPlusOneMinute()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 2, 0, 1, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneDay, "20000101"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010200"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneDayMinusOneMinute()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 23, 59, 0),
                            new DateTime(2000, 1, 3, 0, 0, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000010123"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneDay, "20000102"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneMonthPlusOneMinute()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 2, 1, 0, 1, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2000020100"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneYearPlusOneMinute()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2001, 1, 1, 0, 1, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(13, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200002"),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200003"),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200004"),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200005"),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200006"),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200007"),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200008"),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200009"),
                            keys[8]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200010"),
                            keys[9]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200011"),
                            keys[10]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200012"),
                            keys[11]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneHour, "2001010100"),
                            keys[12]);
                    }
                }

                public class OneDayIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 0, 1, 0),
                            TimeInterval.OneDay);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneDay, "20000101"),
                            keys[0]);
                    }

                    [Fact]
                    public void TimeInTwoSeparateHours()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 1, 1, 0),
                            TimeInterval.OneDay);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneDay, "20000101"),
                            keys[0]);
                    }

                    [Fact]
                    public void OneDayAndOneMinute()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 2, 0, 1, 0),
                            TimeInterval.OneDay);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneDay, "20000101"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneDay, "20000102"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneMonthAndOneMinute()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 2, 1, 0, 1, 0),
                            TimeInterval.OneDay);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneDay, "20000201"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneYearAndOneMinute()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2001, 1, 1, 0, 1, 0),
                            TimeInterval.OneDay);

                        Assert.Equal(13, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200002"),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200003"),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200004"),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200005"),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200006"),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200007"),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200008"),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200009"),
                            keys[8]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200010"),
                            keys[9]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200011"),
                            keys[10]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200012"),
                            keys[11]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneDay, "20010101"),
                            keys[12]);
                    }
                }

                public class OneMonthIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 0, 1, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                    }

                    [Fact]
                    public void TwoHours()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 1, 1, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                    }

                    [Fact]
                    public void TwoDays()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 2, 0, 1, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                    }

                    [Fact]
                    public void OneMonthAndOneMinute()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 2, 1, 0, 1, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200002"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneMonthOddChange()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 23, 59, 0),
                            new DateTime(2000, 2, 3, 0, 0, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200002"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneMonthMinusOneMinute()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 31, 23, 59, 0),
                            new DateTime(2000, 3, 1, 0, 0, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200002"),
                            keys[1]);
                    }

                    [Fact]
                    public void OneYearAndOneMinute()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2001, 1, 1, 0, 1, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(13, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200001"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200002"),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200003"),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200004"),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200005"),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200006"),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200007"),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200008"),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200009"),
                            keys[8]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200010"),
                            keys[9]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200011"),
                            keys[10]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "200012"),
                            keys[11]);
                    }
                }

                public class OneWeekIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2015, 3, 1, 0, 0, 0),
                            new DateTime(2015, 3, 1, 0, 1, 0),
                            TimeInterval.Week);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.Week, "2015W010"),
                            keys[0]);
                    }

                    [Fact]
                    public void ThreeWeeks()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2015, 3, 1, 0, 0, 0),
                            new DateTime(2015, 3, 15, 0, 1, 0),
                            TimeInterval.Week);

                        Assert.Equal(3, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.Week, "2015W010"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.Week, "2015W011"),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.Week, "2015W012"),
                            keys[2]);
                    }
                }

                public class OneQuarterIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2015, 3, 1, 0, 0, 0),
                            new DateTime(2015, 3, 1, 0, 1, 0),
                            TimeInterval.Quarter);

                        Assert.Equal(3, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201501"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201502"),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201503"),
                            keys[2]);
                    }

                    [Fact]
                    public void ThreeQuarters()
                    {
                        Tuple<TimeInterval, string>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2015, 3, 1, 0, 0, 0),
                            new DateTime(2015, 9, 1, 0, 1, 0),
                            TimeInterval.Quarter);

                        Assert.Equal(9, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201501"),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201502"),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201503"),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201504"),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201505"),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201506"),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201507"),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201508"),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string>(TimeInterval.OneMonth, "201509"),
                            keys[8]);
                    }
                }
            }
        }
    }
}