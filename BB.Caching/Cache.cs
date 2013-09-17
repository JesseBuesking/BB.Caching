using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BB.Caching.Hashing;
using BB.Caching.Shared;
using BB.Caching.Utilities;

namespace BB.Caching
{
    /// <summary>
    /// Contains access to all available caching mechanisms. (In-memory, Shared)
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// A cache that is shared across all servers.
        /// </summary>
        public static SharedCache Shared
        {
            get { return SharedCache.Instance; }
        }

        /// <summary>
        /// An in-memory cache specific to the current machine.
        /// </summary>
        public static InMemoryCache Memory
        {
            get { return InMemoryCache.Instance; }
        }

        public static class Config
        {
            /// <summary>
            /// The channel used to publish and subscribe to configuration removal notifications.
            /// </summary>
            private const string _cacheConfigRemovedChannel = "cache/config-remove";

            /// <summary>
            /// The channel used to publish and subscribe to configuration change notifications.
            /// </summary>
            private const string _cacheConfigChangeChannel = "cache/config-change";

            private static readonly HashSet<string> _alreadyRemoved = new HashSet<string>();

            private const string _keyPrefix = "bb.cache.config";

            public static void Prepare()
            {
                Config.SetupSubscribeRemoval();
            }

            public static TType Get<TType>(string key)
            {
                TType value;
                if (Cache.Memory.TryGetDecompact(_keyPrefix + key, out value))
                    return value;

                var byteArrayWrapper = Cache.Shared.Hashes.GetByteArray(Config._keyPrefix, key);
                if (byteArrayWrapper.IsNil)
                    return default(TType);

                byte[] bytes = byteArrayWrapper.Value;
                value = Cache.Compaction.Decompact<TType>(bytes);
                return value;
            }

            public static Task<TType> GetAsync<TType>(string key)
            {
                var decompactedWrapper = Cache.Memory.TryGetDecompactAsync<TType>(_keyPrefix + key);
                if (!decompactedWrapper.IsNil)
                    return decompactedWrapper.ValueAsync;

                var result = Task.Run(async () =>
                    {
                        var byteArrayWrapper = Cache.Shared.Hashes.GetByteArray(Config._keyPrefix, key);
                        if (byteArrayWrapper.IsNil)
                            return default(TType);

                        byte[] byteArray = await byteArrayWrapper.ValueAsync;
                        TType value = await Cache.Compaction.DecompactAsync<TType>(byteArray);
                        return value;
                    });
                return result;
            }

            public static void Set<TType>(string key, TType value, bool broadcast = true)
            {
                byte[] compact = Cache.Memory.SetCompact(_keyPrefix + key, value);
                Cache.Shared.Hashes.Set(_keyPrefix, key, compact).Wait();
                if (broadcast)
                    Config.PublishChange(key).Wait();
            }

            public static Task SetAsync<TType>(string key, TType value, bool broadcast = true)
            {
                return Task.Run(async () =>
                    {
                        var compacted = await Cache.Memory.SetCompactAsync(_keyPrefix + key, value);
                        await Cache.Shared.Hashes.Set(_keyPrefix, key, compacted);
                        if (broadcast)
                            await Config.PublishChange(key);
                    });
            }

            public static void Remove(string key, bool broadcast = true)
            {
                Task.Run(async () =>
                    {
                        await Cache.Shared.Keys.Invalidate(_keyPrefix + key);
                        await Cache.Shared.Hashes.Remove(_keyPrefix, key);
                        if (broadcast)
                            await Config.PublishRemoval(key);
                    }).Wait();
            }

            public static Task RemoveAsync(string key, bool broadcast = true)
            {
                return Task.Run(async () =>
                    {
                        await Cache.Shared.Keys.Invalidate(_keyPrefix + key);
                        await Cache.Shared.Hashes.Remove(_keyPrefix, key);
                        if (broadcast)
                            await Config.PublishRemoval(key);
                    });
            }

            public static Task SubscribeChange(string configKey, Action subscriptionCallback)
            {
                return SharedCache.Instance.RedisChannelSubscribe(Config._cacheConfigChangeChannel, (channel, data) =>
                    {
                        string key = Encoding.UTF8.GetString(data);
                        if (key == configKey)
                            subscriptionCallback();
                    });
            }

            private static void SetupSubscribeRemoval()
            {
                SharedCache.Instance.RedisChannelSubscribe(Config._cacheConfigRemovedChannel, (channel, data) =>
                    {
                        string key = Encoding.UTF8.GetString(data);
                        if (Config._alreadyRemoved.Contains(key))
                            Config._alreadyRemoved.Remove(key);
                        else
                            Config.Remove(key, false);
                    });
            }

