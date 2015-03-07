namespace BB.Caching.Redis.Analytics
{
    using System.Collections;
    using System.Collections.Generic;

    using StackExchange.Redis;

    /// <summary>
    /// A custom iterator to get the active ids from a redis key.
    /// </summary>
    public class RedisKeyBitEnumerable : IEnumerable<long>
    {
        /// <summary>
        /// The redis key.
        /// </summary>
        private readonly RedisKey _key;

        /// <summary>
        /// The database connection.
        /// </summary>
        private IDatabase _database;

        /// <summary>
        /// The property where the data is stored.
        /// </summary>
        private byte[] _storedData;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisKeyBitEnumerable"/> class. 
        /// </summary>
        /// <param name="key">
        /// The key where the data is stored in Redis.
        /// </param>
        /// <param name="database">
        /// An optional database connection, otherwise this will default to the write connection.
        /// </param>
        public RedisKeyBitEnumerable(RedisKey key, IDatabase database = null)
        {
            this._key = key;
            this._database = database;
            this.Position = -1;
        }

        /// <summary>
        /// The current index position into the data.
        /// </summary>
        public long Position { get; private set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        private byte[] Data
        {
            get
            {
                if (this._storedData != null)
                {
                    return this._storedData;
                }

                if (this._database == null)
                {
                    this._database = 
                        SharedCache.Instance.GetAnalyticsWriteConnection().GetDatabase(SharedCache.Instance.Db);
                }

                this._storedData = this._database.StringGet(this._key);

                return this._storedData;
            }
        }

        /// <summary>
        /// Gets the enumerator for this enumerable.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator<long> GetEnumerator()
        {
            if (this.Position < 0)
            {
                this.Position = 0;
            }

            for (; this.Position < this.Data.Length; ++this.Position)
            {
                byte current = this.Data[this.Position];

                for (int bitPosition = 7; bitPosition >= 0; --bitPosition)
                {
                    if ((current & (1 << bitPosition)) != 0)
                    {
                        yield return (this.Position * 8) + (7 - bitPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
