namespace BB.Caching.Tests.Redis
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using BB.Caching.Redis;

    using Xunit;

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
            PubSub.SubscribeAsync("mychannel", "a", _ => { on = true; });
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