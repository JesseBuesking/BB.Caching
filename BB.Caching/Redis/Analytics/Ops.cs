namespace BB.Caching.Redis.Analytics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using StackExchange.Redis;

    /// <summary>
    /// The operations to use when querying for event data.
    /// </summary>
    public static class Ops
    {
        /// <summary>
        /// Combines events by finding their logical intersection.
        /// </summary>
        /// <param name="events">
        /// The events to combine.
        /// </param>
        /// <returns>
        /// A <see cref="RedisKey"/> where the results have been stored.
        /// </returns>
        public static RedisKey And(params Event[] events)
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            // get the keys for the events (need to group subsets using TemporarilyOrKeys)
            var keys = events.Select(@event => TemporarilyOrKeys(database, @event.RedisKeys())).ToList();

            RedisKey tmpKey = TempKey(events);
            BitwiseAnalytics.BitwiseAnd(database, tmpKey, keys.ToArray(), TimeSpan.FromHours(1));
            return tmpKey;
        }

        /// <summary>
        /// Combines events by finding their logical intersection.
        /// </summary>
        /// <param name="events">
        /// The events to combine.
        /// </param>
        /// <returns>
        /// A <see cref="RedisKey"/> where the results have been stored.
        /// </returns>
        public static async Task<RedisKey> AndAsync(params Event[] events)
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            // get the keys for the events (need to group subsets using TemporarilyOrKeys)
            var taskKeys = events.Select(
                async @event => await TemporarilyOrKeysAsync(database, await @event.RedisKeysAsync())).ToList();

            RedisKey[] keys = await Task.WhenAll(taskKeys);

            RedisKey tmpKey = TempKey(events);
            await BitwiseAnalytics.BitwiseAndAsync(database, tmpKey, keys.ToArray(), TimeSpan.FromHours(1));
            return tmpKey;
        }

        /// <summary>
        /// Combines events by finding their logical union.
        /// </summary>
        /// <param name="events">
        /// The events to combine.
        /// </param>
        /// <returns>
        /// A <see cref="RedisKey"/> where the results have been stored.
        /// </returns>
        public static RedisKey Or(params Event[] events)
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            // get the keys for the events (no need to group subsets since the full thing will be OR'd)
            var keys = events.SelectMany(@event => @event.RedisKeys()).ToArray();

            RedisKey tmpKey = TempKey(events);
            BitwiseAnalytics.BitwiseOr(database, tmpKey, keys, TimeSpan.FromHours(1));
            return tmpKey;
        }

        /// <summary>
        /// Combines events by finding their logical union.
        /// </summary>
        /// <param name="events">
        /// The events to combine.
        /// </param>
        /// <returns>
        /// A <see cref="RedisKey"/> where the results have been stored.
        /// </returns>
        public static async Task<RedisKey> OrAsync(params Event[] events)
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            // get the keys for the events (no need to group subsets since the full thing will be OR'd)
            var taskKeys = events.Select(@event => @event.RedisKeysAsync());

            var tempKeys = await Task.WhenAll(taskKeys);

            var keys = tempKeys.SelectMany(x => x).ToArray();

            RedisKey tmpKey = TempKey(events);
            await BitwiseAnalytics.BitwiseOrAsync(database, tmpKey, keys, TimeSpan.FromHours(1));
            return tmpKey;
        }

        /// <summary>
        /// Combines events by finding their logical exclusive OR.
        /// </summary>
        /// <param name="events">
        /// The events to combine.
        /// </param>
        /// <returns>
        /// A <see cref="RedisKey"/> where the results have been stored.
        /// </returns>
        public static RedisKey XOr(params Event[] events)
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            // get the keys for the events (need to group subsets using TemporarilyOrKeys)
            var keys = events.Select(@event => TemporarilyOrKeys(database, @event.RedisKeys())).ToList();

            RedisKey tmpKey = TempKey(events);
            BitwiseAnalytics.BitwiseXOr(database, tmpKey, keys.ToArray(), TimeSpan.FromHours(1));
            return tmpKey;
        }

        /// <summary>
        /// Combines events by finding their logical exclusive OR.
        /// </summary>
        /// <param name="events">
        /// The events to combine.
        /// </param>
        /// <returns>
        /// A <see cref="RedisKey"/> where the results have been stored.
        /// </returns>
        public static async Task<RedisKey> XOrAsync(params Event[] events)
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            // get the keys for the events (need to group subsets using TemporarilyOrKeys)
            var taskKeys = events.Select(
                async @event => await TemporarilyOrKeysAsync(database, await @event.RedisKeysAsync())).ToList();

            RedisKey[] keys = await Task.WhenAll(taskKeys);

            RedisKey tmpKey = TempKey(events);
            await BitwiseAnalytics.BitwiseXOrAsync(database, tmpKey, keys.ToArray(), TimeSpan.FromHours(1));
            return tmpKey;
        }

        /// <summary>
        /// Gets the logical negation of an event.
        /// </summary>
        /// <param name="event">
        /// The event to negate.
        /// </param>
        /// <returns>
        /// A <see cref="RedisKey"/> where the results have been stored.
        /// </returns>
        public static RedisKey Not(Event @event)
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            // get the keys for the event (need to group subsets using TemporarilyOrKeys)
            var key = TemporarilyOrKeys(database, @event.RedisKeys());

            RedisKey tmpKey = TempKey(@event);
            BitwiseAnalytics.BitwiseNot(database, tmpKey, key, TimeSpan.FromHours(1));
            return tmpKey;
        }

        /// <summary>
        /// Gets the logical negation of an event.
        /// </summary>
        /// <param name="event">
        /// The event to negate.
        /// </param>
        /// <returns>
        /// A <see cref="RedisKey"/> where the results have been stored.
        /// </returns>
        public static async Task<RedisKey> NotAsync(Event @event)
        {
            var database = SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);

            // get the keys for the event (need to group subsets using TemporarilyOrKeys)
            var key = await TemporarilyOrKeysAsync(database, @event.RedisKeys());

            RedisKey tmpKey = TempKey(@event);
            await BitwiseAnalytics.BitwiseNotAsync(database, tmpKey, key, TimeSpan.FromHours(1));
            return tmpKey;
        }

        /// <summary>
        /// Logically ORs the keys supplied and returns the location of the value if more than one key is supplied,
        /// otherwise it immediately returns the key.
        /// </summary>
        /// <param name="database">
        /// The database.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <returns>
        /// The <see cref="RedisKey"/>.
        /// </returns>
        private static RedisKey TemporarilyOrKeys(IDatabase database, RedisKey[] keys)
        {
            if (keys.Length == 1)
            {
                return keys[0];
            }

            var newKey = TempKey(keys);
            BitwiseAnalytics.BitwiseOr(database, newKey, keys, TimeSpan.FromHours(1));

            return newKey;
        }

        /// <summary>
        /// Logically ORs the keys supplied and returns the location of the value if more than one key is supplied,
        /// otherwise it immediately returns the key.
        /// </summary>
        /// <param name="database">
        /// The database.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <returns>
        /// The <see cref="RedisKey"/>.
        /// </returns>
        private static async Task<RedisKey> TemporarilyOrKeysAsync(IDatabase database, RedisKey[] keys)
        {
            if (keys.Length == 1)
            {
                return keys[0];
            }

            var newKey = TempKey(keys);
            await BitwiseAnalytics.BitwiseOrAsync(database, newKey, keys, TimeSpan.FromHours(1));

            return newKey;
        }

        /// <summary>
        /// Creates a new temporary key.
        /// </summary>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <returns>
        /// The temporary <see cref="RedisKey"/>.
        /// </returns>
        private static RedisKey TempKey(params object[] values)
        {
            return string.Format("tmp:{0}:{1}", DateTime.UtcNow.Ticks, Base64(values));
        }

        /// <summary>
        /// Gets the Base64 representation of a collection of values.
        /// </summary>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <returns>
        /// A Base64 string.
        /// </returns>
        private static string Base64(IEnumerable<object> values)
        {
            var combine = string.Join(string.Empty, values);
            var bytes = System.Text.Encoding.UTF8.GetBytes(combine);
            return Convert.ToBase64String(bytes);
        }
    }
}
