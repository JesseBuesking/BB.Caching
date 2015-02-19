using System;
using System.Threading;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class PubSubTests : TestBase
    {
        public PubSubTests()
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
        }

        [Fact]
        public void SubscriptionIsCalled()
        {
            bool isSet = false;
            Cache.PubSub.Subscribe("mychannel", "a", async _ =>
                {
                    isSet = true;
                    await Cache.Config.GetAsync<ConfigTests.ConfigDummy>("a");
                });
            Cache.PubSub.Publish("mychannel", "a", "test");

            for (int i = 0; i < 100 && !isSet; i++)
                Thread.Sleep(20);

            Assert.True(isSet);
        }
    }
}