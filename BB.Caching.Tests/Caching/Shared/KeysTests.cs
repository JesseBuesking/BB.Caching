namespace BB.Caching.Tests.Caching.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using StackExchange.Redis;

    using Xunit;

    public class KeysTests : IUseFixture<DefaultTestFixture>, IDisposable
    {
        private readonly Dictionary<string, string> _keyValuePairs = new Dictionary<string, string>
            {
                { "key1", "0" },
                { "key2", "1" },
                { "key003", "2" },
                { "key4", "3" }
            };

        public KeysTests()
        {
            Cache.Shared.Keys.DeleteAsync(this.Keyz).Wait();
            foreach (var key in this._keyValuePairs.Keys)
            {
                Assert.False(Cache.Shared.Keys.ExistsAsync(key).Result);
            }
        }

        private Dictionary<RedisKey, RedisValue> KVPs
        {
            get { return this._keyValuePairs.ToDictionary(x => (RedisKey)x.Key, x => (RedisValue)x.Value); }
        }

        private RedisKey Key
        {
            get { return this._keyValuePairs.First().Key; }
        }

        private RedisKey[] Keyz
        {
            get { return this._keyValuePairs.Keys.Select(x => (RedisKey)x).ToArray(); }
        }

        private RedisValue Value
        {
            get { return this._keyValuePairs.First().Value; }
        }

        public void Dispose()
        {
            Cache.Shared.Keys.DeleteAsync(this.Keyz).Wait();
            foreach (var key in this._keyValuePairs.Keys)
            {
                Assert.False(Cache.Shared.Keys.ExistsAsync(key).Result);
            }
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
            foreach (var kvp in this._keyValuePairs)
            {
                Cache.Shared.Strings.Set(kvp.Key, kvp.Value);
            }

            foreach (var key in this._keyValuePairs.Keys)
            {
                Assert.True(Cache.Shared.Keys.Exists(key));
            }

            Assert.Equal(this._keyValuePairs.Count, Cache.Shared.Keys.Delete(this.Keyz));

            foreach (var key in this._keyValuePairs.Keys)
            {
                Assert.False(Cache.Shared.Keys.Exists(key));
            }
        }

        [Fact]
        public void DeleteMultipleAsync()
        {
            foreach (var kvp in this._keyValuePairs)
            {
                Cache.Shared.Strings.SetAsync(kvp.Key, kvp.Value).Wait();
            }

            foreach (var key in this._keyValuePairs.Keys)
            {
                Assert.True(Cache.Shared.Keys.ExistsAsync(key).Result);
            }

            Assert.Equal(this._keyValuePairs.Count, Cache.Shared.Keys.DeleteAsync(this.Keyz).Result);

            foreach (var key in this._keyValuePairs.Keys)
            {
                Assert.False(Cache.Shared.Keys.ExistsAsync(key).Result);
            }
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
                    {
                        Assert.True(false, "didn't find the value");
                    }

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
                    {
                        Assert.True(false, "didn't find the value");
                    }

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
            Assert.Equal(this._keyValuePairs.Count + 1, Cache.Shared.Keys.GetLength());
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

        public void SetFixture(DefaultTestFixture data)
        {
        }
    }
}