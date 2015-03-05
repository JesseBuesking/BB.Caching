namespace BB.Caching.Redis.Analytics
{
    using System;

    using StackExchange.Redis;

    /// <summary>
    /// An instance of an event. This is used when querying for event data.
    /// </summary>
    public struct Event
    {
        /// <summary>
        /// Typically the object that was interacted with (e.g. button)
        /// </summary>
        private readonly string _category;

        /// <summary>
        /// The type of interaction (e.g. click)
        /// </summary>
        private readonly string _action;

        /// <summary>
        /// The inclusive start of the period that we're interested in.
        /// </summary>
        private readonly DateTime _from;

        /// <summary>
        /// The exclusive end of the period that we're interested in.
        /// </summary>
        private readonly DateTime _to;

        /// <summary>
        /// The accuracy at which we want the data. For example, setting this to TimeInterval.OneDay means there won't
        /// be any keys at the fifteen minute or one hour levels, so if the _from DateTime is for the middle of a day,
        /// it'll include the entire day.
        /// </summary>
        private readonly TimeInterval _timeInterval;

        /// <summary>
        /// The redis keys.
        /// </summary>
        private RedisKey[] _redisKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> struct.
        /// </summary>
        /// <param name="category">
        /// Typically the object that was interacted with (e.g. button)
        /// </param>
        /// <param name="action">
        /// The type of interaction (e.g. click)
        /// </param>
        /// <param name="from">
        /// The inclusive start of the period that we're interested in.
        /// </param>
        /// <param name="to">
        /// The exclusive end of the period that we're interested in.
        /// </param>
        /// <param name="timeInterval">
        /// The accuracy at which we want the data. For example, setting this to TimeInterval.OneDay means there won't
        /// be any keys at the fifteen minute or one hour levels, so if the <paramref name="from"/> DateTime is for the
        /// middle of a day, it'll include the entire day.
        /// </param>
        public Event(
            string category,
            string action,
            DateTime from,
            DateTime to,
            TimeInterval timeInterval = TimeInterval.FifteenMinutes)
        {
            this._category = category;
            this._action = action;
            this._from = from;
            this._to = to;
            this._redisKeys = null;
            this._timeInterval = timeInterval;
        }

        /// <summary>
        /// All of the keys that would be involved in the period of time specified for this event.
        /// </summary>
        public RedisKey[] RedisKeys
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (this._redisKeys == null)
                {
                    this._redisKeys = BitwiseAnalytics.GetMinKeysForRange(
                        SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db),
                        this._category,
                        this._action,
                        this._from,
                        this._to,
                        this._timeInterval);
                }

                return this._redisKeys;
            }
        }

        /// <summary>
        /// Logic to convert this Event instance into a string.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        /// <returns>
        /// The current event instance as a string.
        /// </returns>
        public static implicit operator string(Event @event)
        {
            return string.Join(string.Empty, @event.RedisKeys);
        }

        /// <summary>
        /// Logic to convert this Event instance into a Base64 string.
        /// </summary>
        /// <returns>
        /// The current event instance as a Base64 string.
        /// </returns>
        public string ToBase64()
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(this);
            return Convert.ToBase64String(bytes);
        }
    }
}
