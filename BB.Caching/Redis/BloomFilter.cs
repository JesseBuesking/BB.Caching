namespace BB.Caching.Redis
{
    using System;
    using System.Threading.Tasks;

    using BB.Caching.Redis.Lua;

    using StackExchange.Redis;

    /// <summary>
    /// Bloom filter class using redis.
    /// </summary>
    public class BloomFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BloomFilter"/> class.
        /// </summary>
        /// <param name="numberOfItems">
        /// How many items that are expected to be stored in the bloom filter.
        /// </param>
        /// <param name="probFalsePos">
        /// The probability [0, 1] that there will be false positives.
        /// </param>
        public BloomFilter(long numberOfItems = 1000000, float probFalsePos = 0.001f)
        {
            var allScript = ScriptLoader.Instance["AllBitsSet"];
            var multipleScript = ScriptLoader.Instance["SetMultipleBits"];

            this.Options = new BFOptions(numberOfItems, probFalsePos);
            var connections = SharedCache.Instance.GetAllWriteConnections();
            foreach (var connection in connections)
            {
                foreach (var endpoint in connection.GetEndPoints())
                {
                    BloomFilter.AllBitsSetHash = connection.GetServer(endpoint).ScriptLoad(allScript);
                    BloomFilter.SetMultipleBitsHash = connection.GetServer(endpoint).ScriptLoad(multipleScript);
                }
            }
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        internal BFOptions Options
        {
            get;
            private set;
        }

        /// <summary>
        /// SHA hash for the AllBitsSet lua script.
        /// </summary>
        private static byte[] AllBitsSetHash { get; set; }

        /// <summary>
        /// SHA hash for the SetMultipleBits lua script.
        /// </summary>
        private static byte[] SetMultipleBitsHash { get; set; }

        /// <summary>
        /// Adds a value to the bloom filter.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void Add(string key, string value)
        {
            var bits = new RedisValue[this.Options.NumberOfHashes];
            for (int i = 0; i < this.Options.NumberOfHashes; i++)
            {
                bits[i] = Hashing.Murmur3.ComputeInt(value + i) % this.Options.NumberOfBits;
            }

            SetBits(key, bits, true);
        }

        /// <summary>
        /// Adds a value to the bloom filter.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task AddAsync(string key, string value)
        {
            var bits = new RedisValue[this.Options.NumberOfHashes];
            for (int i = 0; i < this.Options.NumberOfHashes; i++)
            {
                bits[i] = Hashing.Murmur3.ComputeInt(value + i) % this.Options.NumberOfBits;
            }

            await SetBitsAsync(key, bits, true);
        }

        /// <summary>
        /// Determines if the value is set in the bloom filter.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsSet(string key, string value)
        {
            var bits = new RedisValue[this.Options.NumberOfHashes];
            for (int i = 0; i < this.Options.NumberOfHashes; i++)
            {
                bits[i] = Hashing.Murmur3.ComputeInt(value + i) % this.Options.NumberOfBits;
            }

            return AllBitsSet(key, bits);
        }

        /// <summary>
        /// Determines if the value is set in the bloom filter.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<bool> IsSetAsync(string key, string value)
        {
            var bits = new RedisValue[this.Options.NumberOfHashes];
            for (int i = 0; i < this.Options.NumberOfHashes; i++)
            {
                bits[i] = Hashing.Murmur3.ComputeInt(value + i) % this.Options.NumberOfBits;
            }

            return await AllBitsSetAsync(key, bits);
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Options.ToString();
        }

        /// <summary>
        /// Sets bits in the bloom filter.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="bits">
        /// The bits.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        private static void SetBits(string key, RedisValue[] bits, bool value)
        {
            RedisKey[] keyArgs = { key };
            RedisValue[] valueArgs = new RedisValue[bits.Length + 1];
            valueArgs[0] = value;
            bits.CopyTo(valueArgs, 1);

            SharedCache.Instance.GetWriteConnection(key)
                .GetDatabase(SharedCache.Instance.Db)
                .ScriptEvaluate(BloomFilter.SetMultipleBitsHash, keyArgs, valueArgs);
        }

        /// <summary>
        /// Sets bits in the bloom filter.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="bits">
        /// The bits.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private static Task SetBitsAsync(string key, RedisValue[] bits, bool value)
        {
            RedisKey[] keyArgs = { key };
            RedisValue[] valueArgs = new RedisValue[bits.Length + 1];
            valueArgs[0] = value;
            bits.CopyTo(valueArgs, 1);

            return SharedCache.Instance.GetWriteConnection(key)
                .GetDatabase(SharedCache.Instance.Db)
                .ScriptEvaluateAsync(BloomFilter.SetMultipleBitsHash, keyArgs, valueArgs);
        }

        /// <summary>
        /// Determines if all the bits are set in the bloom filter.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="bits">
        /// The bits.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool AllBitsSet(string key, RedisValue[] bits)
        {
            RedisKey[] keyArgs = { key };
            RedisValue[] valueArgs = new RedisValue[bits.Length];
            bits.CopyTo(valueArgs, 0);

            RedisResult result = SharedCache.Instance.GetWriteConnection(key)
                .GetDatabase(SharedCache.Instance.Db)
                .ScriptEvaluate(BloomFilter.AllBitsSetHash, keyArgs, valueArgs);

            return null != result && !result.IsNull && 1L == (long)result;
        }

        /// <summary>
        /// Determines if all the bits are set in the bloom filter.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="bits">
        /// The bits.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private static async Task<bool> AllBitsSetAsync(string key, RedisValue[] bits)
        {
            RedisKey[] keyArgs = { key };
            RedisValue[] valueArgs = new RedisValue[bits.Length];
            bits.CopyTo(valueArgs, 0);

            Task<RedisResult> result = SharedCache.Instance.GetWriteConnection(key)
                .GetDatabase(SharedCache.Instance.Db)
                .ScriptEvaluateAsync(BloomFilter.AllBitsSetHash, keyArgs, valueArgs);

            if (null == result)
            {
                return false;
            }

            RedisResult value = await result;
            return !value.IsNull && 1L == (long)value;
        }

        /// <summary>
        /// The bloom filter options.
        /// </summary>
        internal class BFOptions
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BFOptions"/> class.
            /// </summary>
            /// <param name="numberOfItems">
            /// The number of items.
            /// </param>
            /// <param name="probabilityOfFalsePositives">
            /// The probability of false positives.
            /// </param>
            public BFOptions(long numberOfItems, float probabilityOfFalsePositives)
            {
                this.NumberOfItems = numberOfItems;
                this.ProbabilityOfFalsePositives = probabilityOfFalsePositives;

                double numberOfBits = Math.Ceiling(
                    (numberOfItems * Math.Log(probabilityOfFalsePositives)) /
                        Math.Log(1.0 / Math.Pow(2.0, Math.Log(2.0))));

                this.NumberOfBits = (long)numberOfBits;

                double numberOfHashes = Math.Round(Math.Log(2.0) * numberOfBits / numberOfItems);

                this.NumberOfHashes = (int)numberOfHashes;
            }

            /// <summary>
            /// Gets the number of bits.
            /// </summary>
            public long NumberOfBits
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the number of hashes.
            /// </summary>
            public int NumberOfHashes
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the number of items.
            /// </summary>
            internal long NumberOfItems
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets or sets the probability of false positives.
            /// </summary>
            private float ProbabilityOfFalsePositives
            {
                get;
                set;
            }

            /// <summary>
            /// The to string.
            /// </summary>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            public override string ToString()
            {
                return string.Format(
                    "{0} hashes, {1:#,##0}KB, {2:#0.##%} fp",
                    this.NumberOfHashes,
                    (this.NumberOfBits / 8) / 1024,
                    this.ProbabilityOfFalsePositives);
            }
        }
    }
}