namespace BB.Caching.Redis.Analytics
{
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
        /// Data is grouped into 1 hour intervals.
        /// </summary>
        OneHour,

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
}