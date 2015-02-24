using System;
using System.Globalization;
using System.Threading;
using Xunit;

namespace BB.Caching.Tests.Caching.Memory
{
    public class StringsTests : IDisposable
    {
        private const string KEY = "Memory.StringsTests.Key";

        private const string SVALUE = "Memory.StringsTests.Value";

        private const long LVALUE = 1235L;

        public StringsTests()
        {
            Cache.Memory.Strings.Remove(KEY);
            Assert.False(Cache.Memory.Strings.Exists(KEY));
        }

        public void Dispose()
        {
            Cache.Memory.Strings.Remove(KEY);
            Assert.False(Cache.Memory.Strings.Exists(KEY));
        }

        [Fact]
        public void String()
        {
            Cache.Memory.Strings.Set(KEY, SVALUE);
            string actual;
            Cache.Memory.Strings.TryGet(KEY, out actual);

            Assert.Equal(SVALUE, actual);
        }

        [Fact]
        public void Long()
        {
            Cache.Memory.Strings.Set(KEY, LVALUE);
            long actual;
            Cache.Memory.Strings.TryGet(KEY, out actual);

            Assert.Equal(LVALUE, actual);
        }

        [Fact]
        public void Remove()
        {
            Cache.Memory.Strings.Set(KEY, SVALUE);

            Assert.True(Cache.Memory.Strings.Exists(KEY));

            Cache.Memory.Strings.Remove(KEY);

            Assert.False(Cache.Memory.Strings.Exists(KEY));
        }

        [Fact]
        public void Exists()
        {
            Cache.Memory.Strings.Set(KEY, SVALUE);

            Assert.True(Cache.Memory.Strings.Exists(KEY));
        }

        [Fact]
        public void Count()
        {
            Cache.Memory.Strings.Clear();
            Assert.Equal(0, Cache.Memory.Strings.GetCount());

            for (int i = 0; i < 10; i++)
            {
                Cache.Memory.Strings.Set(KEY + i.ToString(CultureInfo.InvariantCulture), SVALUE);
                Assert.Equal(i + 1, Cache.Memory.Strings.GetCount());
            }

            for (int i = 9; i >= 0; i--)
            {
                Cache.Memory.Strings.Remove(KEY + i.ToString(CultureInfo.InvariantCulture));
                Assert.Equal(i, Cache.Memory.Strings.GetCount());
            }
        }

        [Fact]
        public void AbsoluteExpiration()
        {
            Cache.Memory.Strings.Set(KEY, SVALUE, TimeSpan.FromMilliseconds(100));
            string actual;
            Cache.Memory.Strings.TryGet(KEY, out actual);

            Assert.Equal(SVALUE, actual);
            Thread.Sleep(110);

            Assert.False(Cache.Memory.Strings.TryGet(KEY, out actual));
            Assert.Equal(null, actual);
        }

        [Fact]
        public void SlidingExpiration()
        {
            Cache.Memory.Strings.SetSliding(KEY, SVALUE, TimeSpan.FromSeconds(2));
            string actual;

            Assert.True(Cache.Memory.Strings.TryGet(KEY, out actual));
            Assert.Equal(SVALUE, actual);

            Thread.Sleep(1900);
            Assert.True(Cache.Memory.Strings.TryGet(KEY, out actual));

            Thread.Sleep(200);
            Assert.True(Cache.Memory.Strings.TryGet(KEY, out actual));

            Thread.Sleep(2001);

            Assert.False(Cache.Memory.Strings.TryGet(KEY, out actual));
            Assert.Equal(null, actual);
        }
    }
}