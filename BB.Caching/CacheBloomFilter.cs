using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BB.Caching.Hashing;
using BB.Caching.Shared;
using BB.Caching.Utilities;
using StackExchange.Redis;

namespace BB.Caching
{
    public static partial class Cache
    {
        public class BloomFilter
        {
            internal class BFOptions
            {
                public long NumberOfBits
                {
                    get;
                    private set;
                }

                public int NumberOfHashes
                {
                    get;
                    private set;
                }

                internal long NumberOfItems
                {
                    get;
                    private set;
                }

                private float ProbabilityOfFalsePositives
                {
                    get;
                    set;
                }

                public BFOptions(long numberOfItems, float probabilityOfFalsePositives)
                {
                    this.NumberOfItems = numberOfItems;
                    this.ProbabilityOfFalsePositives = probabilityOfFalsePositives;

                    double numberOfBits = Math.Ceiling(
                        (numberOfItems*Math.Log(probabilityOfFalsePositives))/
                            Math.Log(1.0/(Math.Pow(2.0, Math.Log(2.0)))));

                    this.NumberOfBits = (long) numberOfBits;

                    double numberOfHashes = Math.Round(Math.Log(2.0)*numberOfBits/numberOfItems);

                    this.NumberOfHashes = (int) numberOfHashes;
                }

                public override string ToString()
                {
                    return string.Format("{0} hashes, {1:#,##0}KB, {2:#0.##%} fp", this.NumberOfHashes,
                        (this.NumberOfBits/8)/1024, this.ProbabilityOfFalsePositives);
                }
            }

            internal BFOptions Options
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="numberOfItems">How many items that are expected to be stored in the bloom filter.</param>
            /// <param name="probFalsePos">The probability [0, 1] that there will be false positives.</param>
            public BloomFilter(long numberOfItems = 1000000, float probFalsePos = 0.001f)
            {
                var allScript = Lua.Instance["AllBitsSet"];
                var multipleScript = Lua.Instance["SetMultipleBits"];

                this.Options = new BFOptions(numberOfItems, probFalsePos);
                var connections = SharedCache.Instance.GetAllWriteConnections();
                foreach (var connection in connections)
                {
                    foreach (var endpoint in connection.GetEndPoints())
                    {
                        BloomFilter._allBitsSetHash = connection.GetServer(endpoint).ScriptLoad(allScript);
                        BloomFilter._setMultipleBitsHash = connection.GetServer(endpoint).ScriptLoad(multipleScript);
                    }
                }
            }

            private static byte[] AllBitsSetHash
            {
                get { return _allBitsSetHash; }
            }

            private static byte[] _allBitsSetHash;

            private static byte[] SetMultipleBitsHash
            {
                get { return _setMultipleBitsHash; }
            }

            private static byte[] _setMultipleBitsHash;

            public Task Add(string key, string value)
            {
                var bits = new RedisValue[this.Options.NumberOfHashes];
                for (int i = 0; i < this.Options.NumberOfHashes; i++)
                    bits[i] = Murmur3.Instance.ComputeInt(value + i)%this.Options.NumberOfBits;

                this.SetBits(key, bits, true);
                return Task.FromResult(false);
            }

// ReSharper disable UnusedMethodReturnValue.Local
            private Task SetBits(string key, RedisValue[] bits, bool value)
// ReSharper restore UnusedMethodReturnValue.Local
            {
                RedisKey[] keyArgs = {key};
                RedisValue[] valueArgs = new RedisValue[bits.Length + 1];
                valueArgs[0] = value;
                bits.CopyTo(valueArgs, 1);

                var connections = SharedCache.Instance.GetWriteConnections(key);
                foreach (var connection in connections)
                {
                    connection.GetDatabase(SharedCache.Instance.Db)
                        .ScriptEvaluateAsync(
                            BloomFilter.SetMultipleBitsHash,
                            keys: keyArgs,
                            values: valueArgs,
                            flags: CommandFlags.None
                        );
                }
                return Task.FromResult(false);
            }

            public Task<bool> IsSet(string key, string value)
            {
                var bits = new RedisValue[this.Options.NumberOfHashes];
                for (int i = 0; i < this.Options.NumberOfHashes; i++)
                    bits[i] = Murmur3.Instance.ComputeInt(value + i)%this.Options.NumberOfBits;

                return this.AllBitsSet(key, bits);
            }

            private Task<bool> AllBitsSet(string key, RedisValue[] bits)
            {
                RedisKey[] keyArgs = {key};
                RedisValue[] valueArgs = new RedisValue[bits.Length];
                bits.CopyTo(valueArgs, 0);

                Task<RedisResult> result = null;

                var connections = SharedCache.Instance.GetWriteConnections(key);
                foreach (var connection in connections)
                {
                    var task = connection.GetDatabase(SharedCache.Instance.Db)
                        .ScriptEvaluateAsync(
                            BloomFilter.AllBitsSetHash,
                            keys: keyArgs,
                            values: valueArgs,
                            flags: CommandFlags.None
                        );

                    if (null == result)
                        result = task;
                }

                return Task.Run(async () =>
                    {
                        if (null == result)
                            return false;

                        RedisResult value = await result;
                        return !value.IsNull && 1L == (long)value;
                    });
            }

            public override string ToString()
            {
                return this.Options.ToString();
            }
        }
    }
}