            private static Task PublishRemoval(string configKey)
            {
                Config._alreadyRemoved.Add(configKey);
                return SharedCache.Instance.RedisChannelPublish(Config._cacheConfigRemovedChannel, configKey);
            }

            private static Task PublishChange(string configKey)
            {
                return SharedCache.Instance.RedisChannelPublish(Config._cacheConfigChangeChannel, configKey);
            }
        }

        public static class Compression
        {
            public static Task<byte[]> CompressAsync(byte[] value)
            {
                return Compressor.Instance.CompressAsync(value);
            }

            public static Task<byte[]> CompressAsync(string value)
            {
                return Compressor.Instance.CompressAsync(value);
            }

            public static Task<string> DecompressStringAsync(byte[] value)
            {
                return Compressor.Instance.DecompressStringAsync(value);
            }

            public static Task<byte[]> DecompressByteArrayAsync(byte[] value)
            {
                return Compressor.Instance.DecompressAsync(value);
            }

            public static byte[] Compress(byte[] value)
            {
                return Compressor.Instance.Compress(value);
            }

            public static byte[] Compress(string value)
            {
                return Compressor.Instance.Compress(value);
            }

            public static string DecompressString(byte[] value)
            {
                return Compressor.Instance.DecompressString(value);
            }

            public static byte[] DecompressByteArray(byte[] value)
            {
                return Compressor.Instance.Decompress(value);
            }
        }

        public static class Compaction
        {
            public static async Task<byte[]> CompactAsync<TObject>(TObject value)
            {
                byte[] serialize = Serializer.Instance.Serialize(value);
                byte[] compressed = await Compressor.Instance.CompressAsync(serialize);
                return compressed;
            }

            public static async Task<TObject> DecompactAsync<TObject>(byte[] value)
            {
                byte[] decompressed = await Compressor.Instance.DecompressAsync(value);
                TObject deserialized = Serializer.Instance.Deserialize<TObject>(decompressed);
                return deserialized;
            }

            public static byte[] Compact<TObject>(TObject value)
            {
                byte[] serialize = Serializer.Instance.Serialize(value);
                byte[] compressed = Compressor.Instance.Compress(serialize);
                return compressed;
            }

            public static TObject Decompact<TObject>(byte[] value)
            {
                byte[] decompressed = Compressor.Instance.Decompress(value);
                TObject deserialized = Serializer.Instance.Deserialize<TObject>(decompressed);
                return deserialized;
            }
        }

        public static class Serialization
        {
            public static byte[] Serialize<TObject>(TObject value)
            {
                byte[] serialized = Serializer.Instance.Serialize(value);
                return serialized;
            }

            public static TObject Deserialize<TObject>(byte[] value)
            {
                TObject deserialized = Serializer.Instance.Deserialize<TObject>(value);
                return deserialized;
            }
        }

        public static class Statistic
        {
            public static void Prepare()
            {
                var connections = SharedCache.Instance.GetAllWriteConnections();
                foreach (var connection in connections)
                    connection.Scripting.Prepare(new[]
                        {
                            Statistic.SetStatisticScript,
                            Statistic.GetStatisticScript
                        });
            }

            private static string SetStatisticScript
            {
                get
                {
                    return Lua.Instance["SetStatistic"];
                }
            }

            private static string GetStatisticScript
            {
                get
                {
                    return Lua.Instance["GetStatistic"];
                }
            }

            public class Stat
            {
                public long NumberOfValues
                {
                    get;
                    private set;
                }

                private readonly double _sumOfValues;

                private readonly double _sumOfValuesSquared;

                public double MinimumValue
                {
                    get;
                    private set;
                }

                public double MaximumValue
                {
                    get;
                    private set;
                }

                public double Mean
                {
                    get
                    {
                        if (0 >= this.NumberOfValues)
                            return 0.0f;
                        return this._sumOfValues/this.NumberOfValues;
                    }
                }

                public double PopulationVariance
                {
                    get
                    {
                        if (1 >= this.NumberOfValues)
                            return 0.0;
                        return (this._sumOfValuesSquared - (this._sumOfValues*this.Mean))/(this.NumberOfValues - 1.0);
                    }
                }

                public double PopulationStandardDeviation
                {
                    get { return Math.Sqrt(this.PopulationVariance); }
                }

                public double Variance
                {
                    get
                    {
                        if (1 >= this.NumberOfValues)
                            return 0.0;
                        return (this._sumOfValuesSquared - (this._sumOfValues*this.Mean))/(this.NumberOfValues);
                    }
                }

                public double StandardDeviation
                {
                    get { return Math.Sqrt(this.Variance); }
                }

