using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests.SharedTests
{
    public class HashesTests : IDisposable
    {
        private readonly Dictionary<string, Dictionary<string, string>> _kvPs =
            new Dictionary<string, Dictionary<string, string>>
                {
                    {"key1", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                    {"key2", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                    {"key003", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                    {"key4", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                };

        private Dictionary<string, Dictionary<string, byte[]>> KVPsByteArrays
        {
            get
            {
                return this._kvPs.ToDictionary(k => k.Key, v => v.Value.ToDictionary(
                    k => k.Key, w => Encoding.UTF8.GetBytes(w.Value)));
            }
        }

        private string Key
        {
            get { return this._kvPs.First().Key; }
        }

        private string[] Keys
        {
            get { return this._kvPs.Keys.ToArray(); }
        }

        private Dictionary<string, string> Value
        {
            get { return this._kvPs.First().Value; }
        }

        public HashesTests()
        {
            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

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
        public void Remove()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, this.Value.ElementAt(0).Value).Wait();
            Assert.True(Cache.Shared.Hashes.Remove(this.Key, this.Value.ElementAt(0).Key).Result);
            Assert.False(Cache.Shared.Hashes.Remove(this.Key, this.Value.ElementAt(0).Key).Result);
        }

        [Fact]
        public void RemoveMultiple()
        {
            var first = this.KVPsByteArrays.First();

            Cache.Shared.Hashes.Set(first.Key, first.Value).Wait();
            Assert.Equal(2, Cache.Shared.Hashes.Remove(first.Key, first.Value.Keys.ToArray()).Result);
            Assert.Equal(0, Cache.Shared.Hashes.Remove(first.Key, first.Value.Keys.ToArray()).Result);
        }

        [Fact]
        public void Exists()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, this.Value.ElementAt(0).Value).Wait();
            Assert.True(Cache.Shared.Hashes.Exists(this.Key, this.Value.ElementAt(0).Key).Result);

            Assert.True(Cache.Shared.Hashes.Remove(this.Key, this.Value.ElementAt(0).Key).Result);

            Assert.False(Cache.Shared.Hashes.Exists(this.Key, this.Value.ElementAt(0).Key).Result);
        }

        [Fact]
        public void GetString()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, this.Value.ElementAt(0).Value).Wait();
            string value = Cache.Shared.Hashes.GetString(this.Key, this.Value.ElementAt(0).Key).Value;

            Assert.Equal(this.Value.ElementAt(0).Value, value);
        }

        [Fact]
        public void GetLong()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Value, "2").Wait();
            long value = Cache.Shared.Hashes.GetInt64(this.Key, this.Value.ElementAt(0).Value).Value;

            Assert.Equal(2L, value);
        }

        [Fact]
        public void GetByteArray()
        {
            byte[] expected = Encoding.UTF8.GetBytes(this.Value.ElementAt(0).Value);
            Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, expected).Wait();
            byte[] value = Cache.Shared.Hashes.GetByteArray(this.Key, this.Value.ElementAt(0).Key).Value;

            Assert.Equal(expected, value);
        }

        [Fact]
        public void GetMultipleStrings()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();

            string[] actuals = Cache.Shared.Hashes.GetString(this.Key, this.Value.Keys.ToArray()).Value;

            int i = 0;
            foreach (string key in this.Value.Keys)
                Assert.Equal(this.Value[key], actuals[i++]);
        }

        [Fact]
        public void GetMultipleByteArray()
        {
            var value = this.KVPsByteArrays.First().Value;

            Cache.Shared.Hashes.Set(this.Key, value).Wait();

            byte[][] actuals = Cache.Shared.Hashes.GetByteArray(this.Key, value.Keys.ToArray()).Value;

            int i = 0;
            foreach (string key in value.Keys)
                Assert.Equal(value[key], actuals[i++]);
        }

        [Fact]
        public void GetAll()
        {
            var value = this.KVPsByteArrays.First().Value;

            Cache.Shared.Hashes.Set(this.Key, value).Wait();

            var actuals = Cache.Shared.Hashes.GetAll(this.Key).Result;

            int i = 0;
            foreach (var kvp in value)
            {
                Assert.Equal(kvp.Key, actuals.ElementAt(i).Key);
                Assert.Equal(kvp.Value, actuals.ElementAt(i).Value);
                ++i;
            }
        }

        [Fact]
        public void IncrementLong()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
            Assert.Equal(0, Cache.Shared.Hashes.GetInt64(this.Key, this.Value.ElementAt(0).Key).Value);

            Cache.Shared.Hashes.Increment(this.Key, this.Value.ElementAt(0).Key, 2).Wait();
            Assert.Equal(2, Cache.Shared.Hashes.GetInt64(this.Key, this.Value.ElementAt(0).Key).Value);
        }

        [Fact]
        public void IncrementDouble()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
            Assert.Equal(0.0, Cache.Shared.Hashes.GetDouble(this.Key, this.Value.ElementAt(0).Key).Value);

            Cache.Shared.Hashes.Increment(this.Key, this.Value.ElementAt(0).Key, 2.84d).Wait();
            Assert.Equal(2.84, Cache.Shared.Hashes.GetDouble(this.Key, this.Value.ElementAt(0).Key).Value);
        }

        [Fact]
        public void DecrementLong()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
            Assert.Equal(0, Cache.Shared.Hashes.GetInt64(this.Key, this.Value.ElementAt(0).Key).Value);

            Cache.Shared.Hashes.Decrement(this.Key, this.Value.ElementAt(0).Key, 2).Wait();
            Assert.Equal(-2, Cache.Shared.Hashes.GetInt64(this.Key, this.Value.ElementAt(0).Key).Value);
        }

        [Fact]
        public void DecrementDouble()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
            Assert.Equal(0.0, Cache.Shared.Hashes.GetDouble(this.Key, this.Value.ElementAt(0).Key).Value);

            Cache.Shared.Hashes.Decrement(this.Key, this.Value.ElementAt(0).Key, 2.84d).Wait();
            Assert.Equal(-2.84, Cache.Shared.Hashes.GetDouble(this.Key, this.Value.ElementAt(0).Key).Value);
        }

        [Fact]
        public void GetKeys()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
            string[] actual = Cache.Shared.Hashes.GetKeys(this.Key).Result;

            Assert.Equal(this.Value.Keys, actual);
        }

        [Fact]
        public void GetValues()
        {
            Cache.Shared.Hashes.Set(this.Key, this.KVPsByteArrays.First().Value).Wait();
            byte[][] actual = Cache.Shared.Hashes.GetValues(this.Key).Result;

            Assert.Equal(this.KVPsByteArrays.First().Value.Values, actual);
        }

        [Fact]
        public void GetLength()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
            Assert.Equal(this.Value.Count, Cache.Shared.Hashes.GetLength(this.Key).Result);
        }

        [Fact]
        public void SetString()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, this.Value.ElementAt(0).Value).Wait();
            string value = Cache.Shared.Hashes.GetString(this.Key, this.Value.ElementAt(0).Key).Value;

            Assert.Equal(this.Value.ElementAt(0).Value, value);
        }

        [Fact]
        public void SetByteArray()
        {
            byte[] expected = Encoding.UTF8.GetBytes(this.Value.ElementAt(0).Value);
            Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, expected).Wait();
            byte[] value = Cache.Shared.Hashes.GetByteArray(this.Key, this.Value.ElementAt(0).Key).Value;

            Assert.Equal(expected, value);
        }

        [Fact]
        public void SetMultipleStrings()
        {
            Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();

            string[] actuals = Cache.Shared.Hashes.GetString(this.Key, this.Value.Keys.ToArray()).Value;

            int i = 0;
            foreach (string key in this.Value.Keys)
                Assert.Equal(this.Value[key], actuals[i++]);
        }

        [Fact]
        public void SetMultipleByteArray()
        {
            var value = this.KVPsByteArrays.First().Value;

            Cache.Shared.Hashes.Set(this.Key, value).Wait();

            byte[][] actuals = Cache.Shared.Hashes.GetByteArray(this.Key, value.Keys.ToArray()).Value;

            int i = 0;
            foreach (string key in value.Keys)
                Assert.Equal(value[key], actuals[i++]);
        }

        [Fact]
        public void SetIfNotExistsString()
        {
            var kvp = this.Value.ElementAt(0);
            const string bg = "bubblegum";

            Assert.True(Cache.Shared.Hashes.SetIfNotExists(this.Key, kvp.Key, kvp.Value).Result);

            Assert.Equal(kvp.Value, Cache.Shared.Hashes.GetString(this.Key, kvp.Key).Value);

            Assert.False(Cache.Shared.Hashes.SetIfNotExists(this.Key, kvp.Key, bg).Result);
        }

        [Fact]
        public void SetIfNotExistsByteArray()
        {
            var kvp = this.Value.ElementAt(0);
            byte[] bytes = Encoding.UTF8.GetBytes(kvp.Value);
            byte[] bg = Encoding.UTF8.GetBytes("bubblegum");

            Assert.True(Cache.Shared.Hashes.SetIfNotExists(this.Key, kvp.Key, bytes).Result);

            Assert.Equal(kvp.Value, Cache.Shared.Hashes.GetString(this.Key, kvp.Key).Value);

            Assert.False(Cache.Shared.Hashes.SetIfNotExists(this.Key, kvp.Key, bg).Result);
        }
    }
}