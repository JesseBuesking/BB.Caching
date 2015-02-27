using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackExchange.Redis;
using Xunit;

namespace BB.Caching.Tests.Caching.Shared
{
    public class HashesTests : IUseFixture<DefaultTestFixture>, IDisposable
    {
        private readonly Dictionary<string, Dictionary<string, string>> _kvPs =
            new Dictionary<string, Dictionary<string, string>>
                {
                    {"key1", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                    {"key2", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                    {"key003", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                    {"key4", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                };

        private Dictionary<string, HashEntry[]> KVPsByteArrays
        {
            get
            {
                return this._kvPs.ToDictionary(
                    k => k.Key,
                    v => v.Value.Select(w => new HashEntry(
                        w.Key,
                        Encoding.UTF8.GetBytes(w.Value)
                    )).ToArray()
                );
            }
        }

        private string Key
        {
            get { return this._kvPs.First().Key; }
        }

        private RedisKey[] Keys
        {
            get { return this._kvPs.Keys.Select(x => (RedisKey)x).ToArray(); }
        }

        private HashEntry[] Value
        {
            get { return this.KVPsByteArrays.First().Value; }
        }

        public HashesTests()
        {
            Cache.Shared.Keys.DeleteAsync(this.Keys).Wait();
            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.ExistsAsync(key).Result);
        }

        public void Dispose()
        {
            Cache.Shared.Keys.DeleteAsync(this.Keys).Wait();
            Cache.Shared.Keys.DeleteAsync(this.Keys).Wait();
            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.ExistsAsync(key).Result);
        }

        [Fact]
        public void Remove()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value.ElementAt(0).Name, this.Value.ElementAt(0).Value).Wait();
            Assert.True(Cache.Shared.Hashes.DeleteAsync(this.Key, this.Value.ElementAt(0).Name).Result);
            Assert.False(Cache.Shared.Hashes.DeleteAsync(this.Key, this.Value.ElementAt(0).Name).Result);
        }

        [Fact]
        public void RemoveMultiple()
        {
            var first = this.KVPsByteArrays.First();

            Cache.Shared.Hashes.SetAsync(first.Key, first.Value).Wait();
            Assert.Equal(2, Cache.Shared.Hashes.DeleteAsync(first.Key, first.Value.Select(x => x.Name).ToArray()).Result);
            Assert.Equal(0, Cache.Shared.Hashes.DeleteAsync(first.Key, first.Value.Select(x => x.Name).ToArray()).Result);
        }

        [Fact]
        public void Exists()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value.ElementAt(0).Name, this.Value.ElementAt(0).Value).Wait();
            Assert.True(Cache.Shared.Hashes.ExistsAsync(this.Key, this.Value.ElementAt(0).Name).Result);

            Assert.True(Cache.Shared.Hashes.DeleteAsync(this.Key, this.Value.ElementAt(0).Name).Result);

