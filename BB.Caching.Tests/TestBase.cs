namespace BB.Caching.Tests
{
    using System;

    using BB.Caching.Redis;

    using StackExchange.Redis;

    public sealed class DefaultTestFixture : IDisposable
    {
        public DefaultTestFixture()
        {
            var connectionGroup1 = new ConnectionGroup("node-0");

            connectionGroup1.AddWriteConnection(string.Format("{0}:{1},allowAdmin=True", this.TestIp, this.TestPort1));

            SharedCache.Instance.AddRedisConnectionGroup(connectionGroup1);

            if (0 != this.TestPort2)
            {
                var connectionGroup2 = new ConnectionGroup("node-1");

                connectionGroup2.AddWriteConnection(string.Format("{0}:{1}", this.TestIp, this.TestPort1));

                SharedCache.Instance.AddRedisConnectionGroup(connectionGroup2);
            }

            PubSub.Configure(ConnectionMultiplexer.Connect(string.Format("{0}:{1}", this.TestIp, this.TestPort1)));
            Cache.Prepare();
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

        public void Dispose()
        {
        }
    }
}
