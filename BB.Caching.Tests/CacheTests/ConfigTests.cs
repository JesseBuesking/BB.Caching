using System;
using System.Threading;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class ConfigTests : TestBase
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
            try
            {
                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-0", new SafeRedisConnection(this.TestIp, this.TestPort1)));

                if (0 != this.TestPort2)
                {
                    Cache.Shared.AddRedisConnectionGroup(
                        new RedisConnectionGroup("node-1", new SafeRedisConnection(this.TestIp, this.TestPort2)));
                }

                Cache.PubSub.Configure(new SafeRedisConnection(this.TestIp, this.TestPort1));
                Cache.Shared.SetPubSubRedisConnection();

                Cache.Prepare();
            }
            catch (Exception)
            {
            }

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
            Cache.Config.Set("imatest", "hello");
            Thread.Sleep(200);

            var value = Cache.Config.Get<string>("imatest");
            Assert.Equal("hello", value);
            Cache.Config.Remove("imatest", true);

            value = Cache.Config.Get<string>("imatest");
            Assert.Equal(null, value);
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