using System;
using System.Threading;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class ConfigTests : IDisposable
    {
// ReSharper disable MemberCanBePrivate.Global
        public class ConfigDummy
// ReSharper restore MemberCanBePrivate.Global
        {
            public int One
            {
                get;
                set;
            }

            public int Two
            {
                get;
                set;
            }
        }

        private const string _key = "key1";

        private readonly ConfigDummy _value = new ConfigDummy
            {
                One = 1,
                Two = 2
            };

        private const string _key2 = "key2";

        private const string _value2 = "value2";

        public ConfigTests()
        {
            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27")));

            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

            Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27"));

            Cache.Prepare();

            Cache.Shared.Keys.Remove(_key).Wait();
            Cache.Shared.Keys.Remove(_key2).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(_key).Wait();
            Cache.Shared.Keys.Remove(_key2).Wait();
        }

        [Fact]
        public void Set()
        {
            bool isSet = false;
            ConfigDummy value = null;
            Cache.Config.SubscribeChange(_key, async () =>
                {
                    isSet = true;
                    value = await Cache.Config.GetAsync<ConfigDummy>(_key);
                });
            Cache.Config.Set(_key, this._value);

            while (!isSet)
                Thread.Sleep(100);

            Assert.True(isSet);
            Assert.Equal(this._value.One, value.One);
            Assert.Equal(this._value.Two, value.Two);

            Cache.Config.Set(_key2, _value2, false);

            ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(_key);
            string value2 = Cache.Config.Get<string>(_key2);

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(_value2, value2);

            Cache.Config.Remove(_key2, false);
            value2 = Cache.Config.Get<string>(_key2);
            Assert.Equal(null, value2);
        }

        [Fact]
        public void SetAsync()
        {
            Cache.Config.SetAsync(_key, this._value, false).Wait();
            Cache.Config.SetAsync(_key2, _value2, false).Wait();

            ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(_key).Result;
            string value2 = Cache.Config.GetAsync<string>(_key2).Result;

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(_value2, value2);

            Cache.Config.RemoveAsync(_key2, false).Wait();
            value2 = Cache.Config.GetAsync<string>(_key2).Result;
            Assert.Equal(null, value2);
        }

        [Fact]
        public void Get()
        {
            Cache.Config.Set(_key, this._value, false);
            Cache.Config.Set(_key2, _value2, false);

            ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(_key);
            string value2 = Cache.Config.Get<string>(_key2);

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(_value2, value2);

            Cache.Config.Remove(_key2, false);
            value2 = Cache.Config.Get<string>(_key2);
            Assert.Equal(null, value2);
        }

        [Fact]
        public void GetAsync()
        {
            Cache.Config.SetAsync(_key, this._value, false).Wait();
            Cache.Config.SetAsync(_key2, _value2, false).Wait();

            ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(_key).Result;
            string value2 = Cache.Config.GetAsync<string>(_key2).Result;

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(_value2, value2);

            Cache.Config.RemoveAsync(_key2, false).Wait();
            value2 = Cache.Config.GetAsync<string>(_key2).Result;
            Assert.Equal(null, value2);
        }

        [Fact]
        public void Remove()
        {
            Cache.Config.Set(_key, this._value, false);
            Cache.Config.Set(_key2, _value2, false);

            ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(_key);
            string value2 = Cache.Config.Get<string>(_key2);

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(_value2, value2);

            Cache.Config.Remove(_key2, false);
            value2 = Cache.Config.Get<string>(_key2);
            Assert.Equal(null, value2);
        }

        [Fact]
        public void RemoveAsync()
        {
            Cache.Config.SetAsync(_key, this._value, false).Wait();
            Cache.Config.SetAsync(_key2, _value2, false).Wait();

            ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(_key).Result;
            string value2 = Cache.Config.GetAsync<string>(_key2).Result;

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(_value2, value2);

            Cache.Config.RemoveAsync(_key2, false).Wait();
            value2 = Cache.Config.GetAsync<string>(_key2).Result;
            Assert.Equal(null, value2);
        }
    }
}