            Assert.False(Cache.Shared.Hashes.ExistsAsync(this.Key, this.Value.ElementAt(0).Name).Result);
        }

        [Fact]
        public void GetString()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value.ElementAt(0).Name, this.Value.ElementAt(0).Value).Wait();
            RedisValue value = Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result;

            Assert.Equal(this.Value.ElementAt(0).Value, value);
        }

        [Fact]
        public void GetLong()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value.ElementAt(0).Value, "2").Wait();
            RedisValue value = Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Value).Result;

            Assert.Equal(2L, value);
        }

        [Fact]
        public void GetByteArray()
        {
            byte[] expected = Encoding.UTF8.GetBytes(this.Value.ElementAt(0).Value);
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value.ElementAt(0).Name, expected).Wait();
            byte[] value = Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result;

            Assert.Equal(expected, value);
        }

        [Fact]
        public void GetMultipleStrings()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value).Wait();

            RedisValue[] actuals = Cache.Shared.Hashes.GetAsync(this.Key, this.Value.Select(x => x.Name).ToArray()).Result;

            int i = 0;
            foreach (object value in this.Value.Select(x => x.Value))
                Assert.Equal(value, actuals[i++]);
        }

        [Fact]
        public void GetMultipleByteArray()
        {
            var value = this.KVPsByteArrays.First().Value;

            Cache.Shared.Hashes.SetAsync(this.Key, value).Wait();

            RedisValue[] actuals = Cache.Shared.Hashes.GetAsync(this.Key, value.Select(x => x.Name).ToArray()).Result;

            int i = 0;
            foreach (HashEntry key in value)
                Assert.Equal(key.Value, actuals[i++]);
        }

        [Fact]
        public void GetAll()
        {
            var value = this.KVPsByteArrays.First().Value;

            Cache.Shared.Hashes.SetAsync(this.Key, value).Wait();

            var actuals = Cache.Shared.Hashes.GetAllAsync(this.Key).Result;

            int i = 0;
            foreach (var kvp in value)
            {
                Assert.Equal(kvp.Name, actuals.ElementAt(i).Name);
                Assert.Equal(kvp.Value, actuals.ElementAt(i).Value);
                ++i;
            }
        }

        [Fact]
        public void IncrementLong()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value).Wait();
            Assert.Equal(0, Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result);

            Cache.Shared.Hashes.IncrementAsync(this.Key, this.Value.ElementAt(0).Name, 2).Wait();
            Assert.Equal(2, Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result);
        }

        [Fact]
        public void IncrementDouble()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value).Wait();
            double value = (double) Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result;
            Assert.Equal(0.0, value);

            Cache.Shared.Hashes.IncrementAsync(this.Key, this.Value.ElementAt(0).Name, 2.84d).Wait();
            value = (double) Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result;
            Assert.Equal(2.84, value);
        }

        [Fact]
        public void DecrementLong()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value).Wait();
            Assert.Equal(0, Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result);

            Cache.Shared.Hashes.DecrementAsync(this.Key, this.Value.ElementAt(0).Name, 2).Wait();
            Assert.Equal(-2, Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result);
        }

        [Fact]
        public void DecrementDouble()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value).Wait();
            double value = (double) Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result;
            Assert.Equal(0.0, value);

            Cache.Shared.Hashes.DecrementAsync(this.Key, this.Value.ElementAt(0).Name, 2.84d).Wait();
            value = (double) Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result;
            Assert.Equal(-2.84, value);
        }

        [Fact]
        public void GetKeys()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value).Wait();
            RedisValue[] actual = Cache.Shared.Hashes.GetKeysAsync(this.Key).Result;

            Assert.Equal(this.Value.Select(x => x.Name), actual);
        }

        [Fact]
        public void GetValues()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.KVPsByteArrays.First().Value).Wait();
            RedisValue[] actual = Cache.Shared.Hashes.GetValuesAsync(this.Key).Result;

            Assert.Equal(this.KVPsByteArrays.First().Value.Select(x => x.Value), actual);
        }

        [Fact]
        public void GetLength()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value).Wait();
            Assert.Equal(this.Value.Length, Cache.Shared.Hashes.GetLengthAsync(this.Key).Result);
        }

        [Fact]
        public void SetString()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value.ElementAt(0).Name, this.Value.ElementAt(0).Value).Wait();
            RedisValue value = Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result;

            Assert.Equal(this.Value.ElementAt(0).Value, value);
        }

        [Fact]
        public void SetByteArray()
        {
            byte[] expected = Encoding.UTF8.GetBytes(this.Value.ElementAt(0).Value);
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value.ElementAt(0).Name, expected).Wait();
            byte[] value = Cache.Shared.Hashes.GetAsync(this.Key, this.Value.ElementAt(0).Name).Result;

            Assert.Equal(expected, value);
        }

        [Fact]
        public void SetMultipleStrings()
        {
            Cache.Shared.Hashes.SetAsync(this.Key, this.Value).Wait();

            RedisValue[] actuals = Cache.Shared.Hashes.GetAsync(this.Key, this.Value.Select(x => x.Name).ToArray()).Result;

            int i = 0;
            foreach (HashEntry entry in this.Value)
                Assert.Equal(entry.Value, actuals[i++]);
        }

        [Fact]
        public void SetMultipleByteArray()
        {
            var value = this.KVPsByteArrays.First().Value;

            Cache.Shared.Hashes.SetAsync(this.Key, value).Wait();

            RedisValue[] actuals = Cache.Shared.Hashes.GetAsync(this.Key, value.Select(x => x.Name).ToArray()).Result;

            int i = 0;
            foreach (HashEntry entry in value)
                Assert.Equal(entry.Value, actuals[i++]);
        }

        [Fact]
        public void SetIfNotExistsString()
        {
            var kvp = this.Value.ElementAt(0);
            const string bg = "bubblegum";

            Assert.True(Cache.Shared.Hashes.SetIfNotExistsAsync(this.Key, kvp.Name, kvp.Value).Result);

            Assert.Equal(kvp.Value, Cache.Shared.Hashes.GetAsync(this.Key, kvp.Name).Result);

            Assert.False(Cache.Shared.Hashes.SetIfNotExistsAsync(this.Key, kvp.Name, bg).Result);
        }

        [Fact]
        public void SetIfNotExistsByteArray()
        {
            var kvp = this.Value.ElementAt(0);
            byte[] bytes = Encoding.UTF8.GetBytes(kvp.Value);
            byte[] bg = Encoding.UTF8.GetBytes("bubblegum");

            Assert.True(Cache.Shared.Hashes.SetIfNotExistsAsync(this.Key, kvp.Name, bytes).Result);

            Assert.Equal(kvp.Value, Cache.Shared.Hashes.GetAsync(this.Key, kvp.Name).Result);

            Assert.False(Cache.Shared.Hashes.SetIfNotExistsAsync(this.Key, kvp.Name, bg).Result);
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }
    }
}