                public Stat(long numberOfValues, double sumOfValues, double sumOfValuesSquared, double minimum,
                    double maximum)
                {
                    this.NumberOfValues = numberOfValues;
                    this._sumOfValues = sumOfValues;
                    this._sumOfValuesSquared = sumOfValuesSquared;
                    this.MinimumValue = minimum;
                    this.MaximumValue = maximum;
                }

                public override string ToString()
                {
                    return
                        string.Format(
                            "MaximumValue: {0}, Mean: {1}, MinimumValue: {2}, NumberOfValues: {3}, PopulationStandardDeviation: {4}, PopulationVariance: {5}, StandardDeviation: {6}, Variance: {7}, SumOfValues: {8}, SumOfValuesSquared: {9}",
                            this.MaximumValue, this.Mean, this.MinimumValue, this.NumberOfValues,
                            this.PopulationStandardDeviation, this.PopulationVariance, this.StandardDeviation, this.Variance,
                            this._sumOfValues, this._sumOfValuesSquared);
                }
            }

            public static Task SetStatistic(string key, double value)
            {
                string[] keyArgs = new[] {key};
                object[] valueArgs = new object[] {value};

                var connections = SharedCache.Instance.GetWriteConnections(key);
                foreach (var connection in connections)
                {
                    var task = connection.Scripting.Eval(SharedCache.Instance.Db, Statistic.SetStatisticScript,
                        keyArgs, valueArgs, true, false, SharedCache.Instance.QueueJump);
                }

                return Task.FromResult(false);
            }

            public static Task<Stat> GetStatistic(string key)
            {
                string[] keyArgs = new[] {key};

                var connections = SharedCache.Instance.GetWriteConnections(key);
                Task<object> result = null;
                foreach (var connection in connections)
                {
                    var task = connection.Scripting.Eval(SharedCache.Instance.Db, Statistic.GetStatisticScript,
                        keyArgs, new object[0], true, false, SharedCache.Instance.QueueJump);
                    if (null == result)
                        result = task;
                }

                return Task.Run(async () =>
                    {
                        if (null == result)
                            return null;

                        object[] res = (object[]) await result;
                        long numberOfValues;
                        byte[] bytes = res[0] as byte[];
                        if (null != bytes)
                            numberOfValues = long.Parse(Encoding.UTF8.GetString(bytes));
                        else
                            numberOfValues = (long) res[0];

                        double sumOfValues;
                        bytes = res[1] as byte[];
                        if (null != bytes)
                            sumOfValues = double.Parse(Encoding.UTF8.GetString(bytes));
                        else
                            sumOfValues = res[1] is double ? (double) res[1] : (long) res[1];

                        double sumOfValuesSquared;
                        bytes = res[2] as byte[];
                        if (null != bytes)
                            sumOfValuesSquared = double.Parse(Encoding.UTF8.GetString((byte[]) res[2]));
                        else
                            sumOfValuesSquared = res[2] is double ? (double) res[2] : (long) res[2];

                        double minimum;
                        bytes = res[3] as byte[];
                        if (null != bytes)
                            minimum = double.Parse(Encoding.UTF8.GetString((byte[]) res[3]));
                        else
                            minimum = res[3] is double ? (double) res[3] : (long) res[3];

                        double maximum;
                        bytes = res[4] as byte[];
                        if (null != bytes)
                            maximum = double.Parse(Encoding.UTF8.GetString((byte[]) res[4]));
                        else
                            maximum = res[4] is double ? (double) res[4] : (long) res[4];

                        return new Stat(numberOfValues, sumOfValues, sumOfValuesSquared, minimum, maximum);
                    });
            }
        }

        public static class RateLimiter
        {
            public static void Prepare()
            {
                var connections = SharedCache.Instance.GetAllWriteConnections();
                foreach (var connection in connections)
                    connection.Scripting.Prepare(new[] {RateLimiter.RateLimitIncrementScript});
            }

            private static string RateLimitIncrementScript
            {
                get
                {
                    return Lua.Instance["RateLimitIncrement"];
                }
            }

            public static Task<object> Increment(string key, TimeSpan spanSize, TimeSpan bucketSize, long throttle,
                int increment = 1)
            {
                string[] keyArgs = new[] {key};
                object[] valueArgs = new object[]
                    {
                        DateTime.UtcNow.Ticks/TimeSpan.TicksPerMillisecond,
                        (long) spanSize.TotalMilliseconds,
                        (long) bucketSize.TotalMilliseconds,
                        increment,
                        throttle
                    };

                var connections = SharedCache.Instance.GetWriteConnections(key);
                Task<object> result = null;
                foreach (var connection in connections)
                {
                    var task = connection.Scripting.Eval(SharedCache.Instance.Db, RateLimiter.RateLimitIncrementScript,
                        keyArgs, valueArgs, true, false, SharedCache.Instance.QueueJump);
                    if (null == result)
                        result = task;
                }
                return result;
            }
        }

