namespace BB.Caching.Caching
{
    /// <summary>
    /// Represents values that can be stored in memory.
    /// </summary>
    public struct MemoryValue<TObject>
    {
        /// <summary>
        /// The actual value stored in memory.
        /// </summary>
        public TObject Value
        {
            get { return this._value; }
        }

        private readonly TObject _value;

        /// <summary>
        /// Indicates whether the key existed.
        /// </summary>
        public bool Exists
        {
            get { return this._exists; }
        }

        private readonly bool _exists;

        public static MemoryValue<TObject> Null = new MemoryValue<TObject>(default(TObject), false); 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">the value stored in memory</param>
        /// <param name="exists">indicates whether the key existed</param>
        public MemoryValue(TObject value, bool exists)
        {
            this._value = value;
            this._exists = exists;
        }
    }
}
