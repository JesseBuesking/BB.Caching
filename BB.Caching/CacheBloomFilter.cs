using System;
using System.Threading.Tasks;
using BB.Caching.Hashing;
using BB.Caching.Shared;
using BB.Caching.Utilities;

namespace BB.Caching
{
    public static partial class Cache
    {
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
                get { return Lua.Instance["AllBitsSet"]; }
            }

            private static string SetMultipleBitsScript
            {
                get { return Lua.Instance["SetMultipleBits"]; }
            }

            public Task Add(string key, string value)
            {
                var bits = new long[this.Option.NumberOfHashes];
                for (int i = 0; i < this.Option.NumberOfHashes; i++)
                    bits[i] = Murmur3.Instance.ComputeInt(value + i)%this.Option.NumberOfBits;

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
                    bits[i] = Murmur3.Instance.ComputeInt(value + i)%this.Option.NumberOfBits;

                return this.AllBitsSet(key, bits);
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
                        return null != value && 1 == (long) value;
                    });
            }

            public override string ToString()
            {
                return this.Option.ToString();
            }
        }
    }
}