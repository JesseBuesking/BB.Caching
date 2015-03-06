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
        FifteenMinutes = 0,

        /// <summary>
        /// Data is grouped into 1 hour intervals.
        /// </summary>
        OneHour = 1,

        /// <summary>
        /// Data is grouped into 1 day intervals.
        /// </summary>
        OneDay = 2,

        /// <summary>
        /// Data is grouped into 1 week intervals.
        /// </summary>
        Week = 3,

        /// <summary>
        /// Data is grouped into 1 month intervals.
        /// </summary>
        OneMonth = 4,

        /// <summary>
        /// Data is grouped into 1 quarter intervals.
        /// </summary>
        Quarter = 5
    }
}