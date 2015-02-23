using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BB.Caching.Compression;
using StackExchange.Redis;
using Xunit;

namespace BB.Caching.Tests.Caching.Shared
{
    public class StringsTests : IUseFixture<DefaultTestFixture>, IDisposable
    {
        private readonly Dictionary<string, string> _kvPs = new Dictionary<string, string>
            {
                {"key1", "0"},
                {"key2", "1"},
                {"key003", "2"},
                {"key4", "3"}
            };

        private Dictionary<RedisKey, RedisValue> KVPs
        {
            get { return this._kvPs.ToDictionary(x => (RedisKey)x.Key, x => (RedisValue)x.Value); }
        }

        private RedisKey Key
        {
            get { return this._kvPs.First().Key; }
        }

        private RedisKey[] Keys
        {
            get { return this._kvPs.Keys.Select(x => (RedisKey) x).ToArray(); }
        }

        private RedisValue Value
        {
            get { return this._kvPs.First().Value; }
        }

        public StringsTests()
        {
            Cache.Shared.Keys.Remove(this.Keys).Wait();
            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.Exists(key).Result);
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(this.Keys).Wait();
            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.Exists(key).Result);
        }

        [Fact]
        public void GetPerformance()
        {
            const int asyncAmount = 100000;
            const int divisor = 10;
            const int syncAmount = asyncAmount / divisor;
            RedisKey key = "s-s-gp-key";
            RedisValue value = "s-s-gp-value";

            Cache.Shared.Keys.Remove(key).Wait();
            Cache.Shared.Strings.Set(key, value).Wait();

            var asyncMs = Get(asyncAmount, key, value);

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < syncAmount; i++)
                Assert.Equal(value, Cache.Shared.Strings.GetString(key).Result);
            long syncMs = sw.ElapsedMilliseconds;

            Console.WriteLine("String Gets:");
            Console.WriteLine("\tshared async vs sync: {0:#,##0.#}%", ((float)asyncMs / (syncMs * divisor)) * 100);
            Console.WriteLine("\t{0:#,##0.0#} aops/ms", (float)asyncAmount / asyncMs);
            Console.WriteLine("\t{0:#,##0.0#} aops/s", (float)asyncAmount * 1000 / asyncMs);
            Console.WriteLine("\t{0:#,##0.0#} sops/ms", (float)syncAmount / syncMs);
            Console.WriteLine("\t{0:#,##0.0#} sops/s", (float)syncAmount * 1000 / syncMs);

            Assert.True(asyncMs < (syncMs * divisor));

            Cache.Shared.Keys.Remove(key).Wait();
        }

        [Fact]
        public void AsyncPerformance()
        {
            const int asyncAmount = 100000;
            RedisKey key = "s-s-gp-key";
            RedisValue value = "s-s-gp-value";

            Cache.Shared.Keys.Remove(key).Wait();
            Cache.Shared.Strings.Set(key, value).Wait();

            var asyncMs = Get(asyncAmount, key, value);

            Console.WriteLine("String Gets:");
            Console.WriteLine("\t{0:#,##0.0#} aops/ms", (float)asyncAmount / asyncMs);
            Console.WriteLine("\t{0:#,##0.0#} aops/s", (float)asyncAmount * 1000 / asyncMs);

            Cache.Shared.Keys.Remove(key).Wait();
        }

        // ReSharper disable UnusedParameter.Local
        private static long Get(int amount, RedisKey key, RedisValue value)
        // ReSharper restore UnusedParameter.Local
        {
            var tasks = new Task<RedisValue>[amount];
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < amount; i++)
                tasks[i] = Cache.Shared.Strings.GetString(key);

            for (int i = 0; i < amount; i++)
            {
                Assert.False(tasks[i].Result.IsNull);
                Assert.Equal(value, tasks[i].Result);
            }
            return sw.ElapsedMilliseconds;
        }

        [Fact]
        public void AppendString()
        {
            string value = this.Value;
            Assert.Equal(value.Length, Cache.Shared.Strings.Append(this.Key, this.Value).Result);
            Assert.Equal(this.Value, Cache.Shared.Strings.GetString(this.Key).Result);
            Assert.Equal(value.Length * 2, Cache.Shared.Strings.Append(this.Key, this.Value).Result);
            Assert.Equal(value + value, (string)Cache.Shared.Strings.GetString(this.Key).Result);
        }

        [Fact]
        public void AppendBytes()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Assert.Equal(bytes.Length, Cache.Shared.Strings.Append(this.Key, bytes).Result);
            Assert.Equal(this.Value, Cache.Shared.Strings.GetString(this.Key).Result);
            Assert.Equal(bytes.Length * 2, Cache.Shared.Strings.Append(this.Key, bytes).Result);
            Assert.Equal(
                Encoding.UTF8.GetBytes(this.Value + this.Value),
                (byte[])Cache.Shared.Strings.GetByteArray(this.Key).Result
            );
        }

        [Fact]
        public void DecrementLong()
        {
            Cache.Shared.Strings.Decrement(this.Key).Wait();
            Assert.Equal(-1, Cache.Shared.Strings.GetInt64(this.Key).Result);
            Cache.Shared.Strings.Decrement(this.Key, 3).Wait();
            Assert.Equal(-4, Cache.Shared.Strings.GetInt64(this.Key).Result);
        }

        [Fact]
        public void IncrementLong()
        {
            Cache.Shared.Strings.Increment(this.Key).Wait();
            Assert.Equal(1, Cache.Shared.Strings.GetInt64(this.Key).Result);
            Cache.Shared.Strings.Increment(this.Key, 3).Wait();
            Assert.Equal(4, Cache.Shared.Strings.GetInt64(this.Key).Result);
        }

        [Fact]
        public void GetByteArray()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.Set(this.Key, bytes).Wait();
            var result = Cache.Shared.Strings.GetByteArray(this.Key).Result;

            Assert.False(result.IsNull);
            Assert.Equal(bytes, (byte[])result);
        }

        [Fact]
        public void GetString()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            var result = Cache.Shared.Strings.GetString(this.Key).Result;

            Assert.False(result.IsNull);
            Assert.Equal(this.Value, result);
        }

        [Fact]
        public void GetLong()
        {
            long value = long.Parse(this.Value);
            Cache.Shared.Strings.Set(this.Key, value).Wait();
            var result = Cache.Shared.Strings.GetInt64(this.Key).Result;

            Assert.False(result.IsNull);
            Assert.Equal(value, result);
        }

        [Fact]
        public void GetByteArraySubset()
        {
            const string value = "hello";
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            Cache.Shared.Strings.Set(this.Key, bytes).Wait();

            byte[] subset = new byte[3];
            Array.Copy(bytes, 0, subset, 0, 3);

            var result = Cache.Shared.Strings.GetByteArray(this.Key, 0, 2).Result;
            Assert.False(result.IsNull);
            Assert.Equal(subset, (byte[])result);
        }

        [Fact]
        public void GetStringSubset()
        {
            const string value = "hello";
            Cache.Shared.Strings.Set(this.Key, value).Wait();

            var result = Cache.Shared.Strings.GetString(this.Key, 0, 2).Result;
            Assert.False(result.IsNull);
            Assert.Equal(value.Substring(0, 3), (string)result);
        }

        [Fact]
        public void GetMultipleByteArray()
        {
            var dictionary = this._kvPs.ToDictionary(k => (RedisKey)k.Key, k => (RedisValue)Encoding.UTF8.GetBytes(k.Value));
            Cache.Shared.Strings.Set(dictionary).Wait();

            var results = Cache.Shared.Strings.GetByteArray(this._kvPs.Keys.Select(x => (RedisKey)x).ToArray()).Result;
            int i = 0;
            foreach (var result in results)
            {
                Assert.False(result.IsNull);
                Assert.True(((byte[])dictionary.ElementAt(i).Value).SequenceEqual((byte[])result));
                ++i;
            }
        }

        [Fact]
        public void GetMultipleStrings()
        {
            Cache.Shared.Strings.Set(this.KVPs).Wait();

            var results = Cache.Shared.Strings.GetString(this.KVPs.Keys.ToArray()).Result;
            int i = 0;
            foreach (var result in results)
            {
                Assert.False(result.IsNull);
                Assert.Equal(this._kvPs.ElementAt(i).Value, (string)result);
                ++i;
            }
        }

        [Fact]
        public void GetSetByteArray()
        {
            byte[] first = Encoding.UTF8.GetBytes("0");
            byte[] second = Encoding.UTF8.GetBytes("1");

            Cache.Shared.Strings.Set(this.Key, first).Wait();
            byte[] result = Cache.Shared.Strings.GetSet(this.Key, second).Result;
            Assert.Equal(first, result);
            Assert.Equal(second, (byte[])Cache.Shared.Strings.GetByteArray(this.Key).Result);
        }

        [Fact]
        public void GetSetString()
        {
            const string first = "0";
            const string second = "1";

            Cache.Shared.Strings.Set(this.Key, first).Wait();
            string result = Cache.Shared.Strings.GetSet(this.Key, second).Result;
            Assert.Equal(first, result);
            Assert.Equal(second, (string)Cache.Shared.Strings.GetString(this.Key).Result);
        }

        [Fact]
        public void SetString()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            Assert.Equal(this.Value, Cache.Shared.Strings.GetString(this.Key).Result);
        }

        [Fact]
        public void SetLong()
        {
            Cache.Shared.Strings.Set(this.Key, 2L).Wait();
            Assert.Equal(2L, Cache.Shared.Strings.GetInt64(this.Key).Result);
        }

        [Fact]
        public void SetByteArray()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.Set(this.Key, bytes).Wait();
            Assert.Equal(bytes, (byte[])Cache.Shared.Strings.GetByteArray(this.Key).Result);
        }

        [Fact]
        public void SetStringExpires()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Wait();
            Assert.False(Cache.Shared.Strings.GetString(this.Key).Result.IsNull);
            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.GetString(this.Key).Result.IsNull);
        }

        [Fact]
        public void SetByteArrayExpires()
        {
            Cache.Shared.Strings.Set(this.Key, Encoding.UTF8.GetBytes(this.Value), TimeSpan.FromSeconds(.2))
                .Wait();
            Assert.False(Cache.Shared.Strings.GetByteArray(this.Key).Result.IsNull);
            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.GetByteArray(this.Key).Result.IsNull);
        }

        [Fact]
        public void SetStringOffset()
        {
            string value = this.Value;
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            Cache.Shared.Strings.Set(this.Key, 1, this.Value).Wait();
            Assert.Equal(value.Substring(0, 1) + this.Value, (string)Cache.Shared.Strings.GetString(this.Key).Result);
        }

        [Fact]
        public void SetByteArrayOffset()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.Set(this.Key, bytes).Wait();
            Cache.Shared.Strings.Set(this.Key, 1, bytes).Wait();
            byte[] result = new byte[2];
            Buffer.BlockCopy(bytes, 0, result, 0, 1);
            Buffer.BlockCopy(bytes, 0, result, 1, 1);
            Assert.Equal(result, (byte[])Cache.Shared.Strings.GetByteArray(this.Key).Result);
        }

        [Fact]
        public void SetMultipleStrings()
        {
            Cache.Shared.Strings.Set(this.KVPs).Wait();
            foreach (var kvp in this._kvPs)
                Assert.Equal(kvp.Value, (string)Cache.Shared.Strings.GetString(kvp.Key).Result);
        }

        [Fact]
        public void SetMultipleByteArrays()
        {
            var dict = this.KVPs.ToDictionary(kvp => kvp.Key, kvp => (RedisValue)Encoding.UTF8.GetBytes(kvp.Value));

            Cache.Shared.Strings.Set(dict).Wait();
            foreach (var kvp in dict)
                Assert.Equal((byte[])kvp.Value, (byte[])Cache.Shared.Strings.GetByteArray(kvp.Key).Result);
        }

        [Fact]
        public void SetMultipleStringsIfNotExists()
        {
            Cache.Shared.Strings.Set(this.KVPs).Wait();
            Assert.False(Cache.Shared.Strings.SetIfNotExists(this.KVPs).Result);
        }

        [Fact]
        public void SetMultipleByteArraysIfNotExists()
        {
            var dict = this.KVPs.ToDictionary(kvp => kvp.Key, kvp => (RedisValue)Encoding.UTF8.GetBytes(kvp.Value));

            Cache.Shared.Strings.Set(dict).Wait();
            Assert.False(Cache.Shared.Strings.SetIfNotExists(dict).Result);
        }

        [Fact]
        public void SetStringIfNotExists()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            Assert.False(Cache.Shared.Strings.SetIfNotExists(this.Key, this.Value).Result);
        }

        [Fact]
        public void SetByteArrayIfNotExists()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.Set(this.Key, bytes).Wait();
            Assert.False(Cache.Shared.Strings.SetIfNotExists(this.Key, bytes).Result);
        }

        [Fact]
        public void GetBit()
        {
            Cache.Shared.Strings.SetBit(this.Key, 3, true);
            Assert.True(Cache.Shared.Strings.GetBit(this.Key, 3).Result);
            Cache.Shared.Strings.SetBit(this.Key, 3, false);
            Assert.False(Cache.Shared.Strings.GetBit(this.Key, 3).Result);
        }

        [Fact]
        public void GetLength()
        {
            string value = this.Value;
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            Assert.Equal(value.Length, Cache.Shared.Strings.GetLength(this.Key).Result);
        }

        [Fact]
        public void SetBit()
        {
            Cache.Shared.Strings.SetBit(this.Key, 3, true);
            Assert.True(Cache.Shared.Strings.GetBit(this.Key, 3).Result);
            Cache.Shared.Strings.SetBit(this.Key, 3, false);
            Assert.False(Cache.Shared.Strings.GetBit(this.Key, 3).Result);
        }

        [Fact]
        public void CountSetBits()
        {
            Cache.Shared.Strings.SetBit(this.Key, 3, true);
            Assert.Equal(1, Cache.Shared.Strings.CountSetBits(this.Key).Result);
            Cache.Shared.Strings.SetBit(this.Key, 10, true);
            Assert.Equal(2, Cache.Shared.Strings.CountSetBits(this.Key).Result);
            Cache.Shared.Strings.SetBit(this.Key, 13, true);
            Assert.Equal(3, Cache.Shared.Strings.CountSetBits(this.Key).Result);
        }

        //                [Fact]
        //                public void BitwiseAnd()
        //                {
        //                    string firstKey = Keys.ElementAt(1);
        //                    string secondKey = Keys.ElementAt(2);
        //                    string thirdKey = Keys.ElementAt(3);
        //
        //                    Cache.Shared.Strings.SetBit(firstKey, 3, true);
        //                    Cache.Shared.Strings.SetBit(secondKey, 4, true);
        //                    Cache.Shared.Strings.BitwiseAnd(thirdKey, new [] {firstKey, secondKey});
        //
        //                    Assert.Equal(0, Cache.Shared.Strings.CountSetBits(thirdKey).Result);
        //                }

        [Fact]
        public void TakeLock()
        {
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);
            Assert.False(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);

            Cache.Shared.Strings.ReleaseLock(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);

            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);
        }

        [Fact]
        public void ReleaseLock()
        {
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);
            Assert.False(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);

            Cache.Shared.Strings.ReleaseLock(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);

            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);
        }

        [Fact]
        public void CompressionTest()
        {
            const string expected = "abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd";
            byte[] compressSet = Compress.Compression.Compress(expected);
            Cache.Shared.Strings.Set(this.Key, compressSet).Wait();

            byte[] compressGet = Cache.Shared.Strings.GetByteArray(this.Key).Result;
            Assert.Equal(expected, Compress.Compression.DecompressString(compressGet));
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }
    }
}