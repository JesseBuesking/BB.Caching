using System;
using System.Threading;
using BB.Caching.Redis;
using BB.Caching.Tests.Caching;
using Xunit;

namespace BB.Caching.Tests.Redis
{
    public class PubSubTestsFixture : IDisposable
    {
        public PubSubTestsFixture()
        {
            try
            {
                Cache.Prepare();
            }
            catch (PubSub.ChannelAlreadySubscribedException)
            { }
        }

        public void Dispose()
        {
        }
    }

    public class PubSubTests : IUseFixture<DefaultTestFixture>, IUseFixture<PubSubTestsFixture>
    {
        [Fact]
        public void SubscriptionIsCalled()
        {
            bool isSet = false;
            PubSub.SubscribeAsync("mychannel", "a", async _ =>
                {
                    isSet = true;
                    await Cache.Config.GetAsync<ConfigTests.ConfigDummy>("a");
                });
            PubSub.PublishAsync("mychannel", "a", "test");

            for (int i = 0; i < 100 && !isSet; i++)
                Thread.Sleep(20);

            Assert.True(isSet);
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }

        public void SetFixture(PubSubTestsFixture data)
        {
        }
    }
}