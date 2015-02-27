using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BB.Caching.Caching;
using StackExchange.Redis;
using Xunit;

namespace BB.Caching.Tests.Caching.Shared
{
    public class KeysTests : IUseFixture<DefaultTestFixture>, IDisposable
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

        private RedisKey[] Keyz
        {
            get { return this._kvPs.Keys.Select(x => (RedisKey)x).ToArray(); }
        }

        private RedisValue Value
        {
            get { return this._kvPs.First().Value; }
        }

        public KeysTests()
        {
            Cache.Shared.Keys.DeleteAsync(this.Keyz).Wait();
            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.ExistsAsync(key).Result);
        }

        public void Dispose()
        {
            Cache.Shared.Keys.DeleteAsync(this.Keyz).Wait();
            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.ExistsAsync(key).Result);
        }

        [Fact]
        public void Delete()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value);
            Assert.True(Cache.Shared.Keys.Exists(this.Key));

            Assert.True(Cache.Shared.Keys.Delete(this.Key));
            Assert.False(Cache.Shared.Keys.Exists(this.Key));
        }

        [Fact]
        public void DeleteAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);

            Assert.True(Cache.Shared.Keys.DeleteAsync(this.Key).Result);
            Assert.False(Cache.Shared.Keys.ExistsAsync(this.Key).Result);
        }

        [Fact]
        public void DeleteMultiple()
        {
            foreach (var kvp in this._kvPs)
                Cache.Shared.Strings.Set(kvp.Key, kvp.Value);

            foreach (var key in this._kvPs.Keys)
                Assert.True(Cache.Shared.Keys.Exists(key));

            Assert.Equal(this._kvPs.Count, Cache.Shared.Keys.Delete(this.Keyz));

            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.Exists(key));
        }

        [Fact]
        public void DeleteMultipleAsync()
        {
            foreach (var kvp in this._kvPs)
                Cache.Shared.Strings.SetAsync(kvp.Key, kvp.Value).Wait();

            foreach (var key in this._kvPs.Keys)
                Assert.True(Cache.Shared.Keys.ExistsAsync(key).Result);

            Assert.Equal(this._kvPs.Count, Cache.Shared.Keys.DeleteAsync(this.Keyz).Result);

            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.ExistsAsync(key).Result);
        }

        [Fact]
        public void Exists()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value);
            Assert.True(Cache.Shared.Keys.Exists(this.Key));

            Assert.True(Cache.Shared.Keys.Delete(this.Key));
            Assert.False(Cache.Shared.Keys.Exists(this.Key));
        }

        [Fact]
        public void ExistsAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);

            Assert.True(Cache.Shared.Keys.DeleteAsync(this.Key).Result);
            Assert.False(Cache.Shared.Keys.ExistsAsync(this.Key).Result);
        }

        [Fact]
        public void Expire()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value);
            Assert.True(Cache.Shared.Keys.Exists(this.Key));

            Thread.Sleep(100);
            Assert.True(Cache.Shared.Keys.Exists(this.Key));

            Cache.Shared.Keys.Expire(this.Key, TimeSpan.FromSeconds(.5));
            Assert.True(Cache.Shared.Keys.Exists(this.Key));

            Thread.Sleep(400);
            Assert.True(Cache.Shared.Keys.Exists(this.Key));

            Thread.Sleep(150);
            Assert.False(Cache.Shared.Keys.Exists(this.Key));
        }

        [Fact]
        public void ExpireAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);

            Thread.Sleep(100);
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);

            Cache.Shared.Keys.ExpireAsync(this.Key, TimeSpan.FromSeconds(.5));
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);

            Thread.Sleep(400);
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);

            Thread.Sleep(150);
            Assert.False(Cache.Shared.Keys.ExistsAsync(this.Key).Result);
        }

        [Fact]
        public void Persist()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value);
            Assert.True(Cache.Shared.Keys.Exists(this.Key));

            Thread.Sleep(100);
            Assert.True(Cache.Shared.Keys.Exists(this.Key));

            Cache.Shared.Keys.Expire(this.Key, TimeSpan.FromSeconds(.5));
            Assert.True(Cache.Shared.Keys.Exists(this.Key));

            Thread.Sleep(400);
            Assert.True(Cache.Shared.Keys.Exists(this.Key));
            Assert.True(Cache.Shared.Keys.Persist(this.Key));

            Thread.Sleep(150);
            Assert.True(Cache.Shared.Keys.Exists(this.Key));
        }

        [Fact]
        public void PersistAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);

            Thread.Sleep(100);
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);

            Cache.Shared.Keys.ExpireAsync(this.Key, TimeSpan.FromSeconds(.5));
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);

            Thread.Sleep(400);
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);
            Assert.True(Cache.Shared.Keys.PersistAsync(this.Key).Result);

            Thread.Sleep(150);
            Assert.True(Cache.Shared.Keys.ExistsAsync(this.Key).Result);
        }

        // TODO FIX
        //[Fact]
        //public void Find()
        //{
        //    Cache.Shared.Strings.SetAsync(this.KVPs).Wait();
        //    string[] keys = Cache.Shared.Keys.Find("key").Result;

        //    Assert.Equal(0, keys.Length);

        //    keys = Cache.Shared.Keys.Find("key*").Result;

        //    Assert.Equal(4, keys.Length);
        //}

        [Fact(Skip = "Skipping")]
        public void Random()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value);

            int i = 0;
            while (true)
            {
                ++i;
                RedisKey key = Cache.Shared.Keys.Random();
                if (null == (string)key)
                {
                    if (1000 < i)
                        Assert.True(false, "didn't find the value");
                    continue;
                }
                Assert.Equal(this.Key, key);
                break;
            }
        }

        [Fact(Skip = "Skipping")]
        public void RandomAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();

            int i = 0;
            while (true)
            {
                ++i;
                RedisKey key = Cache.Shared.Keys.RandomAsync().Result;
                if (null == (string)key)
                {
                    if (1000 < i)
                        Assert.True(false, "didn't find the value");
                    continue;
                }
                Assert.Equal(this.Key, key);
                break;
            }
        }

        [Fact]
        public void TimeToLive()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value);
            Assert.Equal(null, Cache.Shared.Keys.TimeToLive(this.Key));
            Cache.Shared.Keys.Expire(this.Key, TimeSpan.FromSeconds(.5));

            TimeSpan? diff = TimeSpan.FromSeconds(.5) - Cache.Shared.Keys.TimeToLive(this.Key);
            Assert.True(diff < TimeSpan.FromSeconds(.01));
            Thread.Sleep(250);

            diff = TimeSpan.FromSeconds(.25) - Cache.Shared.Keys.TimeToLive(this.Key);
            Assert.True(diff < TimeSpan.FromSeconds(.01));
            Thread.Sleep(260);

            Assert.False(Cache.Shared.Keys.Exists(this.Key));
        }

        [Fact]
        public void TimeToLiveAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            Assert.Equal(null, Cache.Shared.Keys.TimeToLiveAsync(this.Key).Result);
            Cache.Shared.Keys.ExpireAsync(this.Key, TimeSpan.FromSeconds(.5)).Wait();

            TimeSpan? diff = TimeSpan.FromSeconds(.5) - Cache.Shared.Keys.TimeToLiveAsync(this.Key).Result;
            Assert.True(diff < TimeSpan.FromSeconds(.01));
            Thread.Sleep(250);

            diff = TimeSpan.FromSeconds(.25) - Cache.Shared.Keys.TimeToLiveAsync(this.Key).Result;
            Assert.True(diff < TimeSpan.FromSeconds(.01));
            Thread.Sleep(260);

            Assert.False(Cache.Shared.Keys.ExistsAsync(this.Key).Result);
        }

        [Fact]
        public void Type()
        {
            // TODO test the other types once they're implemented
            Cache.Shared.Strings.Set(this.Key, this.Value);
            Assert.Equal(RedisType.String, Cache.Shared.Keys.Type(this.Key));
        }

        [Fact]
        public void TypeAsync()
        {
            // TODO test the other types once they're implemented
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            Assert.Equal(RedisType.String, Cache.Shared.Keys.TypeAsync(this.Key).Result);
        }

        [Fact(Skip = "changes depending on tests")]
        public void GetLength()
        {
            // 1 b/c bb.cache.config
            Assert.Equal(1, Cache.Shared.Keys.GetLength());
            Cache.Shared.Strings.SetAsync(this.KVPs);
            Assert.Equal(this._kvPs.Count + 1, Cache.Shared.Keys.GetLength());
        }

        [Fact]
        public void DebugObject()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value);
            string result = Cache.Shared.Keys.DebugObject(this.Key);
            Assert.True(result.Contains("encoding:int serializedlength:2"));
        }

        [Fact]
        public void DebugObjectAsync()
        {
            Cache.Shared.Strings.SetAsync(this.Key, this.Value).Wait();
            string result = Cache.Shared.Keys.DebugObjectAsync(this.Key).Result;
            Assert.True(result.Contains("encoding:int serializedlength:2"));
        }

        [Fact]
        public void Invalidate()
        {
            Cache.Memory.Strings.Set(this.Key, (string)this.Value);

            MemoryValue<string> value = Cache.Memory.Strings.Get<string>(this.Key);
            Assert.True(value.Exists);
            Assert.Equal((string)this.Value, value.Value);

            long receivedBy = Cache.Shared.Keys.Invalidate(this.Key);
            Assert.Equal(1, receivedBy);

            value = Cache.Memory.Strings.Get<string>(this.Key);
            Assert.False(value.Exists);
        }

        [Fact]
        public void InvalidateAsync()
        {
            Cache.Memory.Strings.SetAsync(this.Key, (string)this.Value).Wait();

            MemoryValue<string> value = Cache.Memory.Strings.GetAsync<string>(this.Key).Result;
            Assert.True(value.Exists);
            Assert.Equal((string)this.Value, value.Value);

            long receivedBy = Cache.Shared.Keys.InvalidateAsync(this.Key).Result;
            Assert.Equal(1, receivedBy);

            value = Cache.Memory.Strings.GetAsync<string>(this.Key).Result;
            Assert.False(value.Exists);
        }

        [Fact]
        public void InvalidateMultiple()
        {
            foreach (var kvp in this._kvPs)
            {
                Cache.Memory.Strings.Set(kvp.Key, kvp.Value);
            }

            MemoryValue<string> value;
            foreach (var kvp in this._kvPs)
            {
                value = Cache.Memory.Strings.Get<string>(kvp.Key);
                Assert.True(value.Exists);
                Assert.Equal(kvp.Value, value.Value);
            }

            long receivedBy = Cache.Shared.Keys.Invalidate(this._kvPs.Keys.ToArray());
            Assert.Equal(1, receivedBy);

            foreach (var kvp in this._kvPs)
            {
                value = Cache.Memory.Strings.Get<string>(kvp.Key);
                Assert.False(value.Exists);
            }
        }

        [Fact]
        public void InvalidateMultipleAsync()
        {
            foreach (var kvp in this._kvPs)
            {
                Cache.Memory.Strings.SetAsync(kvp.Key, kvp.Value).Wait();
            }

            MemoryValue<string> value;
            foreach (var kvp in this._kvPs)
            {
                value = Cache.Memory.Strings.GetAsync<string>(kvp.Key).Result;
                Assert.True(value.Exists);
                Assert.Equal(kvp.Value, value.Value);
            }

            long receivedBy = Cache.Shared.Keys.InvalidateAsync(this._kvPs.Keys.ToArray()).Result;
            Assert.Equal(1, receivedBy);

            foreach (var kvp in this._kvPs)
            {
                value = Cache.Memory.Strings.GetAsync<string>(kvp.Key).Result;
                Assert.False(value.Exists);
            }
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }
    }
}