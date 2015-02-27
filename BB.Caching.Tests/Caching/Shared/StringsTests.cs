using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            Cache.Shared.Keys.DeleteAsync(this.Keys).Wait();
            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.ExistsAsync(key).Result);
        }

        public void Dispose()
        {
            Cache.Shared.Keys.DeleteAsync(this.Keys).Wait();
            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.ExistsAsync(key).Result);
        }

        [Fact]
        public void AppendString()
        {
            string value = this.Value;
            Assert.Equal(value.Length, Cache.Shared.Strings.Append(this.Key, this.Value));
            Assert.Equal(this.Value, Cache.Shared.Strings.Get(this.Key));
            Assert.Equal(value.Length * 2, Cache.Shared.Strings.Append(this.Key, this.Value));
            Assert.Equal(value + value, (string)Cache.Shared.Strings.Get(this.Key));
        }

        [Fact]
        public void AppendStringAsync()
        {
            string value = this.Value;
            Assert.Equal(value.Length, Cache.Shared.Strings.AppendAsync(this.Key, this.Value).Result);
            Assert.Equal(this.Value, Cache.Shared.Strings.GetAsync(this.Key).Result);
            Assert.Equal(value.Length * 2, Cache.Shared.Strings.AppendAsync(this.Key, this.Value).Result);
            Assert.Equal(value + value, (string)Cache.Shared.Strings.GetAsync(this.Key).Result);
        }

        [Fact]
        public void AppendBytes()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Assert.Equal(bytes.Length, Cache.Shared.Strings.Append(this.Key, bytes));
            Assert.Equal(this.Value, Cache.Shared.Strings.Get(this.Key));
            Assert.Equal(bytes.Length * 2, Cache.Shared.Strings.Append(this.Key, bytes));
            Assert.Equal(
                Encoding.UTF8.GetBytes(this.Value + this.Value),
                (byte[])Cache.Shared.Strings.Get(this.Key)
            );
        }

        [Fact]
        public void AppendBytesAsync()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Assert.Equal(bytes.Length, Cache.Shared.Strings.AppendAsync(this.Key, bytes).Result);
            Assert.Equal(this.Value, Cache.Shared.Strings.GetAsync(this.Key).Result);
            Assert.Equal(bytes.Length * 2, Cache.Shared.Strings.AppendAsync(this.Key, bytes).Result);
            Assert.Equal(
                Encoding.UTF8.GetBytes(this.Value + this.Value),
                (byte[])Cache.Shared.Strings.GetAsync(this.Key).Result
            );
        }

        [Fact]
        public void DecrementLong()
        {
            Cache.Shared.Strings.Decrement(this.Key);
            Assert.Equal(-1, Cache.Shared.Strings.Get(this.Key));
            Cache.Shared.Strings.Decrement(this.Key, 3);
            Assert.Equal(-4, Cache.Shared.Strings.Get(this.Key));
        }

        [Fact]
        public void DecrementLongAsync()
        {
            Cache.Shared.Strings.DecrementAsync(this.Key).Wait();
            Assert.Equal(-1, Cache.Shared.Strings.GetAsync(this.Key).Result);
            Cache.Shared.Strings.DecrementAsync(this.Key, 3).Wait();
            Assert.Equal(-4, Cache.Shared.Strings.GetAsync(this.Key).Result);
        }

        [Fact]
        public void IncrementLong()
        {
            Cache.Shared.Strings.Increment(this.Key);
            Assert.Equal(1, Cache.Shared.Strings.Get(this.Key));
            Cache.Shared.Strings.Increment(this.Key, 3);
            Assert.Equal(4, Cache.Shared.Strings.Get(this.Key));
        }

        [Fact]
        public void IncrementLongAsync()
        {
            Cache.Shared.Strings.IncrementAsync(this.Key).Wait();
            Assert.Equal(1, Cache.Shared.Strings.GetAsync(this.Key).Result);
            Cache.Shared.Strings.IncrementAsync(this.Key, 3).Wait();
            Assert.Equal(4, Cache.Shared.Strings.GetAsync(this.Key).Result);
        }

        [Fact]
        public void GetByteArray()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.Set(this.Key, bytes);
            var result = Cache.Shared.Strings.Get(this.Key);

            Assert.False(result.IsNull);
            Assert.Equal(bytes, (byte[])result);
        }

        [Fact]
        public void GetByteArrayAsync()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.SetAsync(this.Key, bytes).Wait();
            var result = Cache.Shared.Strings.GetAsync(this.Key).Result;

            Assert.False(result.IsNull);
            Assert.Equal(bytes, (byte[])result);
        }

        [Fact]
        public void GetString()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value);
            var result = Cache.Shared.Strings.Get(this.Key);

            Assert.False(result.IsNull);
            Assert.Equal(this.Value, result);
        }

        [Fact]
        public void GetStringAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            var result = Cache.Shared.Strings.GetAsync(this.Key).Result;

            Assert.False(result.IsNull);
            Assert.Equal(this.Value, result);
        }

        [Fact]
        public void GetLong()
        {
            long value = long.Parse(this.Value);
            Cache.Shared.Strings.Set(this.Key, value);
            var result = Cache.Shared.Strings.Get(this.Key);

            Assert.False(result.IsNull);
            Assert.Equal(value, result);
        }

        [Fact]
        public void GetLongAsync()
        {
            long value = long.Parse(this.Value);
            Cache.Shared.Strings.SetAsync(this.Key, value).Wait();
            var result = Cache.Shared.Strings.GetAsync(this.Key).Result;

            Assert.False(result.IsNull);
            Assert.Equal(value, result);
        }

        [Fact]
        public void GetByteArraySubset()
        {
            const string value = "hello";
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            Cache.Shared.Strings.Set(this.Key, bytes);

            byte[] subset = new byte[3];
            Array.Copy(bytes, 0, subset, 0, 3);

            var result = Cache.Shared.Strings.Get(this.Key, 0, 2);
            Assert.False(result.IsNull);
            Assert.Equal(subset, (byte[])result);
        }

        [Fact]
        public void GetByteArraySubsetAsync()
        {
            const string value = "hello";
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            Cache.Shared.Strings.SetAsync(this.Key, bytes).Wait();

            byte[] subset = new byte[3];
            Array.Copy(bytes, 0, subset, 0, 3);

            var result = Cache.Shared.Strings.GetAsync(this.Key, 0, 2).Result;
            Assert.False(result.IsNull);
            Assert.Equal(subset, (byte[])result);
        }

        [Fact]
        public void GetStringSubset()
        {
            const string value = "hello";
            Cache.Shared.Strings.Set(this.Key, value);

            var result = Cache.Shared.Strings.Get(this.Key, 0, 2);
            Assert.False(result.IsNull);
            Assert.Equal(value.Substring(0, 3), (string)result);
        }

        [Fact]
        public void GetStringSubsetAsync()
        {
            const string value = "hello";
            Cache.Shared.Strings.SetAsync(this.Key, value).Wait();

            var result = Cache.Shared.Strings.GetAsync(this.Key, 0, 2).Result;
            Assert.False(result.IsNull);
            Assert.Equal(value.Substring(0, 3), (string)result);
        }

        [Fact]
        public void GetMultipleByteArray()
        {
            var dictionary = this._kvPs.ToDictionary(k => (RedisKey)k.Key, k => (RedisValue)Encoding.UTF8.GetBytes(k.Value));
            Cache.Shared.Strings.Set(dictionary);

            var results = Cache.Shared.Strings.Get(this._kvPs.Keys.Select(x => (RedisKey)x).ToArray());
            int i = 0;
            foreach (var result in results)
            {
                Assert.False(result.IsNull);
                Assert.True(((byte[])dictionary.ElementAt(i).Value).SequenceEqual((byte[])result));
                ++i;
            }
        }

        [Fact]
        public void GetMultipleByteArrayAsync()
        {
            var dictionary = this._kvPs.ToDictionary(k => (RedisKey)k.Key, k => (RedisValue)Encoding.UTF8.GetBytes(k.Value));
            Cache.Shared.Strings.SetAsync(dictionary).Wait();

            var results = Cache.Shared.Strings.GetAsync(this._kvPs.Keys.Select(x => (RedisKey)x).ToArray()).Result;
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
            Cache.Shared.Strings.Set(this.KVPs);

            var results = Cache.Shared.Strings.Get(this.KVPs.Keys.ToArray());
            int i = 0;
            foreach (var result in results)
            {
                Assert.False(result.IsNull);
                Assert.Equal(this._kvPs.ElementAt(i).Value, (string)result);
                ++i;
            }
        }

        [Fact]
        public void GetMultipleStringsAsync()
        {
            Cache.Shared.Strings.SetAsync(this.KVPs).Wait();

            var results = Cache.Shared.Strings.GetAsync(this.KVPs.Keys.ToArray()).Result;
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

            Cache.Shared.Strings.Set(this.Key, first);
            byte[] result = Cache.Shared.Strings.GetSet(this.Key, second);
            Assert.Equal(first, result);
            Assert.Equal(second, (byte[])Cache.Shared.Strings.Get(this.Key));
        }

        [Fact]
        public void GetSetByteArrayAsync()
        {
            byte[] first = Encoding.UTF8.GetBytes("0");
            byte[] second = Encoding.UTF8.GetBytes("1");

            Cache.Shared.Strings.SetAsync(this.Key, first).Wait();
            byte[] result = Cache.Shared.Strings.GetSetAsync(this.Key, second).Result;
            Assert.Equal(first, result);
            Assert.Equal(second, (byte[])Cache.Shared.Strings.GetAsync(this.Key).Result);
        }

        [Fact]
        public void GetSetString()
        {
            const string first = "0";
            const string second = "1";

            Cache.Shared.Strings.Set(this.Key, first);
            string result = Cache.Shared.Strings.GetSet(this.Key, second);
            Assert.Equal(first, result);
            Assert.Equal(second, (string)Cache.Shared.Strings.Get(this.Key));
        }

        [Fact]
        public void GetSetStringAsync()
        {
            const string first = "0";
            const string second = "1";

            Cache.Shared.Strings.SetAsync(this.Key, first).Wait();
            string result = Cache.Shared.Strings.GetSetAsync(this.Key, second).Result;
            Assert.Equal(first, result);
            Assert.Equal(second, (string)Cache.Shared.Strings.GetAsync(this.Key).Result);
        }

        [Fact]
        public void SetString()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value);
            Assert.Equal(this.Value, Cache.Shared.Strings.Get(this.Key));
        }

        [Fact]
        public void SetStringAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            Assert.Equal(this.Value, Cache.Shared.Strings.GetAsync(this.Key).Result);
        }

        [Fact]
        public void SetLong()
        {
            Cache.Shared.Strings.Set(this.Key, 2L);
            Assert.Equal(2L, Cache.Shared.Strings.Get(this.Key));
        }

        [Fact]
        public void SetLongAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, 2L).Wait();
            Assert.Equal(2L, Cache.Shared.Strings.GetAsync(this.Key).Result);
        }

        [Fact]
        public void SetByteArray()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.Set(this.Key, bytes);
            Assert.Equal(bytes, (byte[])Cache.Shared.Strings.Get(this.Key));
        }

        [Fact]
        public void SetByteArrayAsync()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.SetAsync(this.Key, bytes).Wait();
            Assert.Equal(bytes, (byte[])Cache.Shared.Strings.GetAsync(this.Key).Result);
        }

        [Fact]
        public void SetStringExpires()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value, TimeSpan.FromSeconds(.2));
            Assert.False(Cache.Shared.Strings.Get(this.Key).IsNull);
            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.Get(this.Key).IsNull);
        }

        [Fact]
        public void SetStringExpiresAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Wait();
            Assert.False(Cache.Shared.Strings.GetAsync(this.Key).Result.IsNull);
            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.GetAsync(this.Key).Result.IsNull);
        }

        [Fact]
        public void SetByteArrayExpires()
        {
            Cache.Shared.Strings.Set(this.Key, Encoding.UTF8.GetBytes(this.Value), TimeSpan.FromSeconds(.2));
            Assert.False(Cache.Shared.Strings.Get(this.Key).IsNull);
            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.Get(this.Key).IsNull);
        }

        [Fact]
        public void SetByteArrayExpiresAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, Encoding.UTF8.GetBytes(this.Value), TimeSpan.FromSeconds(.2))
                .Wait();
            Assert.False(Cache.Shared.Strings.GetAsync(this.Key).Result.IsNull);
            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.GetAsync(this.Key).Result.IsNull);
        }

        [Fact]
        public void SetStringOffset()
        {
            string value = this.Value;
            Cache.Shared.Strings.Set(this.Key, this.Value);
            Cache.Shared.Strings.Set(this.Key, 1, this.Value);
            Assert.Equal(value.Substring(0, 1) + this.Value, (string)Cache.Shared.Strings.Get(this.Key));
        }

        [Fact]
        public void SetStringOffsetAsync()
        {
            string value = this.Value;
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            Cache.Shared.Strings.SetAsync(this.Key, 1, this.Value).Wait();
            Assert.Equal(value.Substring(0, 1) + this.Value, (string)Cache.Shared.Strings.GetAsync(this.Key).Result);
        }

        [Fact]
        public void SetByteArrayOffset()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.Set(this.Key, bytes);
            Cache.Shared.Strings.Set(this.Key, 1, bytes);
            byte[] result = new byte[2];
            Buffer.BlockCopy(bytes, 0, result, 0, 1);
            Buffer.BlockCopy(bytes, 0, result, 1, 1);
            Assert.Equal(result, (byte[])Cache.Shared.Strings.Get(this.Key));
        }

        [Fact]
        public void SetByteArrayOffsetAsync()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.SetAsync(this.Key, bytes).Wait();
            Cache.Shared.Strings.SetAsync(this.Key, 1, bytes).Wait();
            byte[] result = new byte[2];
            Buffer.BlockCopy(bytes, 0, result, 0, 1);
            Buffer.BlockCopy(bytes, 0, result, 1, 1);
            Assert.Equal(result, (byte[])Cache.Shared.Strings.GetAsync(this.Key).Result);
        }

        [Fact]
        public void SetMultipleStrings()
        {
            Cache.Shared.Strings.Set(this.KVPs);
            foreach (var kvp in this._kvPs)
                Assert.Equal(kvp.Value, (string)Cache.Shared.Strings.Get(kvp.Key));
        }

        [Fact]
        public void SetMultipleStringsAsync()
        {
            Cache.Shared.Strings.SetAsync(this.KVPs).Wait();
            foreach (var kvp in this._kvPs)
                Assert.Equal(kvp.Value, (string)Cache.Shared.Strings.GetAsync(kvp.Key).Result);
        }

        [Fact]
        public void SetMultipleByteArrays()
        {
            var dict = this.KVPs.ToDictionary(kvp => kvp.Key, kvp => (RedisValue)Encoding.UTF8.GetBytes(kvp.Value));

            Cache.Shared.Strings.Set(dict);
            foreach (var kvp in dict)
                Assert.Equal((byte[])kvp.Value, (byte[])Cache.Shared.Strings.Get(kvp.Key));
        }

        [Fact]
        public void SetMultipleByteArraysAsync()
        {
            var dict = this.KVPs.ToDictionary(kvp => kvp.Key, kvp => (RedisValue)Encoding.UTF8.GetBytes(kvp.Value));

            Cache.Shared.Strings.SetAsync(dict).Wait();
            foreach (var kvp in dict)
                Assert.Equal((byte[])kvp.Value, (byte[])Cache.Shared.Strings.GetAsync(kvp.Key).Result);
        }

        [Fact]
        public void SetMultipleStringsIfNotExists()
        {
            Cache.Shared.Strings.Set(this.KVPs);
            Assert.False(Cache.Shared.Strings.SetIfNotExists(this.KVPs));
        }

        [Fact]
        public void SetMultipleStringsIfNotExistsAsync()
        {
            Cache.Shared.Strings.SetAsync(this.KVPs).Wait();
            Assert.False(Cache.Shared.Strings.SetIfNotExistsAsync(this.KVPs).Result);
        }

        [Fact]
        public void SetMultipleByteArraysIfNotExists()
        {
            var dict = this.KVPs.ToDictionary(kvp => kvp.Key, kvp => (RedisValue)Encoding.UTF8.GetBytes(kvp.Value));

            Cache.Shared.Strings.Set(dict);
            Assert.False(Cache.Shared.Strings.SetIfNotExists(dict));
        }

        [Fact]
        public void SetMultipleByteArraysIfNotExistsAsync()
        {
            var dict = this.KVPs.ToDictionary(kvp => kvp.Key, kvp => (RedisValue)Encoding.UTF8.GetBytes(kvp.Value));

            Cache.Shared.Strings.SetAsync(dict).Wait();
            Assert.False(Cache.Shared.Strings.SetIfNotExistsAsync(dict).Result);
        }

        [Fact]
        public void SetStringIfNotExists()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value);
            Assert.False(Cache.Shared.Strings.SetIfNotExists(this.Key, this.Value));
        }

        [Fact]
        public void SetStringIfNotExistsAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            Assert.False(Cache.Shared.Strings.SetIfNotExistsAsync(this.Key, this.Value).Result);
        }

        [Fact]
        public void SetByteArrayIfNotExists()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.Set(this.Key, bytes);
            Assert.False(Cache.Shared.Strings.SetIfNotExists(this.Key, bytes));
        }

        [Fact]
        public void SetByteArrayIfNotExistsAsync()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
            Cache.Shared.Strings.SetAsync(this.Key, bytes).Wait();
            Assert.False(Cache.Shared.Strings.SetIfNotExistsAsync(this.Key, bytes).Result);
        }

        [Fact]
        public void GetBit()
        {
            Cache.Shared.Strings.SetBit(this.Key, 3, true);
            Assert.True(Cache.Shared.Strings.GetBit(this.Key, 3));
            Cache.Shared.Strings.SetBit(this.Key, 3, false);
            Assert.False(Cache.Shared.Strings.GetBit(this.Key, 3));
        }

        [Fact]
        public void GetBitAsync()
        {
            Cache.Shared.Strings.SetBitAsync(this.Key, 3, true).Wait();
            Assert.True(Cache.Shared.Strings.GetBitAsync(this.Key, 3).Result);
            Cache.Shared.Strings.SetBitAsync(this.Key, 3, false).Wait();
            Assert.False(Cache.Shared.Strings.GetBitAsync(this.Key, 3).Result);
        }

        [Fact]
        public void GetLength()
        {
            string value = this.Value;
            Cache.Shared.Strings.Set(this.Key, this.Value);
            Assert.Equal(value.Length, Cache.Shared.Strings.GetLength(this.Key));
        }

        [Fact]
        public void GetLengthAsync()
        {
            string value = this.Value;
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            Assert.Equal(value.Length, Cache.Shared.Strings.GetLengthAsync(this.Key).Result);
        }

        [Fact]
        public void SetBit()
        {
            Cache.Shared.Strings.SetBit(this.Key, 3, true);
            Assert.True(Cache.Shared.Strings.GetBit(this.Key, 3));
            Cache.Shared.Strings.SetBit(this.Key, 3, false);
            Assert.False(Cache.Shared.Strings.GetBit(this.Key, 3));
        }

        [Fact]
        public void SetBitAsync()
        {
            Cache.Shared.Strings.SetBitAsync(this.Key, 3, true).Wait();
            Assert.True(Cache.Shared.Strings.GetBitAsync(this.Key, 3).Result);
            Cache.Shared.Strings.SetBitAsync(this.Key, 3, false).Wait();
            Assert.False(Cache.Shared.Strings.GetBitAsync(this.Key, 3).Result);
        }

        [Fact]
        public void CountSetBits()
        {
            Cache.Shared.Strings.SetBit(this.Key, 3, true);
            Assert.Equal(1, Cache.Shared.Strings.CountSetBits(this.Key));
            Cache.Shared.Strings.SetBit(this.Key, 10, true);
            Assert.Equal(2, Cache.Shared.Strings.CountSetBits(this.Key));
            Cache.Shared.Strings.SetBit(this.Key, 13, true);
            Assert.Equal(3, Cache.Shared.Strings.CountSetBits(this.Key));
        }

        [Fact]
        public void CountSetBitsAsync()
        {
            Cache.Shared.Strings.SetBitAsync(this.Key, 3, true).Wait();
            Assert.Equal(1, Cache.Shared.Strings.CountSetBitsAsync(this.Key).Result);
            Cache.Shared.Strings.SetBitAsync(this.Key, 10, true).Wait();
            Assert.Equal(2, Cache.Shared.Strings.CountSetBitsAsync(this.Key).Result);
            Cache.Shared.Strings.SetBitAsync(this.Key, 13, true).Wait();
            Assert.Equal(3, Cache.Shared.Strings.CountSetBitsAsync(this.Key).Result);
        }

        //                [Fact]
        //                public void BitwiseAnd()
        //                {
        //                    string firstKey = Keys.ElementAt(1);
        //                    string secondKey = Keys.ElementAt(2);
        //                    string thirdKey = Keys.ElementAt(3);
        //
        //                    Cache.Shared.Strings.SetBitAsync(firstKey, 3, true);
        //                    Cache.Shared.Strings.SetBitAsync(secondKey, 4, true);
        //                    Cache.Shared.Strings.BitwiseAnd(thirdKey, new [] {firstKey, secondKey});
        //
        //                    Assert.Equal(0, Cache.Shared.Strings.CountSetBitsAsync(thirdKey).Result);
        //                }

        [Fact]
        public void TakeLock()
        {
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)));
            Assert.False(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)));

            Cache.Shared.Strings.ReleaseLock(this.Key, this.Value);
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)));

            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)));
        }

        [Fact]
        public void TakeLockAsync()
        {
            Assert.True(Cache.Shared.Strings.TakeLockAsync(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);
            Assert.False(Cache.Shared.Strings.TakeLockAsync(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);

            Cache.Shared.Strings.ReleaseLockAsync(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Strings.TakeLockAsync(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);

            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.TakeLockAsync(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);
        }

        [Fact]
        public void ReleaseLock()
        {
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)));
            Assert.False(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)));

            Cache.Shared.Strings.ReleaseLock(this.Key, this.Value);
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)));

            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(.2)));
        }

        [Fact]
        public void ReleaseLockAsync()
        {
            Assert.True(Cache.Shared.Strings.TakeLockAsync(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);
            Assert.False(Cache.Shared.Strings.TakeLockAsync(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);

            Cache.Shared.Strings.ReleaseLockAsync(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Strings.TakeLockAsync(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);

            Thread.Sleep(210);
            Assert.True(Cache.Shared.Strings.TakeLockAsync(this.Key, this.Value, TimeSpan.FromSeconds(.2)).Result);
        }

        [Fact]
        public void CompressionTest()
        {
            const string expected = "abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd";
            byte[] compressSet = Compress.Compression.Compress(expected);
            Cache.Shared.Strings.Set(this.Key, compressSet);

            byte[] compressGet = Cache.Shared.Strings.Get(this.Key);
            Assert.Equal(expected, Compress.Compression.Decompress<string>(compressGet));
        }

        [Fact]
        public void CompressionTestAsync()
        {
            const string expected = "abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd";
            byte[] compressSet = Compress.Compression.CompressAsync(expected).Result;
            Cache.Shared.Strings.SetAsync(this.Key, compressSet).Wait();

            byte[] compressGet = Cache.Shared.Strings.GetAsync(this.Key).Result;
            Assert.Equal(expected, Compress.Compression.DecompressAsync<string>(compressGet).Result);
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }
    }
}