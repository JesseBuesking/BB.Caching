namespace BB.Caching.Redis.Analytics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
            RedisKey tmpKey = string.Format("tmp:{0}:{1}", DateTime.UtcNow.Ticks, Base64(events));
            BitwiseAnalytics.BitwiseAnd(
                SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db),
                tmpKey,
                events.SelectMany(e => e.RedisKeys).ToArray());
            return tmpKey;
        }

        /// <summary>
        /// Gets the Base64 representation of a collection of events.
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        /// <returns>
        /// A Base64 string.
        /// </returns>
        private static string Base64(IEnumerable<Event> events)
        {
            var combine = string.Join(string.Empty, events);
            var bytes = System.Text.Encoding.UTF8.GetBytes(combine);
            return Convert.ToBase64String(bytes);
        }
    }
}
