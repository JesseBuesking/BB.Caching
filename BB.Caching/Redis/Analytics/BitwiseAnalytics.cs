namespace BB.Caching.Redis.Analytics
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using StackExchange.Redis;

    /// <summary>
    /// Methods for performing bitwise analytics.
    /// <para>
    /// Note: bitwise operations use byte-sized chunks, so setting the first bit to a value will allocate a full byte
    /// worth of data.
    /// </para>
    /// <remarks>
    /// Check out the following links:
    /// http://blog.getspool.com/2011/11/29/fast-easy-realtime-metrics-using-redis-bitmaps
    /// http://amix.dk/blog/post/19714
    /// </remarks>
    /// </summary>
    public static class BitwiseAnalytics
    {
        /// <summary>
        /// Tracks an event.
        /// </summary>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="eventId">
        /// The id of the thing being interacted with.
        /// </param>
        /// <param name="precision">
        /// The precision that this event should be tracked at.
        /// </param>
        /// <param name="now">
        /// The now.
        /// </param>
        public static void TrackEvent(
            string category,
            string action,
            long eventId,
            TimePrecision precision = TimePrecision.OneDay,
            DateTime now = default(DateTime))
        {
            if (now == default(DateTime))
            {
                now = DateTime.UtcNow;
            }

            string time;
            switch (precision)
            {
                case TimePrecision.FifteenMinutes:
                {
                    time = DateTimeUtil.FifteenMinutes(now);
                    break;
                }

                case TimePrecision.OneHour:
                {
                    time = DateTimeUtil.OneHour(now);
                    break;
                }

                case TimePrecision.OneDay:
                {
                    time = DateTimeUtil.OneDay(now);
                    break;
                }

                case TimePrecision.OneMonth:
                {
                    time = DateTimeUtil.OneMonth(now);
                    break;
                }

                default:
                {
                    throw new Exception(string.Format("unsupported precision encountered\n\t{0}", precision));
                }
            }

            RedisKey key = EventKey(category, action, time);

            SharedCache.Instance.GetAnalyticsWriteConnection()
                .GetDatabase(SharedCache.Instance.Db)
                .StringSetBit(key, eventId, true);
        }

        /// <summary>
        /// Gets the count of active bits at the key supplied.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// A long representing the count of bits that are set.
        /// </returns>
        public static long Count(RedisKey key)
        {
            return Count(SharedCache.Instance.GetAnalyticsReadConnection().GetDatabase(SharedCache.Instance.Db), key);
        }

        /// <summary>
        /// Gets the count of active bits at the key supplied.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// A long representing the count of bits that are set.
        /// </returns>
        public static long Count(IDatabase database, RedisKey key)
        {
            return database.StringBitCount(key, 0, -1);
        }

        /// <summary>
        /// Determines if the key exists.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// True if the key exists.
        /// </returns>
        public static bool Exists(RedisKey key)
        {
            return Exists(SharedCache.Instance.GetAnalyticsReadConnection().GetDatabase(SharedCache.Instance.Db), key);
        }

        /// <summary>
        /// Determines if the key exists.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// True if the key exists.
        /// </returns>
        public static bool Exists(IDatabase database, RedisKey key)
        {
            return database.KeyExists(key);
        }

        /// <summary>
        /// Gets the key for the 15 minute interval covered by the DateTime supplied.
        /// </summary>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="dateTime">
        /// The DateTime.
        /// </param>
        /// <returns>
        /// The key for the 15 minute interval.
        /// </returns>
        public static RedisKey GetFifteenMinutes(string category, string action, DateTime dateTime)
        {
            // get the key
            string fifteenMinutes = BitwiseAnalytics.DateTimeUtil.FifteenMinutes(dateTime);
            RedisKey key = EventKey(category, action, fifteenMinutes);
            return key;
        }

        /// <summary>
        /// Gets the key for the hour covered by the DateTime supplied, creating the data at the key if necessary.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="dateTime">
        /// The DateTime.
        /// </param>
        /// <returns>
        /// The key for the hour.
        /// </returns>
        public static RedisKey GetHour(IDatabase database, string category, string action, DateTime dateTime)
        {
            // get the key
            string hour = BitwiseAnalytics.DateTimeUtil.OneHour(dateTime);
            RedisKey key = EventKey(category, action, hour);

            // return it if there's already data for this hour
            bool hourExists = BitwiseAnalytics.Exists(database, key);
            if (hourExists)
            {
                return key;
            }

            // no data for the hour, so we need to create it from the 15 minute intervals
            string[] fifteenMinutesInHour = BitwiseAnalytics.DateTimeUtil.FifteenMinutesInHour(dateTime);
            BitwiseAnalytics.BitwiseOr(
                database,
                key,
                fifteenMinutesInHour.Select(x => EventKey(category, action, x)).ToArray());

            return key;
        }

        /// <summary>
        /// Gets the key for the day covered by the DateTime supplied, creating the data at the key if necessary.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="dateTime">
        /// The DateTime.
        /// </param>
        /// <returns>
        /// The key for the day.
        /// </returns>
        public static RedisKey GetDay(IDatabase database, string category, string action, DateTime dateTime)
        {
            // get the key
            string day = BitwiseAnalytics.DateTimeUtil.OneDay(dateTime);
            RedisKey key = EventKey(category, action, day);

            // return it if there's already data for this day
            bool dayExists = BitwiseAnalytics.Exists(database, key);
            if (dayExists)
            {
                return key;
            }

            // no data for the day, so we need to create it from the hours
            string[] hoursInDay = BitwiseAnalytics.DateTimeUtil.HoursInDay(dateTime);

            // make sure each hour exists
            foreach (string hour in hoursInDay)
            {
                GetHour(database, category, action, DateTime.ParseExact(hour, "yyyyMMddHH", CultureInfo.InvariantCulture));
            }

            // combine the hours to form one day
            BitwiseAnalytics.BitwiseOr(
                database,
                key,
                hoursInDay.Select(x => EventKey(category, action, x)).ToArray());

            return key;
        }

        /// <summary>
        /// The get week.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="dateTime">
        /// The DateTime.
        /// </param>
        /// <param name="weekFirstDay">
        /// The first day to start each week. Defaults to Sunday which is used in the US, CA, and JP. You can
        /// change it to Monday to get weekly aggregates which are accurate for other countries, but it'll double
        /// the weekly data stored.
        /// </param>
        /// <returns>
        /// The key for the day.
        /// </returns>
        public static RedisKey GetWeek(
            IDatabase database,
            string category,
            string action,
            DateTime dateTime,
            DayOfWeek weekFirstDay = DayOfWeek.Sunday)
        {
            // get the key
            string week = BitwiseAnalytics.DateTimeUtil.WeekNumber(dateTime, weekFirstDay);
            RedisKey key = EventKey(category, action, week);

            // return it if there's already data for this day
            bool weekExists = BitwiseAnalytics.Exists(database, key);
            if (weekExists)
            {
                return key;
            }

            // no data for the week, so we need to create it from the days
            string[] daysInWeek = BitwiseAnalytics.DateTimeUtil.DaysInWeek(dateTime, weekFirstDay);

            // make sure each day exists
            foreach (string day in daysInWeek)
            {
                GetDay(database, category, action, DateTime.ParseExact(day, "yyyyMMdd", CultureInfo.InvariantCulture));
            }

            // combine the days to form one week
            BitwiseAnalytics.BitwiseOr(
                database,
                key,
                daysInWeek.Select(x => EventKey(category, action, x)).ToArray());

            return key;
        }

        /// <summary>
        /// Gets the key for the month covered by the DateTime supplied, creating the data at the key if necessary.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="dateTime">
        /// The DateTime.
        /// </param>
        /// <returns>
        /// The key for the month.
        /// </returns>
        public static RedisKey GetMonth(IDatabase database, string category, string action, DateTime dateTime)
        {
            // get the key
            string month = BitwiseAnalytics.DateTimeUtil.OneMonth(dateTime);
            RedisKey key = EventKey(category, action, month);

            // return it if there's already data for this month
            bool monthExists = BitwiseAnalytics.Exists(database, key);
            if (monthExists)
            {
                return month;
            }

            // no data for the month, so we need to create it from the days
            string[] daysInMonth = BitwiseAnalytics.DateTimeUtil.DaysInMonth(dateTime);

            // make sure each day exists
            foreach (string day in daysInMonth)
            {
                GetDay(database, category, action, DateTime.ParseExact(day, "yyyyMMdd", CultureInfo.InvariantCulture));
            }

            // combine the days to form one month
            BitwiseAnalytics.BitwiseOr(
                database,
                key,
                daysInMonth.Select(x => EventKey(category, action, x)).ToArray());

            return key;
        }

        /// <summary>
        /// Gets the keys for the months in the quarter covered by the DateTime supplied, creating the data at the key
        /// if necessary.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="dateTime">
        /// The DateTime.
        /// </param>
        /// <returns>
        /// The key for the quarter.
        /// </returns>
        public static RedisKey GetQuarter(IDatabase database, string category, string action, DateTime dateTime)
        {
            // get the key
            string quarter = BitwiseAnalytics.DateTimeUtil.QuarterNumber(dateTime);
            RedisKey key = EventKey(category, action, quarter);

            // return it if there's already data for this quarter
            bool quarterExists = BitwiseAnalytics.Exists(database, key);
            if (quarterExists)
            {
                return key;
            }

            // no data for the month, so we need to create it from the days
            string[] monthsInQuarter = BitwiseAnalytics.DateTimeUtil.MonthsInQuarter(dateTime);

            // make sure each day exists
            foreach (string month in monthsInQuarter)
            {
                GetMonth(database, category, action, DateTime.ParseExact(month, "yyyyMM", CultureInfo.InvariantCulture));
            }

            // combine the days to form one month
            BitwiseAnalytics.BitwiseOr(
                database,
                key,
                monthsInQuarter.Select(x => EventKey(category, action, x)).ToArray());

            return key;
        }

        /// <summary>
        /// Gets the fewest number of keys required to cover the date range supplied.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="from">
        /// The starting DateTime, inclusive.
        /// </param>
        /// <param name="to">
        /// The ending DateTime, exclusive.
        /// </param>
        /// <param name="timeInterval">
        /// The accuracy at which we want the data. For example, setting this to TimeInterval.OneDay means there won't
        /// be any keys at the fifteen minute or one hour levels, so if the <paramref name="from"/> DateTime is for the
        /// middle of a day, it'll include the entire day.
        /// </param>
        /// <returns>
        /// The smallest set of <see><cref>RedisKey[]</cref></see> that cover the range supplied.
        /// </returns>
        public static RedisKey[] GetMinKeysForRange(
            IDatabase database,
            string category,
            string action,
            DateTime from,
            DateTime to,
            TimeInterval timeInterval = TimeInterval.FifteenMinutes)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="dateTime">
        /// The DateTime.
        /// </param>
        /// <returns>
        /// True if there was a key that was deleted.
        /// </returns>
        public static bool Delete(string category, string action, string dateTime)
        {
            RedisKey key = EventKey(category, action, dateTime);
            return SharedCache.Instance.GetAnalyticsWriteConnection()
                .GetDatabase(SharedCache.Instance.Db)
                .KeyDelete(key);
        }

        /// <summary>
        /// Perform a bitwise AND operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be AND'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static long BitwiseAnd(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            long result = database
                .StringBitOperation(Bitwise.And, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise AND operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be AND'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static Task<long> BitwiseAndAsync(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            Task<long> result = database
                .StringBitOperationAsync(Bitwise.And, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise OR operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be OR'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static long BitwiseOr(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            long result = database
                .StringBitOperation(Bitwise.Or, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise OR operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be OR'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static Task<long> BitwiseOrAsync(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            Task<long> result = database
                .StringBitOperationAsync(Bitwise.Or, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise XOR operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be XOR'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static long BitwiseXOr(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            long result = database
                .StringBitOperation(Bitwise.Xor, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise XOR operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be XOR'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static Task<long> BitwiseXOrAsync(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            Task<long> result = database
                .StringBitOperationAsync(Bitwise.Xor, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise NOT operation on a key (containing a string value) and store the result in the destination
        /// key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="key">
        /// The key where the data to be NOT'd is located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static long BitwiseNot(IDatabase database, RedisKey destination, RedisKey key)
        {
            long result = database
                .StringBitOperation(Bitwise.Not, destination, key);

            return result;
        }

        /// <summary>
        /// Perform a bitwise NOT operation on a key (containing a string value) and store the result in the destination
        /// key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="key">
        /// The key where the data to be NOT'd is located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static Task<long> BitwiseNotAsync(IDatabase database, RedisKey destination, RedisKey key)
        {
            Task<long> result = database
                .StringBitOperationAsync(Bitwise.Not, destination, key);

            return result;
        }

        /// <summary>
        /// The event key.
        /// </summary>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="dateTime">
        /// The DateTime as a formatted string.
        /// </param>
        /// <returns>
        /// The <see cref="RedisKey"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RedisKey EventKey(string category, string action, string dateTime)
        {
            RedisKey key = string.Format("{0}:{1}:{2}", category, action, dateTime);
            return key;
        }

        /// <summary>
        /// Utility methods for datetime string formatting.
        /// </summary>
        internal static class DateTimeUtil
        {
            /// <summary>
            /// Returns the datetime supplied rounded to the closest previous 15 minute interval as yyyyMMddHHmm.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <returns>
            /// The formatted <see cref="string"/>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string FifteenMinutes(DateTime dateTime)
            {
                DateTime tmp = dateTime.AddMinutes(-(dateTime.Minute % 15));
                string formatted = string.Format("{0:yyyyMMddHHmm}", tmp);
                return formatted;
            }

            /// <summary>
            /// Gets all 15 minute intervals in the hour as yyyyMMddHHmm.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <returns>
            /// A <see><cref>string[]</cref></see> containing all the 15 minute intervals in the hour.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string[] FifteenMinutesInHour(DateTime dateTime)
            {
                DateTime tmp = dateTime.AddMinutes(-dateTime.Minute);
                return new[]
                           {
                               string.Format("{0:yyyyMMddHHmm}", tmp),
                               string.Format("{0:yyyyMMddHHmm}", tmp.AddMinutes(15)),
                               string.Format("{0:yyyyMMddHHmm}", tmp.AddMinutes(30)),
                               string.Format("{0:yyyyMMddHHmm}", tmp.AddMinutes(45))
                           };
            }

            /// <summary>
            /// Returns the datetime supplied at the hour interval as yyyyMMddhh.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <returns>
            /// The formatted <see cref="string"/>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string OneHour(DateTime dateTime)
            {
                string formatted = string.Format("{0:yyyyMMddHH}", dateTime);
                return formatted;
            }

            /// <summary>
            /// Gets all 1 hour intervals in the day as yyyyMMddHH.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <returns>
            /// A <see><cref>string[]</cref></see> containing all the 15 minute intervals in the hour.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string[] HoursInDay(DateTime dateTime)
            {
                DateTime tmp = dateTime.AddHours(-dateTime.Hour);
                return new[]
                           {
                               string.Format("{0:yyyyMMddHH}", tmp),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(1)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(2)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(3)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(4)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(5)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(6)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(7)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(8)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(9)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(10)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(11)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(12)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(13)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(14)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(15)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(16)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(17)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(18)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(19)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(20)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(21)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(22)),
                               string.Format("{0:yyyyMMddHH}", tmp.AddHours(23))
                           };
            }

            /// <summary>
            /// Returns the datetime supplied at the day interval as yyyyMMdd.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <returns>
            /// The formatted <see cref="string"/>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string OneDay(DateTime dateTime)
            {
                string formatted = string.Format("{0:yyyyMMdd}", dateTime);
                return formatted;
            }

            /// <summary>
            /// Gets all 1 day intervals in the month as yyyyMMdd.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <returns>
            /// A <see><cref>string[]</cref></see> containing all the days in the month.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string[] DaysInMonth(DateTime dateTime)
            {
                return
                    Enumerable.Range(1, DateTime.DaysInMonth(dateTime.Year, dateTime.Month))
                        .Select(
                            day =>
                            string.Format(
                                "{0}{1}{2}",
                                dateTime.Year.ToString("D4"),
                                dateTime.Month.ToString("D2"),
                                day.ToString("D2")))
                        .ToArray();
            }

            /// <summary>
            /// Returns the datetime supplied at the month interval as yyyyMM.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <returns>
            /// The formatted <see cref="string"/>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string OneMonth(DateTime dateTime)
            {
                string formatted = string.Format("{0:yyyyMM}", dateTime);
                return formatted;
            }

            /// <summary>
            /// Returns the datetime supplied as the week number as yyyyW##.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <param name="weekFirstDay">
            /// The first day to start each week. Defaults to Sunday which is used in the US, CA, and JP. You can
            /// change it to Monday to get weekly aggregates which are accurate for other countries, but it'll double
            /// the weekly data stored.
            /// </param>
            /// <returns>
            /// The formatted <see cref="string"/>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string WeekNumber(DateTime dateTime, DayOfWeek weekFirstDay = DayOfWeek.Sunday)
            {
                string weekFormat;
                if (weekFirstDay == DayOfWeek.Sunday)
                {
                    weekFormat = "0";
                }
                else if (weekFirstDay == DayOfWeek.Monday)
                {
                    weekFormat = "1";
                }
                else
                {
                    throw new Exception(string.Format("invalid weekday supplied {0}", weekFirstDay));
                }

                // ReSharper disable once PossibleNullReferenceException
                int weekNumber = DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(
                    dateTime,
                    CalendarWeekRule.FirstDay,
                    weekFirstDay);

                string formatted = string.Format("{0:yyyy}W{1}{2}", dateTime, weekFormat, weekNumber.ToString("D2"));
                return formatted;
            }

            /// <summary>
            /// Gets all 1 day intervals in the week as yyyyMMdd.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <param name="weekFirstDay">
            /// The first day to start each week. Defaults to Sunday which is used in the US, CA, and JP. You can
            /// change it to Monday to get weekly aggregates which are accurate for other countries, but it'll double
            /// the weekly data stored.
            /// </param>
            /// <returns>
            /// A <see><cref>string[]</cref></see> containing all the days in the week.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string[] DaysInWeek(DateTime dateTime, DayOfWeek weekFirstDay = DayOfWeek.Sunday)
            {
                int difference = dateTime.DayOfWeek - weekFirstDay;
                if (difference < 0)
                {
                    difference += 7;
                }

                var tmp = dateTime.AddDays(-difference);
                return new[]
                           {
                               string.Format("{0:yyyyMMdd}", tmp),
                               string.Format("{0:yyyyMMdd}", tmp.AddDays(1)),
                               string.Format("{0:yyyyMMdd}", tmp.AddDays(2)),
                               string.Format("{0:yyyyMMdd}", tmp.AddDays(3)),
                               string.Format("{0:yyyyMMdd}", tmp.AddDays(4)),
                               string.Format("{0:yyyyMMdd}", tmp.AddDays(5)),
                               string.Format("{0:yyyyMMdd}", tmp.AddDays(6))
                           };
            }

            /// <summary>
            /// Returns the datetime supplied as the quarter number as yyyyQ#.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <returns>
            /// The formatted <see cref="string"/>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string QuarterNumber(DateTime dateTime)
            {
                int quarter = (dateTime.Month + 2) / 3;
                string formatted = string.Format("{0:yyyy}Q{1}", dateTime, quarter);
                return formatted;
            }

            /// <summary>
            /// Gets all months in the quarter as yyyyMM.
            /// </summary>
            /// <param name="dateTime">
            /// The DateTime.
            /// </param>
            /// <returns>
            /// The formatted <see cref="string"/>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string[] MonthsInQuarter(DateTime dateTime)
            {
                int quarter = (dateTime.Month + 2) / 3;
                DateTime tmp = dateTime.AddMonths(-(dateTime.Month - 1));
                tmp = tmp.AddMonths((3 * quarter) - 3);

                return new[]
                           {
                               string.Format("{0:yyyyMM}", tmp),
                               string.Format("{0:yyyyMM}", tmp.AddMonths(1)),
                               string.Format("{0:yyyyMM}", tmp.AddMonths(2))
                           };
            }

            /// <summary>
            /// Finds the minimum subset of keys required to cover the range of time specified by
            /// <paramref name="start"/> and <paramref name="end"/> using the <see cref="TimeInterval"/> specified.
            /// </summary>
            /// <param name="start">
            /// The start of the time range.
            /// </param>
            /// <param name="end">
            /// The end of the time range.
            /// </param>
            /// <param name="timeInterval">
            /// The time interval.
            /// </param>
            /// <returns>
            /// The minimum subset of keys required.
            /// </returns>
            /// <exception cref="Exception">
            /// This exception should never be triggered unless a new TimeInterval is supported.
            /// </exception>
            public static Tuple<TimeInterval, string>[] MinKeysForRange(
                DateTime start, DateTime end, TimeInterval timeInterval = TimeInterval.FifteenMinutes)
            {
                if (start > end)
                {
                    throw new Exception(
                        string.Format("expecting dateTime < end\n\tstart: {0}\n\tend: {1}", start, end));
                }

                var result = new List<Tuple<TimeInterval, string>>();

                // round to the closest 15 minute mark
                start = start.AddMinutes(-(start.Minute % 15));
                var roundEnd = end.AddMinutes(-(end.Minute % 15));

                while (true)
                {
                    TimeSpan difference = end - start;

                    // not super accurate, but a decent approximation
                    if (difference.TotalDays >= 365)
                    {
                        // exactly one entire year
                        if (start == new DateTime(start.Year, 1, 1) && start.AddYears(1) == roundEnd)
                        {
                            for (int month = 0; month < 12; ++month)
                            {
                                result.Add(new Tuple<TimeInterval, string>(
                                    TimeInterval.OneMonth, DateTimeUtil.OneMonth(start.AddMonths(month))));
                            }

                            start = start.AddYears(1);

                            if (start == end)
                            {
                                break;
                            }
                        }
                        else
                        {
                            DateTime cutoff = new DateTime(start.Year + 1, 1, 1);
                            result.AddRange(MinKeysForRange(start, cutoff, timeInterval));
                            result.AddRange(MinKeysForRange(cutoff, end, timeInterval));
                            break;
                        }
                    }
                    else if (timeInterval == TimeInterval.Quarter)
                    {
                        while (start <= roundEnd)
                        {
                            result.AddRange(DateTimeUtil.MonthsInQuarter(start)
                                .Select(x => new Tuple<TimeInterval, string>(TimeInterval.OneMonth, x)));
                            start = start.AddMonths(3);
                        }

                        break;
                    }
                    else if (timeInterval == TimeInterval.OneMonth)
                    {
                        while (start <= roundEnd)
                        {
                            result.Add(new Tuple<TimeInterval, string>(
                                TimeInterval.OneMonth, DateTimeUtil.OneMonth(start)));
                            start = start.AddMonths(1);
                        }

                        break;
                    }
                    else if (difference.TotalDays >= 28)
                    {
                        // exactly one entire month
                        if (start == new DateTime(start.Year, start.Month, 1) && start.AddMonths(1) == roundEnd)
                        {
                            result.Add(new Tuple<TimeInterval, string>(
                                TimeInterval.OneMonth, DateTimeUtil.OneMonth(start)));
                            start = start.AddMonths(1);

                            if (start == end)
                            {
                                break;
                            }
                        }
                        else
                        {
                            DateTime cutoff = new DateTime(start.Year, start.Month + 1, 1);
                            result.AddRange(MinKeysForRange(start, cutoff, timeInterval));
                            result.AddRange(MinKeysForRange(cutoff, end, timeInterval));
                            break;
                        }
                    }
                    else if (timeInterval == TimeInterval.Week)
                    {
                        while (start <= roundEnd)
                        {
                            result.Add(new Tuple<TimeInterval, string>(
                                TimeInterval.Week, DateTimeUtil.WeekNumber(start)));
                            start = start.AddDays(7);
                        }

                        break;
                    }
                    else if (difference.TotalDays >= 1)
                    {
                        // exactly one entire day
                        if (start == new DateTime(start.Year, start.Month, start.Day)
                            && start.AddDays(1) == roundEnd)
                        {
                            result.Add(new Tuple<TimeInterval, string>(
                                TimeInterval.OneDay, DateTimeUtil.OneDay(start)));
                            start = start.AddDays(1);

                            if (start == end)
                            {
                                break;
                            }
                        }
                        else
                        {
                            DateTime cutoff = new DateTime(start.Year, start.Month, start.Day + 1);
                            result.AddRange(MinKeysForRange(start, cutoff, timeInterval));
                            result.AddRange(MinKeysForRange(cutoff, end, timeInterval));
                            break;
                        }
                    }
                    else if (timeInterval == TimeInterval.OneDay)
                    {
                        result.Add(new Tuple<TimeInterval, string>(TimeInterval.OneDay, DateTimeUtil.OneDay(start)));
                        break;
                    }
                    else if (difference.TotalHours >= 1)
                    {
                        // exactly one entire hour
                        if (start == new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0)
                            && start.AddHours(1) == roundEnd)
                        {
                            result.Add(new Tuple<TimeInterval, string>(
                                TimeInterval.OneHour, DateTimeUtil.OneHour(start)));
                            start = start.AddHours(1);

                            if (start == end)
                            {
                                break;
                            }
                        }
                        else
                        {
                            DateTime cutoff = new DateTime(start.Year, start.Month, start.Day, start.Hour + 1, 0, 0);
                            result.AddRange(MinKeysForRange(start, cutoff, timeInterval));
                            result.AddRange(MinKeysForRange(cutoff, end, timeInterval));
                            break;
                        }
                    }
                    else if (timeInterval == TimeInterval.OneHour)
                    {
                        result.Add(new Tuple<TimeInterval, string>(TimeInterval.OneHour, DateTimeUtil.OneHour(start)));
                        break;
                    }
                    else if (difference.TotalMinutes >= 15)
                    {
                        if (start == new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0) && difference.TotalMinutes >= 45)
                        {
                            result.Add(new Tuple<TimeInterval, string>(
                                TimeInterval.OneHour, DateTimeUtil.OneHour(start)));
                        }
                        else
                        {
                            do
                            {
                                result.Add(
                                    new Tuple<TimeInterval, string>(
                                        TimeInterval.FifteenMinutes,
                                        DateTimeUtil.FifteenMinutes(start)));
                                start = start.AddMinutes(15);
                            }
                            while (start < end);
                        }

                        break;
                    }
                    else if (difference.TotalMinutes == 0)
                    {
                        break;
                    }
                    else if (difference.TotalMinutes < 15)
                    {
                        result.Add(new Tuple<TimeInterval, string>(
                            TimeInterval.FifteenMinutes, DateTimeUtil.FifteenMinutes(start)));
                        break;
                    }
                    else
                    {
                        throw new Exception(
                            string.Format(
                                "case not handled '{0}'\n\tstart: {1}\n\tend: {2}",
                                difference,
                                start,
                            end));
                    }
                }

                return result.ToArray();
            }
        }
    }
}
