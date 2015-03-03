namespace BB.Caching
{
    /// <summary>
    /// With the AGGREGATE option, it is possible to specify how the results of the union are aggregated. This option
    /// defaults to SUM, where the score of an element is summed across the inputs where it exists. When this option is
    /// set to either MIN or MAX, the resulting set will contain the minimum or maximum score of an element across the
    /// inputs where it exists.
    /// TODO remove this (use a tool to find unused things)
    /// </summary>
    public enum CacheAggregate
    {
        /// <summary>
        /// The score of an element is summed across the inputs where it exists
        /// </summary>
        Sum,

        /// <summary>
        /// The resulting set will contain the minimum score of an element across the inputs where it exists
        /// </summary>
        Min,

        /// <summary>
        /// The resulting set will contain the maximum score of an element across the inputs where it exists
        /// </summary>
        Max
    }
}