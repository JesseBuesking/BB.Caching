namespace BB.Caching.Redis.Analytics
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using StackExchange.Redis;

    /// <summary>
    /// An instance of an event. This is used when querying for event data.
    /// </summary>
    public struct Event
    {
        /// <summary>
        /// Typically the object that was interacted with (e.g. button)
        /// </summary>
        private string _category;

        /// <summary>
        /// The type of interaction (e.g. click)
        /// </summary>
        private string _action;

        /// <summary>
        /// The inclusive start of the period that we're interested in.
        /// </summary>
        private DateTime _from;

        /// <summary>
        /// The exclusive end of the period that we're interested in.
        /// </summary>
        private DateTime _to;

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
        public Event(string category, string action, DateTime from, DateTime to)
        {
            this._category = category;
            this._action = action;
            this._from = from;
            this._to = to;
            this._redisKeys = null;
        }

        /// <summary>
        /// All of the keys that would be involved in the period of time specified for this event.
        /// </summary>
        public RedisKey[] RedisKeys
        {
            get
            {
                if (this._redisKeys == null)
                {
                    // perform logic to get all redis keys involved in this event
                }

                return this._redisKeys;
            }
        }

//        public static void Blah(DateTime from, DateTime to)
//        {
//            if (from > to)
//            {
//                throw new Exception(string.Format(
//                    "expecting from to be <= to\n\tfrom: {0}\n\tto: {1}", from , to));
//            }
//
//            TimeSpan difference = to - from;
//            if difference
//        }

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
