namespace BB.Caching.Redis.Analytics
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
}
