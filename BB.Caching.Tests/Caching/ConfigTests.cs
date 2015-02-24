using System;
using System.Threading;
using Xunit;

namespace BB.Caching.Tests.Caching
{
    public class ConfigTests : IUseFixture<DefaultTestFixture>, IUseFixture<ConfigTests.ConfigTestsFixture>, IDisposable
    {
        public class ConfigTestsFixture : IDisposable
        {
            public ConfigTestsFixture()
            {
                Cache.Prepare();

                Cache.Shared.Keys.Remove(KEY).Wait();
                Cache.Shared.Keys.Remove(KEY2).Wait();
            }

            public void Dispose()
            {
                Cache.Shared.Keys.Remove(KEY).Wait();
                Cache.Shared.Keys.Remove(KEY2).Wait();
            }
        }

        public class ConfigDummy
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

        private const string KEY = "ConfigTests.Key";

        private const string KEY2 = "ConfigTests.Key2";

        private const string VALUE = "ConfigTests.Value";

        public ConfigTests()
        {
            Cache.Shared.Keys.Remove(KEY);
            Cache.Shared.Keys.Remove(KEY2);

            Assert.False(Cache.Shared.Keys.Exists(KEY).Result);
            Assert.False(Cache.Shared.Keys.Exists(KEY2).Result);
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(KEY);
            Cache.Shared.Keys.Remove(KEY2);

            Assert.False(Cache.Shared.Keys.Exists(KEY).Result);
            Assert.False(Cache.Shared.Keys.Exists(KEY2).Result);
        }

        private readonly ConfigDummy _value = new ConfigDummy
            {
                One = 1,
                Two = 2
            };

        [Fact]
        public void Set()
        {
            Cache.Config.Set(KEY, VALUE);
            Thread.Sleep(200);

            var value = Cache.Config.Get<string>(KEY);
            Assert.Equal(VALUE, value);
            Cache.Config.Remove(KEY);

            value = Cache.Config.Get<string>(KEY);
            Assert.Equal(null, value);
        }

        [Fact]
        public void SetAsync()
        {
            Cache.Config.SetAsync(KEY, this._value, false).Wait();
            Cache.Config.SetAsync(KEY2, VALUE, false).Wait();

            ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(KEY).Result;
            string value2 = Cache.Config.GetAsync<string>(KEY2).Result;

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(VALUE, value2);

            Cache.Config.RemoveAsync(KEY2, false).Wait();
            value2 = Cache.Config.GetAsync<string>(KEY2).Result;
            Assert.Equal(null, value2);
        }

        [Fact]
        public void Get()
        {
            Cache.Config.Set(KEY, this._value, false);
            Cache.Config.Set(KEY2, VALUE, false);

            ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(KEY);
            string value2 = Cache.Config.Get<string>(KEY2);

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(VALUE, value2);

            Cache.Config.Remove(KEY2, false);
            value2 = Cache.Config.Get<string>(KEY2);
            Assert.Equal(null, value2);
        }

        [Fact]
        public void GetAsync()
        {
            Cache.Config.SetAsync(KEY, this._value, false).Wait();
            Cache.Config.SetAsync(KEY2, VALUE, false).Wait();

            ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(KEY).Result;
            string value2 = Cache.Config.GetAsync<string>(KEY2).Result;

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(VALUE, value2);

            Cache.Config.RemoveAsync(KEY2, false).Wait();
            value2 = Cache.Config.GetAsync<string>(KEY2).Result;
            Assert.Equal(null, value2);
        }

        [Fact]
        public void Remove()
        {
            Cache.Config.Set(KEY, this._value, false);
            Cache.Config.Set(KEY2, VALUE, false);

            ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(KEY);
            string value2 = Cache.Config.Get<string>(KEY2);

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(VALUE, value2);

            Cache.Config.Remove(KEY2, false);
            value2 = Cache.Config.Get<string>(KEY2);
            Assert.Equal(null, value2);
        }

        [Fact]
        public void RemoveAsync()
        {
            Cache.Config.SetAsync(KEY, this._value, false).Wait();
            Cache.Config.SetAsync(KEY2, VALUE, false).Wait();

            ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(KEY).Result;
            string value2 = Cache.Config.GetAsync<string>(KEY2).Result;

            Assert.Equal(this._value.One, configDummy.One);
            Assert.Equal(this._value.Two, configDummy.Two);
            Assert.Equal(VALUE, value2);

            Cache.Config.RemoveAsync(KEY2, false).Wait();
            value2 = Cache.Config.GetAsync<string>(KEY2).Result;
            Assert.Equal(null, value2);
        }

        public void SetFixture(ConfigTestsFixture data)
        {
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }
    }
}