        public class BloomFilter
        {
            public class Options
            {
                public long NumberOfBits
                {
                    get;
                    set;
                }

                public int NumberOfHashes
                {
                    get;
                    set;
                }

                public long NumberOfItems
                {
                    get;
                    set;
                }

                public float ProbabilityOfFalsePositives
                {
                    get;
                    set;
                }

                public Options(long numberOfItems, float probabilityOfFalsePositives)
                {
                    this.NumberOfItems = numberOfItems;
                    this.ProbabilityOfFalsePositives = probabilityOfFalsePositives;

                    double numberOfBits = Math.Ceiling(
                        (numberOfItems*Math.Log(probabilityOfFalsePositives))/
                        Math.Log(1.0/(Math.Pow(2.0, Math.Log(2.0)))));

                    this.NumberOfBits = (long)numberOfBits;

                    double numberOfHashes = Math.Round(Math.Log(2.0)*numberOfBits/numberOfItems);

                    this.NumberOfHashes = (int) numberOfHashes;
                }

                public override string ToString()
                {
                    return string.Format("{0} hashes, {1:#,##0}KB, {2:#0.##%} fp", this.NumberOfHashes,
                        (this.NumberOfBits/8)/1024, this.ProbabilityOfFalsePositives);
                }
            }

            public Options Option
            {
                get;
                set;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="numberOfItems">How many items that are expected to be stored in the bloom filter.</param>
            /// <param name="probFalsePos">The probability [0, 1] that there will be false positives.</param>
            public BloomFilter(long numberOfItems = 1000000, float probFalsePos = 0.001f)
            {
                this.Option = new Options(numberOfItems, probFalsePos);
            }

            /// <summary>
            /// Prepares the bloom filters.
            /// </summary>
            /// <returns></returns>
            public static void Prepare()
            {
                var connections = SharedCache.Instance.GetAllWriteConnections();
                foreach (var connection in connections)
                    connection.Scripting.Prepare(new[] {BloomFilter.SetMultipleBitsScript});
            }

            private static string AllBitsSetScript
            {
                get
                {
                    return Lua.Instance["AllBitsSet"];
                }
            }

            private static string SetMultipleBitsScript
            {
                get
                {
                    return Lua.Instance["SetMultipleBits"];
                }
            }

            public Task Add(string key, string value)
            {
                var bits = new long[this.Option.NumberOfHashes];
                for (int i = 0; i < this.Option.NumberOfHashes; i++)
                    bits[i] = Murmur3.Instance.ComputeInt(value + i) % this.Option.NumberOfBits;

                this.SetBits(key, bits, true);
                return Task.FromResult(false);
            }

            private Task SetBits(string key, long[] bits, bool value)
            {
                string[] keyArgs = new[] {key};
                object[] valueArgs = new object[bits.Length + 1];
                valueArgs[0] = value;
                bits.CopyTo(valueArgs, 1);

                var connections = SharedCache.Instance.GetWriteConnections(key);
                foreach (var connection in connections)
                {
                    var task = connection.Scripting.Eval(SharedCache.Instance.Db, BloomFilter.SetMultipleBitsScript,
                        keyArgs, valueArgs, true, false, SharedCache.Instance.QueueJump);
                }
                return Task.FromResult(false);
            }

            public Task<bool> IsSet(string key, string value)
            {
                var bits = new long[this.Option.NumberOfHashes];
                for (int i = 0; i < this.Option.NumberOfHashes; i++)
                    bits[i] = Murmur3.Instance.ComputeInt(value + i) % this.Option.NumberOfBits;

                return AllBitsSet(key, bits);
            }

            private Task<bool> AllBitsSet(string key, long[] bits)
            {
                string[] keyArgs = new[] {key};
                object[] valueArgs = new object[bits.Length];
                bits.CopyTo(valueArgs, 0);

                Task<object> result = null;

                var connections = SharedCache.Instance.GetWriteConnections(key);
                foreach (var connection in connections)
                {
                    var task = connection.Scripting.Eval(SharedCache.Instance.Db, BloomFilter.AllBitsSetScript,
                        keyArgs, valueArgs, true, false, SharedCache.Instance.QueueJump);
                    if (null == result)
                        result = task;
                }

                return Task.Run(async () =>
                    {
                        if (null == result)
                            return false;

                        object value = await result;
                        return null != value && 1 == (long)value;
                    });
            }

            public override string ToString()
            {
                return this.Option.ToString();
            }
        }
    }
}