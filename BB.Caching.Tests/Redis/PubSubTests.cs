namespace BB.Caching.Tests.Redis
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using BB.Caching.Redis;
    using BB.Caching.Tests.Caching;

    using Xunit;

    public class PubSubTestsFixture : IDisposable
    {
        public PubSubTestsFixture()
        {
            try
            {
                Cache.Prepare();
            }
            catch (PubSub.ChannelAlreadySubscribedException)
            {
            }
        }

        public void Dispose()
        {
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleClass",
        Justification = "Reviewed. Suppression is OK here.")]
    internal class PubSubTests : IUseFixture<DefaultTestFixture>, IUseFixture<PubSubTestsFixture>
    {
        [Fact]
        public void SubscriptionIsCalled()
        {
            bool on = false;
            PubSub.SubscribeAsync(
                "mychannel",
                "a",
                async _ =>
                {
                    on = true;
                    await Cache.Config.GetAsync<ConfigTests.ConfigDummy>("a");
                });
            PubSub.PublishAsync("mychannel", "a", "test");

            for (int i = 0; i < 100 && !on; i++)
            {
                Thread.Sleep(20);
            }

            Assert.True(on);
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }

        public void SetFixture(PubSubTestsFixture data)
        {
        }
    }
}