using System;
using System.Threading;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class PubSubTestsFixture : IDisposable
    {
        public PubSubTestsFixture()
        {
            Cache.Prepare();
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

        public void SetFixture(DefaultTestFixture data)
        {
        }

        public void SetFixture(PubSubTestsFixture data)
        {
        }
    }
}