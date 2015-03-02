namespace BB.Caching.Caching
{
    /// <summary>
    /// Represents values that can be stored in memory.
    /// </summary>
    /// <typeparam name="TObject">
    /// The type of the underlying value.
    /// </typeparam>
    public struct MemoryValue<TObject>
    {
        /// <summary>
        /// Default null instance.
        /// </summary>
        public static MemoryValue<TObject> Null = new MemoryValue<TObject>(default(TObject), false);

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryValue{TObject}"/> struct.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="exists">
        /// Whether the key existed.
        /// </param>
        public MemoryValue(TObject value, bool exists)
            : this()
        {
            this.Value = value;
            this.Exists = exists;
        }

        /// <summary>
        /// The actual value stored in memory.
        /// </summary>
        public TObject Value { get; private set; }

        /// <summary>
        /// Indicates whether the key existed.
        /// </summary>
        public bool Exists { get; private set; }
    }
}
