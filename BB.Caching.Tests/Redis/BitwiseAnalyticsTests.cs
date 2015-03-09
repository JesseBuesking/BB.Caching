namespace BB.Caching.Tests.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
            SharedCache.Instance.FlushDatabase(SharedCache.Instance.GetAnalyticsWriteConnection());
        }

        public IDatabase GetConnection()
        {
            return SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);
        }

        [Fact]
        public void EnumerableWorks()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 3, TimePrecision.FifteenMinutes, this._now.AddMinutes(15));
            BitwiseAnalytics.TrackEvent("video", "watch", 12, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 77, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 12, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 77, TimePrecision.FifteenMinutes, this._now);

            var key = Ops.And(
                new Event("video", "watch", this._now, this._now.AddMinutes(16), TimeInterval.FifteenMinutes),
                new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.FifteenMinutes));

            var ids = new RedisKeyBitEnumerable(key).ToList();

            Assert.Equal(4, ids.Count);

            Assert.Equal(1, ids.ElementAt(0));
            Assert.Equal(3, ids.ElementAt(1));
            Assert.Equal(12, ids.ElementAt(2));
            Assert.Equal(77, ids.ElementAt(3));
        }

        [Fact]
        public void EnumerableWorksAsync()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 3, TimePrecision.FifteenMinutes, this._now.AddMinutes(15));
            BitwiseAnalytics.TrackEvent("video", "watch", 12, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 77, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 12, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 77, TimePrecision.FifteenMinutes, this._now);

            var key = Ops.AndAsync(
                new Event("video", "watch", this._now, this._now.AddMinutes(16), TimeInterval.FifteenMinutes),
                new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.FifteenMinutes)).Result;

            var ids = new RedisKeyBitEnumerable(key).ToList();

            Assert.Equal(4, ids.Count);

            Assert.Equal(1, ids.ElementAt(0));
            Assert.Equal(3, ids.ElementAt(1));
            Assert.Equal(12, ids.ElementAt(2));
            Assert.Equal(77, ids.ElementAt(3));
        }

        [Fact]
        public void HasEventEvents()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes);

            bool actual = BitwiseAnalytics.HasEvent("video", "watch", 1);

            Assert.True(actual);
        }

        [Fact]
        public void HasEventEventsAsync()
        {
            BitwiseAnalytics.TrackEventAsync("video", "watch", 1, TimePrecision.FifteenMinutes).Wait();

            bool actual = BitwiseAnalytics.HasEventAsync("video", "watch", 1).Result;

            Assert.True(actual);
        }

        [Fact]
        public void HasEventAcrossTime()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddDays(2));

            bool actual = BitwiseAnalytics.HasEvent(
                "video", "watch", 1, this._now, this._now.AddDays(2).AddMinutes(1));

            Assert.True(actual);

            actual = BitwiseAnalytics.HasEvent(
                "video", "watch", 1, this._now.AddMonths(1), this._now.AddMonths(2));

            Assert.False(actual);
        }

        [Fact]
        public void HasEventAcrossTimeAsync()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddDays(2));

            bool actual = BitwiseAnalytics.HasEventAsync(
                "video", "watch", 1, this._now, this._now.AddDays(2).AddMinutes(1)).Result;

            Assert.True(actual);

            actual = BitwiseAnalytics.HasEventAsync(
                "video", "watch", 1, this._now.AddMonths(1), this._now.AddMonths(2)).Result;

            Assert.False(actual);
        }

        [Fact]
        public void GetCountsFifteen()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddMinutes(16));

            var counts = BitwiseAnalytics.GetCounts(
                "video",
                "watch",
                this._now,
                this._now.AddHours(1).AddMinutes(1),
                timeInterval: TimeInterval.FifteenMinutes);

            Assert.Equal(5, counts.Count);
            Assert.Equal(3, counts.Select(x => x.Item2).Sum());

            var first = counts.ElementAt(0);
            var second = counts.ElementAt(1);
            var third = counts.ElementAt(2);
            var fourth = counts.ElementAt(3);
            var fifth = counts.ElementAt(4);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)), first.Item1);
            Assert.Equal(2, first.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(15), second.Item1);
            Assert.Equal(1, second.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(30), third.Item1);
            Assert.Equal(0, third.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(45), fourth.Item1);
            Assert.Equal(0, fourth.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(60), fifth.Item1);
            Assert.Equal(0, fifth.Item2);
        }

        [Fact]
        public void GetCountsFifteenAsync()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddMinutes(16));

            var counts = BitwiseAnalytics.GetCountsAsync(
                "video",
                "watch",
                this._now,
                this._now.AddHours(1).AddMinutes(1),
                timeInterval: TimeInterval.FifteenMinutes).Result;

            Assert.Equal(5, counts.Count);
            Assert.Equal(3, counts.Select(x => x.Item2).Sum());

            var first = counts.ElementAt(0);
            var second = counts.ElementAt(1);
            var third = counts.ElementAt(2);
            var fourth = counts.ElementAt(3);
            var fifth = counts.ElementAt(4);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)), first.Item1);
            Assert.Equal(2, first.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(15), second.Item1);
            Assert.Equal(1, second.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(30), third.Item1);
            Assert.Equal(0, third.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(45), fourth.Item1);
            Assert.Equal(0, fourth.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(60), fifth.Item1);
            Assert.Equal(0, fifth.Item2);
        }

        [Fact]
        public void GetCountsFifteenSingleEventId()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddMinutes(16));

            var counts = BitwiseAnalytics.GetCounts(
                "video",
                "watch",
                this._now,
                this._now.AddHours(1).AddMinutes(1),
                1,
                TimeInterval.FifteenMinutes);

            Assert.Equal(5, counts.Count);
            Assert.Equal(2, counts.Select(x => x.Item2).Sum());

            var first = counts.ElementAt(0);
            var second = counts.ElementAt(1);
            var third = counts.ElementAt(2);
            var fourth = counts.ElementAt(3);
            var fifth = counts.ElementAt(4);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)), first.Item1);
            Assert.Equal(1, first.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(15), second.Item1);
            Assert.Equal(1, second.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(30), third.Item1);
            Assert.Equal(0, third.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(45), fourth.Item1);
            Assert.Equal(0, fourth.Item2);

            Assert.Equal(this._now.AddMinutes(-(this._now.Minute % 15)).AddMinutes(60), fifth.Item1);
            Assert.Equal(0, fifth.Item2);
        }

        [Fact]
        public void GetCountsHour()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddMinutes(61));

            var counts = BitwiseAnalytics.GetCounts(
                "video",
                "watch",
                this._now,
                this._now.AddHours(1).AddMinutes(15),
                timeInterval: TimeInterval.OneHour);

            Assert.Equal(2, counts.Count);
            Assert.Equal(2, counts.Select(x => x.Item2).Sum());

            var first = counts.ElementAt(0);
            var second = counts.ElementAt(1);

            Assert.Equal(this._now.AddMinutes(-this._now.Minute), first.Item1);
            Assert.Equal(1, first.Item2);

            Assert.Equal(this._now.AddMinutes(+(60 - this._now.Minute)), second.Item1);
            Assert.Equal(1, second.Item2);
        }

        [Fact]
        public void GetCountsDay()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddHours(25));

            var counts = BitwiseAnalytics.GetCounts(
                "video",
                "watch",
                this._now,
                this._now.AddDays(1).AddMinutes(1),
                timeInterval: TimeInterval.OneDay);

            Assert.Equal(2, counts.Count);
            Assert.Equal(2, counts.Select(x => x.Item2).Sum());

            var first = counts.ElementAt(0);
            var second = counts.ElementAt(1);

            Assert.Equal(this._now.AddHours(-this._now.Hour), first.Item1);
            Assert.Equal(1, first.Item2);

            Assert.Equal(this._now.AddHours(-this._now.Hour).AddDays(1), second.Item1);
            Assert.Equal(1, second.Item2);
        }

        [Fact]
        public void GetCountsWeek()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddDays(7));

            var counts = BitwiseAnalytics.GetCounts(
                "video",
                "watch",
                this._now,
                this._now.AddDays(8),
                timeInterval: TimeInterval.Week);

            Assert.Equal(2, counts.Count);
            Assert.Equal(2, counts.Select(x => x.Item2).Sum());

            var first = counts.ElementAt(0);
            var second = counts.ElementAt(1);

            Assert.Equal(new DateTime(1999, 12, 26), first.Item1);
            Assert.Equal(1, first.Item2);

            Assert.Equal(new DateTime(2000, 1, 2), second.Item1);
            Assert.Equal(1, second.Item2);
        }

        [Fact]
        public void GetCountsWeekSingleEvent()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 3, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddDays(7));
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddDays(14));

            var counts = BitwiseAnalytics.GetCounts(
                "video",
                "watch",
                this._now,
                this._now.AddDays(14),
                1,
                TimeInterval.Week);

            Assert.Equal(3, counts.Count);
            Assert.Equal(3, counts.Select(x => x.Item2).Sum());

            var first = counts.ElementAt(0);
            var second = counts.ElementAt(1);
            var third = counts.ElementAt(2);

            Assert.Equal(new DateTime(1999, 12, 26), first.Item1);
            Assert.Equal(1, first.Item2);

            Assert.Equal(new DateTime(2000, 1, 2), second.Item1);
            Assert.Equal(1, second.Item2);

            Assert.Equal(new DateTime(2000, 1, 9), third.Item1);
            Assert.Equal(1, second.Item2);
        }

        [Fact]
        public void GetCountsWeekMondayFirst()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddDays(7));

            var counts = BitwiseAnalytics.GetCounts(
                "video",
                "watch",
                this._now,
                this._now.AddDays(8),
                timeInterval: TimeInterval.Week,
                firstDayOfWeek: DayOfWeek.Monday);

            Assert.Equal(2, counts.Count);
            Assert.Equal(2, counts.Select(x => x.Item2).Sum());

            var first = counts.ElementAt(0);
            var second = counts.ElementAt(1);

            Assert.Equal(new DateTime(1999, 12, 27), first.Item1);
            Assert.Equal(1, first.Item2);

            Assert.Equal(new DateTime(2000, 1, 3), second.Item1);
            Assert.Equal(1, second.Item2);
        }

        [Fact]
        public void GetCountsMonth()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddMonths(1));

            var counts = BitwiseAnalytics.GetCounts(
                "video",
                "watch",
                this._now,
                this._now.AddMonths(2),
                timeInterval: TimeInterval.OneMonth);

            Assert.Equal(2, counts.Count);
            Assert.Equal(2, counts.Select(x => x.Item2).Sum());

            var first = counts.ElementAt(0);
            var second = counts.ElementAt(1);

            Assert.Equal(this._now.AddDays(1 - this._now.Day), first.Item1);
            Assert.Equal(1, first.Item2);

            Assert.Equal(this._now.AddDays(1 - this._now.Day).AddMonths(1), second.Item1);
            Assert.Equal(1, second.Item2);
        }

        [Fact]
        public void GetCountsQuarter()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now.AddMonths(3));
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now.AddMonths(3).AddMinutes(1));

            var counts = BitwiseAnalytics.GetCounts(
                "video",
                "watch",
                this._now,
                this._now.AddMonths(4),
                timeInterval: TimeInterval.Quarter);

            Assert.Equal(2, counts.Count);
            Assert.Equal(3, counts.Select(x => x.Item2).Sum());

            var first = counts.ElementAt(0);
            var second = counts.ElementAt(1);

            int nowQuarter = ((this._now.Month + 2) / 3) - 1;

            Assert.Equal(new DateTime(this._now.Year, (nowQuarter * 3) + 1, 1), first.Item1);
            Assert.Equal(1, first.Item2);

            Assert.Equal(new DateTime(this._now.Year, (nowQuarter * 3) + 1, 1).AddMonths(3), second.Item1);
            Assert.Equal(2, second.Item2);
        }

        [Fact]
        public void AndCohort()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1);
            BitwiseAnalytics.TrackEvent("video", "watch", 2);
            BitwiseAnalytics.TrackEvent("video", "watch", 3);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3);

            long actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", DateTime.UtcNow, DateTime.UtcNow, TimeInterval.OneDay),
                    new Event("anything", "purchase", DateTime.UtcNow, DateTime.UtcNow, TimeInterval.OneDay)));

            Assert.Equal(2, actual);
        }

        [Fact]
        public void AndCohortMultipleFifteenMinuteBlocks()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 3, TimePrecision.FifteenMinutes, this._now.AddMinutes(15));
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3, TimePrecision.FifteenMinutes, this._now);

            long actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", this._now, this._now.AddMinutes(1), TimeInterval.FifteenMinutes),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.FifteenMinutes)));

            Assert.Equal(1, actual);

            actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", this._now, this._now.AddMinutes(16), TimeInterval.FifteenMinutes),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.FifteenMinutes)));

            Assert.Equal(2, actual);
        }

        [Fact]
        public void AndCohortAcrossHours()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 3, TimePrecision.FifteenMinutes, this._now.AddMinutes(61));
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3, TimePrecision.FifteenMinutes, this._now);

            long actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", this._now, this._now.AddMinutes(1), TimeInterval.OneHour),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.OneHour)));

            Assert.Equal(1, actual);

            actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", this._now, this._now.AddMinutes(61), TimeInterval.OneHour),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.OneHour)));

            Assert.Equal(2, actual);
        }

        [Fact]
        public void AndCohortAcrossDays()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 3, TimePrecision.FifteenMinutes, this._now.AddDays(1).AddHours(1));
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3, TimePrecision.FifteenMinutes, this._now);

            long actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", this._now, this._now.AddMinutes(1), TimeInterval.OneHour),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.OneHour)));

            Assert.Equal(1, actual);

            actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", this._now, this._now.AddDays(1).AddMinutes(1), TimeInterval.OneHour),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.OneHour)));

            Assert.Equal(1, actual);

            actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", this._now, this._now.AddDays(1).AddHours(1), TimeInterval.OneHour),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.OneHour)));

            Assert.Equal(1, actual);

            actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", this._now, this._now.AddDays(1).AddHours(1).AddMinutes(1), TimeInterval.OneHour),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.OneHour)));

            Assert.Equal(2, actual);

            actual = BitwiseAnalytics.Count(
                Ops.And(
                    new Event("video", "watch", this._now, this._now.AddMonths(1), TimeInterval.OneHour),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.OneHour)));

            Assert.Equal(2, actual);
        }

        [Fact]
        public void OrCohort()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1);
            BitwiseAnalytics.TrackEvent("video", "watch", 2);
            BitwiseAnalytics.TrackEvent("video", "watch", 3);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3);

            long actual = BitwiseAnalytics.Count(
                Ops.Or(
                    new Event("video", "watch", DateTime.UtcNow, DateTime.UtcNow, TimeInterval.OneDay),
                    new Event("anything", "purchase", DateTime.UtcNow, DateTime.UtcNow, TimeInterval.OneDay)));

            Assert.Equal(3, actual);
        }

        [Fact]
        public void OrCohortAsync()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1);
            BitwiseAnalytics.TrackEvent("video", "watch", 2);
            BitwiseAnalytics.TrackEvent("video", "watch", 3);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3);

            long actual = BitwiseAnalytics.CountAsync(
                Ops.OrAsync(
                    new Event("video", "watch", DateTime.UtcNow, DateTime.UtcNow, TimeInterval.OneDay),
                    new Event("anything", "purchase", DateTime.UtcNow, DateTime.UtcNow, TimeInterval.OneDay)).Result).Result;

            Assert.Equal(3, actual);
        }

        [Fact]
        public void OrCohortMultipleFifteenMinuteBlocks()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 3, TimePrecision.FifteenMinutes, this._now.AddMinutes(15));
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 5, TimePrecision.FifteenMinutes, this._now);

            long actual = BitwiseAnalytics.Count(
                Ops.Or(
                    new Event("video", "watch", this._now, this._now.AddMinutes(1), TimeInterval.FifteenMinutes),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.FifteenMinutes)));

            Assert.Equal(3, actual);

            actual = BitwiseAnalytics.Count(
                Ops.Or(
                    new Event("video", "watch", this._now, this._now.AddMinutes(16), TimeInterval.FifteenMinutes),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.FifteenMinutes)));

            Assert.Equal(4, actual);
        }

        [Fact]
        public void OrCohortAcrossHours()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 2, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("video", "watch", 3, TimePrecision.FifteenMinutes, this._now.AddMinutes(61));
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1, TimePrecision.FifteenMinutes, this._now);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 5, TimePrecision.FifteenMinutes, this._now);

            long actual = BitwiseAnalytics.Count(
                Ops.Or(
                    new Event("video", "watch", this._now, this._now.AddMinutes(1), TimeInterval.OneHour),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.OneHour)));

            Assert.Equal(3, actual);

            actual = BitwiseAnalytics.Count(
                Ops.Or(
                    new Event("video", "watch", this._now, this._now.AddMinutes(61), TimeInterval.OneHour),
                    new Event("anything", "purchase", this._now, this._now.AddMinutes(1), TimeInterval.OneHour)));

            Assert.Equal(4, actual);
        }

        [Fact]
        public void XOrCohort()
        {
            BitwiseAnalytics.TrackEvent("video", "watch", 1);
            BitwiseAnalytics.TrackEvent("video", "watch", 2);
            BitwiseAnalytics.TrackEvent("video", "watch", 3);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3);

            long actual = BitwiseAnalytics.Count(
                Ops.XOr(
                    new Event("video", "watch", DateTime.UtcNow, DateTime.UtcNow, TimeInterval.OneDay),
                    new Event("anything", "purchase", DateTime.UtcNow, DateTime.UtcNow, TimeInterval.OneDay)));

            // 2 is the only thing not in both
            Assert.Equal(1, actual);
        }

        [Fact]
        public void NotCohort()
        {
            BitwiseAnalytics.TrackEvent("anything", "purchase", 1);
            BitwiseAnalytics.TrackEvent("anything", "purchase", 3);

            long actual = BitwiseAnalytics.Count(
                Ops.Not(
                    new Event("anything", "purchase", DateTime.UtcNow, DateTime.UtcNow, TimeInterval.OneDay)));

            // six since a full byte is allocated, and only 2 are being set
            Assert.Equal(6, actual);
        }

        [Fact]
        public void GetFifteenMinutes()
        {
            foreach (int i in new[] { 0, 14 })
            {
                BitwiseAnalytics.TrackEvent("video", "watch", i, TimePrecision.FifteenMinutes, this._now);
            }

            long firstFifteen = BitwiseAnalytics.Count(
                this.GetConnection(), BitwiseAnalytics.GetFifteenMinutes("video", "watch", this._now));
            Assert.Equal(2, firstFifteen);
        }

        [Fact]
        public void GetHour()
        {
            foreach (int i in new[] { 0, 23, 44 })
            {
                BitwiseAnalytics.TrackEvent("video", "watch", i, TimePrecision.FifteenMinutes, this._now.AddMinutes(i));
            }

            long oneHour = BitwiseAnalytics.Count(
                this.GetConnection(),
                BitwiseAnalytics.GetHour(this.GetConnection(), "video", "watch", this._now));

            Assert.Equal(3, oneHour);
        }

        [Fact]
        public void GetDay()
        {
            foreach (int i in new[] { 0, 14, 16 })
            {
                BitwiseAnalytics.TrackEvent("video", "watch", i, TimePrecision.FifteenMinutes, this._now.AddHours(i));
            }

            long oneDay = BitwiseAnalytics.Count(
                this.GetConnection(),
                BitwiseAnalytics.GetDay(this.GetConnection(), "video", "watch", this._now));

            Assert.Equal(3, oneDay);
        }

        [Fact]
        public void GetWeek()
        {
            // week is 2nd to the 8th, so we use the 2nd, 5th, and 9th
            foreach (int i in new[] { 1, 4, 8 })
            {
                BitwiseAnalytics.TrackEvent(
                    "video", "watch", i, TimePrecision.FifteenMinutes, this._now.AddDays(i));
            }

            // adding 2 days to make sure we're in the right week
            long oneWeek = BitwiseAnalytics.Count(
                this.GetConnection(),
                BitwiseAnalytics.GetWeek(this.GetConnection(), "video", "watch", this._now.AddDays(2)));

            // only 2 days should count, since one date is in the following week
            Assert.Equal(2, oneWeek);
        }

        [Fact]
        public void GetMonth()
        {
            foreach (int i in new[] { 0, 11, 16 })
            {
                BitwiseAnalytics.TrackEvent("video", "watch", i, TimePrecision.FifteenMinutes, this._now.AddDays(i));
            }

            long oneMonth = BitwiseAnalytics.Count(
                this.GetConnection(),
                BitwiseAnalytics.GetMonth(this.GetConnection(), "video", "watch", this._now));

            Assert.Equal(3, oneMonth);
        }

        [Fact]
        public void GetQuarter()
        {
            // week is 2nd to the 8th, so we use the 2nd, 5th, and 9th
            foreach (int i in new[] { 1, 2, 4 })
            {
                BitwiseAnalytics.TrackEvent(
                    "video", "watch", i, TimePrecision.FifteenMinutes, this._now.AddMonths(i - 1));
            }

            long oneQuarter = BitwiseAnalytics.Count(
                this.GetConnection(),
                BitwiseAnalytics.GetQuarter(this.GetConnection(), "video", "watch", this._now));

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
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 0, 1, 0));

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.FifteenMinutes, "200001010000", new DateTime(2000, 1, 1)),
                            keys[0]);
                    }

                    [Fact]
                    public void BackToBackFifteenMinuteBlocks()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 0, 16, 0));

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.FifteenMinutes, "200001010000", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.FifteenMinutes, "200001010015", new DateTime(2000, 1, 1, 0, 15, 0)),
                            keys[1]);
                    }

                    [Fact]
                    public void FifteenMinuteBlocksAtBothEndRequiringFullHour()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 1, 0, 0));

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010100", new DateTime(2000, 1, 1)),
                            keys[0]);
                    }

                    [Fact]
                    public void OneHourPlusAnotherFifteenMinutes()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 1, 1, 0));

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010100", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.FifteenMinutes, "200001010100", new DateTime(2000, 1, 1, 1, 0, 0)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneHourMinusAnotherFifteenMinutes()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 59, 0),
                            new DateTime(2000, 1, 1, 2, 0, 0));

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.FifteenMinutes, "200001010045", new DateTime(2000, 1, 1, 0, 45, 0)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010101", new DateTime(2000, 1, 1, 1, 0, 0)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneHour()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 1, 0, 0),
                            new DateTime(2000, 1, 1, 2, 0, 0));

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010101", new DateTime(2000, 1, 1, 1, 0, 0)),
                            keys[0]);
                    }

                    [Fact]
                    public void ThreeFullHours()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 2, 59, 0));

                        Assert.Equal(3, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010100", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010101", new DateTime(2000, 1, 1, 1, 0, 0)),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010102", new DateTime(2000, 1, 1, 2, 0, 0)),
                            keys[2]);
                    }

                    [Fact]
                    public void OneFullDay()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 2, 0, 0, 0));

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneDay, "20000101", new DateTime(2000, 1, 1)),
                            keys[0]);
                    }

                    [Fact]
                    public void OneFullDayPlusFifteenMinutes()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 2, 0, 1, 0));

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneDay, "20000101", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.FifteenMinutes, "200001020000", new DateTime(2000, 1, 2)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneFullMonthPlusFifteenMinutes()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 2, 1, 0, 1, 0));

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.FifteenMinutes, "200002010000", new DateTime(2000, 2, 1)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneFullYearPlusFifteenMinutes()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2001, 1, 1, 0, 1, 0));

                        Assert.Equal(13, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200002", new DateTime(2000, 2, 1)),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200003", new DateTime(2000, 3, 1)),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200004", new DateTime(2000, 4, 1)),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200005", new DateTime(2000, 5, 1)),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200006", new DateTime(2000, 6, 1)),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200007", new DateTime(2000, 7, 1)),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200008", new DateTime(2000, 8, 1)),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200009", new DateTime(2000, 9, 1)),
                            keys[8]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200010", new DateTime(2000, 10, 1)),
                            keys[9]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200011", new DateTime(2000, 11, 1)),
                            keys[10]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200012", new DateTime(2000, 12, 1)),
                            keys[11]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.FifteenMinutes, "200101010000", new DateTime(2001, 1, 1)),
                            keys[12]);
                    }

                    [Fact]
                    public void OneFullYearMinusFifteenMinutes()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 12, 31, 23, 59, 0),
                            new DateTime(2002, 1, 1, 0, 0, 0));

                        Assert.Equal(13, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.FifteenMinutes, "200012312345", new DateTime(2000, 12, 31, 23, 45, 0)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200101", new DateTime(2001, 1, 1)),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200102", new DateTime(2001, 2, 1)),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200103", new DateTime(2001, 3, 1)),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200104", new DateTime(2001, 4, 1)),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200105", new DateTime(2001, 5, 1)),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200106", new DateTime(2001, 6, 1)),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200107", new DateTime(2001, 7, 1)),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200108", new DateTime(2001, 8, 1)),
                            keys[8]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200109", new DateTime(2001, 9, 1)),
                            keys[9]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200110", new DateTime(2001, 10, 1)),
                            keys[10]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200111", new DateTime(2001, 11, 1)),
                            keys[11]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200112", new DateTime(2001, 12, 1)),
                            keys[12]);
                    }
                }

                public class OneHourIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 0, 1, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010100", new DateTime(2000, 1, 1)),
                            keys[0]);
                    }

                    [Fact]
                    public void OneHourPlusOneMinuteRequiresTwoFullHours()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 1, 1, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010100", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010101", new DateTime(2000, 1, 1, 1, 0, 0)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneDayPlusOneMinute()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 2, 0, 1, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneDay, "20000101", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010200", new DateTime(2000, 1, 2)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneDayMinusOneMinute()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 23, 59, 0),
                            new DateTime(2000, 1, 3, 0, 0, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000010123", new DateTime(2000, 1, 1, 23, 45, 0)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneDay, "20000102", new DateTime(2000, 1, 2)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneMonthPlusOneMinute()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 2, 1, 0, 1, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2000020100", new DateTime(2000, 2, 1)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneYearPlusOneMinute()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2001, 1, 1, 0, 1, 0),
                            TimeInterval.OneHour);

                        Assert.Equal(13, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200002", new DateTime(2000, 2, 1)),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200003", new DateTime(2000, 3, 1)),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200004", new DateTime(2000, 4, 1)),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200005", new DateTime(2000, 5, 1)),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200006", new DateTime(2000, 6, 1)),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200007", new DateTime(2000, 7, 1)),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200008", new DateTime(2000, 8, 1)),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200009", new DateTime(2000, 9, 1)),
                            keys[8]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200010", new DateTime(2000, 10, 1)),
                            keys[9]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200011", new DateTime(2000, 11, 1)),
                            keys[10]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200012", new DateTime(2000, 12, 1)),
                            keys[11]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneHour, "2001010100", new DateTime(2001, 1, 1)),
                            keys[12]);
                    }
                }

                public class OneDayIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 0, 1, 0),
                            TimeInterval.OneDay);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneDay, "20000101", new DateTime(2000, 1, 1)),
                            keys[0]);
                    }

                    [Fact]
                    public void TimeInTwoSeparateHours()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 1, 1, 0),
                            TimeInterval.OneDay);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneDay, "20000101", new DateTime(2000, 1, 1)),
                            keys[0]);
                    }

                    [Fact]
                    public void OneDayAndOneMinute()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 2, 0, 1, 0),
                            TimeInterval.OneDay);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneDay, "20000101", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneDay, "20000102", new DateTime(2000, 1, 2)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneMonthAndOneMinute()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 2, 1, 0, 1, 0),
                            TimeInterval.OneDay);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneDay, "20000201", new DateTime(2000, 2, 1)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneYearAndOneMinute()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2001, 1, 1, 0, 1, 0),
                            TimeInterval.OneDay);

                        Assert.Equal(13, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200002", new DateTime(2000, 2, 1)),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200003", new DateTime(2000, 3, 1)),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200004", new DateTime(2000, 4, 1)),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200005", new DateTime(2000, 5, 1)),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200006", new DateTime(2000, 6, 1)),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200007", new DateTime(2000, 7, 1)),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200008", new DateTime(2000, 8, 1)),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200009", new DateTime(2000, 9, 1)),
                            keys[8]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200010", new DateTime(2000, 10, 1)),
                            keys[9]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200011", new DateTime(2000, 11, 1)),
                            keys[10]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200012", new DateTime(2000, 12, 1)),
                            keys[11]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneDay, "20010101", new DateTime(2001, 1, 1)),
                            keys[12]);
                    }
                }

                public class OneMonthIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 0, 1, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                    }

                    [Fact]
                    public void TwoHours()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 1, 1, 1, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                    }

                    [Fact]
                    public void TwoDays()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 1, 2, 0, 1, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                    }

                    [Fact]
                    public void OneMonthAndOneMinute()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2000, 2, 1, 0, 1, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200002", new DateTime(2000, 2, 1)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneMonthOddChange()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 23, 59, 0),
                            new DateTime(2000, 2, 3, 0, 0, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1, 23, 45, 0)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200002", new DateTime(2000, 2, 1, 23, 45, 0)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneMonthMinusOneMinute()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 31, 23, 59, 0),
                            new DateTime(2000, 3, 1, 0, 0, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(2, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 31, 23, 45, 0)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200002", new DateTime(2000, 2, 29, 23, 45, 0)),
                            keys[1]);
                    }

                    [Fact]
                    public void OneYearAndOneMinute()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2000, 1, 1, 0, 0, 0),
                            new DateTime(2001, 1, 1, 0, 1, 0),
                            TimeInterval.OneMonth);

                        Assert.Equal(13, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200001", new DateTime(2000, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200002", new DateTime(2000, 2, 1)),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200003", new DateTime(2000, 3, 1)),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200004", new DateTime(2000, 4, 1)),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200005", new DateTime(2000, 5, 1)),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200006", new DateTime(2000, 6, 1)),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200007", new DateTime(2000, 7, 1)),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200008", new DateTime(2000, 8, 1)),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200009", new DateTime(2000, 9, 1)),
                            keys[8]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200010", new DateTime(2000, 10, 1)),
                            keys[9]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200011", new DateTime(2000, 11, 1)),
                            keys[10]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "200012", new DateTime(2000, 12, 1)),
                            keys[11]);
                    }
                }

                public class OneWeekIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2015, 3, 1, 0, 0, 0),
                            new DateTime(2015, 3, 1, 0, 1, 0),
                            TimeInterval.Week);

                        Assert.Equal(1, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.Week, "2015W010", new DateTime(2015, 3, 1)),
                            keys[0]);
                    }

                    [Fact]
                    public void ThreeWeeks()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2015, 3, 1, 0, 0, 0),
                            new DateTime(2015, 3, 15, 0, 1, 0),
                            TimeInterval.Week);

                        Assert.Equal(3, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.Week, "2015W010", new DateTime(2015, 3, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.Week, "2015W011", new DateTime(2015, 3, 8)),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.Week, "2015W012", new DateTime(2015, 3, 15)),
                            keys[2]);
                    }
                }

                public class OneQuarterIntervals
                {
                    [Fact]
                    public void SingleFifteenMinuteBlock()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2015, 3, 1, 0, 0, 0),
                            new DateTime(2015, 3, 1, 0, 1, 0),
                            TimeInterval.Quarter);

                        Assert.Equal(3, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201501", new DateTime(2015, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201502", new DateTime(2015, 2, 1)),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201503", new DateTime(2015, 3, 1)),
                            keys[2]);
                    }

                    [Fact]
                    public void ThreeQuarters()
                    {
                        Tuple<TimeInterval, string, DateTime>[] keys = BitwiseAnalytics.DateTimeUtil.MinKeysForRange(
                            new DateTime(2015, 3, 1, 0, 0, 0),
                            new DateTime(2015, 9, 1, 0, 1, 0),
                            TimeInterval.Quarter);

                        Assert.Equal(9, keys.Length);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201501", new DateTime(2015, 1, 1)),
                            keys[0]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201502", new DateTime(2015, 2, 1)),
                            keys[1]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201503", new DateTime(2015, 3, 1)),
                            keys[2]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201504", new DateTime(2015, 4, 1)),
                            keys[3]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201505", new DateTime(2015, 5, 1)),
                            keys[4]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201506", new DateTime(2015, 6, 1)),
                            keys[5]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201507", new DateTime(2015, 7, 1)),
                            keys[6]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201508", new DateTime(2015, 8, 1)),
                            keys[7]);
                        Assert.Equal(
                            new Tuple<TimeInterval, string, DateTime>(
                                TimeInterval.OneMonth, "201509", new DateTime(2015, 9, 1)),
                            keys[8]);
                    }
                }
            }
        }
    }
}