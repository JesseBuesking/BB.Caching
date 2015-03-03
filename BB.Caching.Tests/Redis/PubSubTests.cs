namespace BB.Caching.Tests.Redis
{
    using System.Threading;

    using BB.Caching.Redis;

    using Xunit;

    internal class PubSubTests : IUseFixture<DefaultTestFixture>
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
    }
}