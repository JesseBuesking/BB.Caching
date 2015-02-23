using System;
using System.Globalization;
using System.Threading;
using Xunit;

namespace BB.Caching.Tests.Caching.Memory
{
    public class StringsTests
    {
        [Fact]
        public void String()
        {
            const string key = "ms-key";
            const string value = "ms-value";

            Cache.Memory.Strings.Set(key, value);
            string actual;
            Cache.Memory.Strings.TryGet(key, out actual);

            Assert.Equal(value, actual);
        }

        [Fact]
        public void Long()
        {
            const string key = "ml-key";
            const long value = 1235L;

            Cache.Memory.Strings.Set(key, value);
            long actual;
            Cache.Memory.Strings.TryGet(key, out actual);

            Assert.Equal(value, actual);
        }

        [Fact]
        public void Delete()
        {
            const string key = "md-key";
            const string value = "md-value";

            Cache.Memory.Strings.Set(key, value);

            Assert.True(Cache.Memory.Strings.Exists(key));

            Cache.Memory.Strings.Remove(key);

            Assert.False(Cache.Memory.Strings.Exists(key));
        }

        [Fact]
        public void Exists()
        {
            const string key = "me-key";
            const string value = "me-value";

            Cache.Memory.Strings.Set(key, value);

            Assert.True(Cache.Memory.Strings.Exists(key));
        }

        [Fact]
        public void Count()
        {
            const string key = "me-key";
            const string value = "me-value";

            Cache.Memory.Strings.Clear();
            Assert.Equal(0, Cache.Memory.Strings.GetCount());

            for (int i = 0; i < 10; i++)
            {
                Cache.Memory.Strings.Set(key + i.ToString(CultureInfo.InvariantCulture), value);
                Assert.Equal(i + 1, Cache.Memory.Strings.GetCount());
            }

            for (int i = 9; i >= 0; i--)
            {
                Cache.Memory.Strings.Remove(key + i.ToString(CultureInfo.InvariantCulture));
                Assert.Equal(i, Cache.Memory.Strings.GetCount());
            }
        }

        [Fact]
        public void AbsoluteExpiration()
        {
            const string key = "mae-key";
            const string value = "mae-value";

            Cache.Memory.Strings.Set(key, value, TimeSpan.FromMilliseconds(100));
            string actual;
            Cache.Memory.Strings.TryGet(key, out actual);

            Assert.Equal(value, actual);
            Thread.Sleep(110);

            Assert.False(Cache.Memory.Strings.TryGet(key, out actual));
            Assert.Equal(null, actual);
        }

        [Fact]
        public void SlidingExpiration()
        {
            const string key = "mse-key";
            const string value = "mse-value";

            Cache.Memory.Strings.SetSliding(key, value, TimeSpan.FromSeconds(2));
            string actual;

            Assert.True(Cache.Memory.Strings.TryGet(key, out actual));
            Assert.Equal(value, actual);

            Thread.Sleep(1900);
            Assert.True(Cache.Memory.Strings.TryGet(key, out actual));

            Thread.Sleep(200);
            Assert.True(Cache.Memory.Strings.TryGet(key, out actual));

            Thread.Sleep(2001);

            Assert.False(Cache.Memory.Strings.TryGet(key, out actual));
            Assert.Equal(null, actual);
        }
    }
}