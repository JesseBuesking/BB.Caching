using System.Threading;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class ConfigTests
    {
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

        public string Key1 = "key1";

        public ConfigDummy Value1 = new ConfigDummy
            {
                One = 1,
                Two = 2
            };

        public string Key2 = "key2";

        public string Value2 = "value2";

        public ConfigTests()
        {
            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

            Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27", 6379));

            Cache.Config.Prepare();

            Cache.Shared.Keys.Remove(this.Key1).Wait();
            Cache.Shared.Keys.Remove(this.Key2).Wait();
        }

        public void Dispose()
        {
            Cache.Shared.Keys.Remove(this.Key1).Wait();
            Cache.Shared.Keys.Remove(this.Key2).Wait();
        }

        [Fact]
        public void Set()
        {
            bool isSet = false;
            ConfigDummy value = null;
            Cache.Config.SubscribeChange(this.Key1, async () =>
                {
                    isSet = true;
                    value = await Cache.Config.GetAsync<ConfigDummy>(this.Key1);
                });
            Cache.Config.Set(this.Key1, this.Value1);

            while (!isSet)
                Thread.Sleep(100);

            Assert.True(isSet);
            Assert.Equal(this.Value1.One, value.One);
            Assert.Equal(this.Value1.Two, value.Two);

            Cache.Config.Set(this.Key2, this.Value2, false);

            ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(this.Key1);
            string value2 = Cache.Config.Get<string>(this.Key2);

            Assert.Equal(this.Value1.One, configDummy.One);
            Assert.Equal(this.Value1.Two, configDummy.Two);
            Assert.Equal(this.Value2, value2);

            Cache.Config.Remove(this.Key2, false);
            value2 = Cache.Config.Get<string>(this.Key2);
            Assert.Equal(null, value2);
        }

        [Fact]
        public void SetAsync()
        {
            Cache.Config.SetAsync(this.Key1, this.Value1, false).Wait();
            Cache.Config.SetAsync(this.Key2, this.Value2, false).Wait();

            ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(this.Key1).Result;
            string value2 = Cache.Config.GetAsync<string>(this.Key2).Result;

            Assert.Equal(this.Value1.One, configDummy.One);
            Assert.Equal(this.Value1.Two, configDummy.Two);
            Assert.Equal(this.Value2, value2);

            Cache.Config.RemoveAsync(this.Key2, false).Wait();
            value2 = Cache.Config.GetAsync<string>(this.Key2).Result;
            Assert.Equal(null, value2);
        }

        [Fact]
        public void Get()
        {
            Cache.Config.Set(this.Key1, this.Value1, false);
            Cache.Config.Set(this.Key2, this.Value2, false);

            ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(this.Key1);
            string value2 = Cache.Config.Get<string>(this.Key2);

            Assert.Equal(this.Value1.One, configDummy.One);
            Assert.Equal(this.Value1.Two, configDummy.Two);
            Assert.Equal(this.Value2, value2);

            Cache.Config.Remove(this.Key2, false);
            value2 = Cache.Config.Get<string>(this.Key2);
            Assert.Equal(null, value2);
        }

        [Fact]
        public void GetAsync()
        {
            Cache.Config.SetAsync(this.Key1, this.Value1, false).Wait();
            Cache.Config.SetAsync(this.Key2, this.Value2, false).Wait();

            ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(this.Key1).Result;
            string value2 = Cache.Config.GetAsync<string>(this.Key2).Result;

            Assert.Equal(this.Value1.One, configDummy.One);
            Assert.Equal(this.Value1.Two, configDummy.Two);
            Assert.Equal(this.Value2, value2);

            Cache.Config.RemoveAsync(this.Key2, false).Wait();
            value2 = Cache.Config.GetAsync<string>(this.Key2).Result;
            Assert.Equal(null, value2);
        }

        [Fact]
        public void Remove()
        {
            Cache.Config.Set(this.Key1, this.Value1, false);
            Cache.Config.Set(this.Key2, this.Value2, false);

            ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(this.Key1);
            string value2 = Cache.Config.Get<string>(this.Key2);

            Assert.Equal(this.Value1.One, configDummy.One);
            Assert.Equal(this.Value1.Two, configDummy.Two);
            Assert.Equal(this.Value2, value2);

            Cache.Config.Remove(this.Key2, false);
            value2 = Cache.Config.Get<string>(this.Key2);
            Assert.Equal(null, value2);
        }

        [Fact]
        public void RemoveAsync()
        {
            Cache.Config.SetAsync(this.Key1, this.Value1, false).Wait();
            Cache.Config.SetAsync(this.Key2, this.Value2, false).Wait();

            ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(this.Key1).Result;
            string value2 = Cache.Config.GetAsync<string>(this.Key2).Result;

            Assert.Equal(this.Value1.One, configDummy.One);
            Assert.Equal(this.Value1.Two, configDummy.Two);
            Assert.Equal(this.Value2, value2);

            Cache.Config.RemoveAsync(this.Key2, false).Wait();
            value2 = Cache.Config.GetAsync<string>(this.Key2).Result;
            Assert.Equal(null, value2);
        }
    }
}