using System;

namespace BB.Caching.Tests
{
    public class TestBase : IDisposable
    {
        public string TestIp
        {
            get { return "127.0.0.1"; }
        }

        public int TestPort1
        {
            get { return 6379; }
        }

        public int TestPort2
        {
            get { return 6380; }
            //get { return 0; }
        }

        public void Dispose()
        {
        }
    }
}
