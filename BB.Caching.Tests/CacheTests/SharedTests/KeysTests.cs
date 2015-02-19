using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests.SharedTests
{
    public class KeysTests : TestBase
    {
        private readonly Dictionary<string, string> _kvPs = new Dictionary<string, string>
            {
                {"key1", "0"},
                {"key2", "1"},
                {"key003", "2"},
                {"key4", "3"}
            };

        private string Key
        {
            get { return this._kvPs.First().Key; }
        }

        private string[] Keyz
        {
            get { return this._kvPs.Keys.ToArray(); }
        }

        private string Value
        {
            get { return this._kvPs.First().Value; }
        }

        public KeysTests()
        {
            try
            {
                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-0", new SafeRedisConnection(this.TestIp, 6379,
                        // Needed for DebugObject
                        allowAdmin: true)));

                if (0 != this.TestPort2)
                {
                    Cache.Shared.AddRedisConnectionGroup(
                        new RedisConnectionGroup("node-1", new SafeRedisConnection(this.TestIp, this.TestPort2)));
                }

                Cache.PubSub.Configure(new SafeRedisConnection(this.TestIp));
                Cache.Shared.SetPubSubRedisConnection();
            }
            catch (Exception)
            {
            }

            Cache.Shared.Keys.Remove(this.Keyz).Wait();
            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.Exists(key).Result);
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(this.Keyz).Wait();
            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.Exists(key).Result);
        }

        [Fact]
        public void Remove()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

            Assert.True(Cache.Shared.Keys.Remove(this.Key).Result);
            Assert.False(Cache.Shared.Keys.Exists(this.Key).Result);
        }

        [Fact]
        public void RemoveMultiple()
        {
            foreach (var kvp in this._kvPs)
                Cache.Shared.Strings.Set(kvp.Key, kvp.Value).Wait();

            foreach (var key in this._kvPs.Keys)
                Assert.True(Cache.Shared.Keys.Exists(key).Result);

            Assert.Equal(this._kvPs.Count, Cache.Shared.Keys.Remove(this.Keyz).Result);

            foreach (var key in this._kvPs.Keys)
                Assert.False(Cache.Shared.Keys.Exists(key).Result);
        }

        [Fact]
        public void Exists()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

            Assert.True(Cache.Shared.Keys.Remove(this.Key).Result);
            Assert.False(Cache.Shared.Keys.Exists(this.Key).Result);
        }

        [Fact]
        public void Expire()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

            Thread.Sleep(1000);
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

            Cache.Shared.Keys.Expire(this.Key, TimeSpan.FromSeconds(2));
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

            Thread.Sleep(1000);
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

            Thread.Sleep(1001);
            Assert.False(Cache.Shared.Keys.Exists(this.Key).Result);
        }

        [Fact]
        public void Persist()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

            Thread.Sleep(1000);
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

            Cache.Shared.Keys.Expire(this.Key, TimeSpan.FromSeconds(2));
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

            Thread.Sleep(1000);
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

            Assert.True(Cache.Shared.Keys.Persist(this.Key).Result);

            Thread.Sleep(1001);
            Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);
        }

        [Fact]
        public void Find()
        {
            Cache.Shared.Strings.Set(this._kvPs).Wait();
            string[] keys = Cache.Shared.Keys.Find("key").Result;

            Assert.Equal(0, keys.Length);

            keys = Cache.Shared.Keys.Find("key*").Result;

            Assert.Equal(4, keys.Length);
        }

        [Fact(Skip = "Skipping")]
        public void Random()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();

            int i = 0;
            while (true)
            {
                ++i;
                string key = Cache.Shared.Keys.Random().Result;
                if (null == key)
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
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            Assert.Equal(-1, Cache.Shared.Keys.TimeToLive(this.Key).Result);
            Cache.Shared.Keys.Expire(this.Key, TimeSpan.FromSeconds(3)).Wait();
            Assert.Equal(3, Cache.Shared.Keys.TimeToLive(this.Key).Result);
            Thread.Sleep(2000);
            Assert.Equal(1, Cache.Shared.Keys.TimeToLive(this.Key).Result);
            Thread.Sleep(1100);
            Assert.False(Cache.Shared.Keys.Exists(this.Key).Result);
        }

        [Fact]
        public void Type()
        {
            // TODO test the other types once they're implemented
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            Assert.Equal("string", Cache.Shared.Keys.Type(this.Key).Result);
        }

        [Fact(Skip = "changes depending on tests")]
        public void GetLength()
        {
            // 1 b/c bb.cache.config
            Assert.Equal(1, Cache.Shared.Keys.GetLength().Result);
            Cache.Shared.Strings.Set(this._kvPs);
            Assert.Equal(this._kvPs.Count + 1, Cache.Shared.Keys.GetLength().Result);
        }

        [Fact]
        public void DebugObject()
        {
            Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
            string result = Cache.Shared.Keys.DebugObject(this.Key).Result;
            Assert.True(result.Contains("encoding:int serializedlength:2"));
        }

        [Fact]
        public void Invalidate()
        {
            Cache.Memory.Set(this.Key, this.Value);

            string value;
            Assert.True(Cache.Memory.TryGet(this.Key, out value));
            Assert.Equal(this.Value, value);

#pragma warning disable 168
            long receivedBy = Cache.Shared.Keys.Invalidate(this.Key).Result;
#pragma warning restore 168

            Assert.False(Cache.Memory.TryGet(this.Key, out value));
            //Assert.Equal(1, receivedBy);
        }

        [Fact]
        public void InvalidateMultiple()
        {
            foreach (var kvp in this._kvPs)
                Cache.Memory.Set(kvp.Key, kvp.Value);

            string value;
            foreach (var kvp in this._kvPs)
            {
                Assert.True(Cache.Memory.TryGet(kvp.Key, out value));
                Assert.Equal(kvp.Value, value);
            }

#pragma warning disable 168
            long receivedBy = Cache.Shared.Keys.Invalidate(this.Keyz).Result;
#pragma warning restore 168
            //Assert.Equal(1, receivedBy);

            foreach (var kvp in this._kvPs)
                Assert.False(Cache.Memory.TryGet(kvp.Key, out value));
        }
    }
}