using System;
using System.Globalization;
using System.Threading;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class MemoryTests
    {
        [Fact]
        public void String()
        {
            const string key = "ms-key";
            const string value = "ms-value";

            Cache.Memory.Set(key, value);
            string actual;
            Cache.Memory.TryGet(key, out actual);

            Assert.Equal(value, actual);
        }

        [Fact]
        public void Long()
        {
            const string key = "ml-key";
            const long value = 1235L;

            Cache.Memory.Set(key, value);
            long actual;
            Cache.Memory.TryGet(key, out actual);

            Assert.Equal(value, actual);
        }

        [Fact]
        public void Delete()
        {
            const string key = "md-key";
            const string value = "md-value";

            Cache.Memory.Set(key, value);

            Assert.True(Cache.Memory.Exists(key));

            Cache.Memory.Remove(key);

            Assert.False(Cache.Memory.Exists(key));
        }

        [Fact]
        public void Exists()
        {
            const string key = "me-key";
            const string value = "me-value";

            Cache.Memory.Set(key, value);

            Assert.True(Cache.Memory.Exists(key));
        }

        [Fact]
        public void Count()
        {
            const string key = "me-key";
            const string value = "me-value";

            Cache.Memory.Clear();
            Assert.Equal(0, Cache.Memory.GetCount());

            for (int i = 0; i < 10; i++)
            {
                Cache.Memory.Set(key + i.ToString(CultureInfo.InvariantCulture), value);
                Assert.Equal(i + 1, Cache.Memory.GetCount());
            }

            for (int i = 9; i >= 0; i--)
            {
                Cache.Memory.Remove(key + i.ToString(CultureInfo.InvariantCulture));
                Assert.Equal(i, Cache.Memory.GetCount());
            }
        }

        [Fact]
        public void AbsoluteExpiration()
        {
            const string key = "mae-key";
            const string value = "mae-value";

            Cache.Memory.Set(key, value, TimeSpan.FromMilliseconds(100));
            string actual;
            Cache.Memory.TryGet(key, out actual);

            Assert.Equal(value, actual);
            Thread.Sleep(110);

            Assert.False(Cache.Memory.TryGet(key, out actual));
            Assert.Equal(null, actual);
        }

        [Fact]
        public void SlidingExpiration()
        {
            const string key = "mse-key";
            const string value = "mse-value";

            Cache.Memory.SetSliding(key, value, TimeSpan.FromSeconds(2));
            string actual;

            Assert.True(Cache.Memory.TryGet(key, out actual));
            Assert.Equal(value, actual);

            Thread.Sleep(1900);
            Assert.True(Cache.Memory.TryGet(key, out actual));

            Thread.Sleep(200);
            Assert.True(Cache.Memory.TryGet(key, out actual));

            Thread.Sleep(2001);

            Assert.False(Cache.Memory.TryGet(key, out actual));
            Assert.Equal(null, actual);
        }
    }
}