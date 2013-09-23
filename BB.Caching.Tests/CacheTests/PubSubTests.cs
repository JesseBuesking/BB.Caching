using System;
using System.Threading;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class PubSubTests
    {
        public PubSubTests()
        {
            try
            {
                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27")));

                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

                Cache.PubSub.Configure(new SafeRedisConnection("192.168.2.27"));
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