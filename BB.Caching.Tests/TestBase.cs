namespace BB.Caching.Tests
{
    using System;

    using BB.Caching.Redis;

    using StackExchange.Redis;

    public class DefaultTestFixture : IDisposable
    {
        public DefaultTestFixture()
        {
            SharedCache.Instance.AddRedisConnectionGroup(new ConnectionGroup(
                "node-0",
                ConnectionMultiplexer.Connect(string.Format("{0}:{1},allowAdmin=True", this.TestIp, this.TestPort1))));

            if (0 != this.TestPort2)
            {
                SharedCache.Instance.AddRedisConnectionGroup(new ConnectionGroup(
                    "node-1",
                    ConnectionMultiplexer.Connect(string.Format("{0}:{1}", this.TestIp, this.TestPort1))));
            }

            PubSub.Configure(ConnectionMultiplexer.Connect(string.Format("{0}:{1}", this.TestIp, this.TestPort1)));

            try
            {
                SharedCache.Instance.SetPubSubRedisConnection();
            }
            catch (Exception ex)
            {
                if (!ex.ToString().Contains("subscription to channel cache/invalidate already exists"))
                {
                    throw;
                }
            }
        }

        private string TestIp
        {
            get { return "127.0.0.1"; }
        }

        private int TestPort1
        {
            get { return 6379; }
        }

        private int TestPort2
        {
            get { return 6380; }
        }

        public virtual void Dispose()
        {
        }
    }
}
