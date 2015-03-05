namespace BB.Caching.Redis
{
    using System;
    using System.Globalization;
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
        /// The precision with which to store the data.
        /// </summary>
        public enum TimePrecision
        {
            /// <summary>
            /// Accurate up to 15 minutes.
            /// </summary>
            FifteenMinutes,

            /// <summary>
            /// Accurate up to 1 hour.
            /// </summary>
            OneHour,

            /// <summary>
            /// Accurate up to 1 day.
            /// </summary>
            OneDay,

            /// <summary>
            /// Accurate up to 1 month.
            /// </summary>
            OneMonth
        }

        /// <summary>
        /// The time interval in which the data is returned.
        /// </summary>
        public enum TimeInterval
        {
            /// <summary>
            /// Data is grouped into 15 minute intervals.
            /// </summary>
            FifteenMinutes,

            /// <summary>
            /// Data is grouped into 30 minute intervals.
            /// </summary>
            ThirtyMinutes,

            /// <summary>
            /// Data is grouped into 1 hour intervals.
            /// </summary>
            OneHour,

            /// <summary>
            /// Data is grouped into 6 hour intervals.
            /// </summary>
            SixHours,

            /// <summary>
            /// Data is grouped into 1 day intervals.
            /// </summary>
            OneDay,

            /// <summary>
            /// Data is grouped into 1 month intervals.
            /// </summary>
            OneMonth,

            /// <summary>
            /// Data is grouped into 1 week intervals.
            /// </summary>
            Week,

            /// <summary>
            /// Data is grouped into 1 quarter intervals.
            /// </summary>
            Quarter,

            /// <summary>
            /// Data is grouped into 1 year intervals.
            /// </summary>
            Year
        }

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
        public static void TrackEvent(
            string category, string action, long eventId, TimePrecision precision = TimePrecision.OneDay)
        {
            string time;
            switch (precision)
            {
                case TimePrecision.FifteenMinutes:
                {
                    time = DateTimeUtil.FifteenMinutes(DateTime.UtcNow);
                    break;
                }

                case TimePrecision.OneHour:
                {
                    time = DateTimeUtil.OneHour(DateTime.UtcNow);
                    break;
                }

                case TimePrecision.OneDay:
                {
                    time = DateTimeUtil.OneDay(DateTime.UtcNow);
                    break;
                }

                case TimePrecision.OneMonth:
                {
                    time = DateTimeUtil.OneMonth(DateTime.UtcNow);
                    break;
                }

                default:
                {
                    throw new Exception(string.Format("unsupported precision encountered\n\t{0}", precision));
                }
            }

            RedisKey key = string.Format("{0}:{1}:{2}", category, action, time);

            SharedCache.Instance.GetAnalyticsWriteConnection()
                .GetDatabase(SharedCache.Instance.Db)
                .StringSetBit(key, eventId, true);
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
        }
    }
}
