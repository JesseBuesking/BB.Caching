using System;
using System.Globalization;
using System.Threading;
using BB.Caching.Caching;
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
            MemoryValue<string> actual = Cache.Memory.Strings.Get<string>(KEY);

            Assert.Equal(SVALUE, actual.Value);
        }

        [Fact]
        public void Long()
        {
            Cache.Memory.Strings.Set(KEY, LVALUE);
            MemoryValue<long> actual = Cache.Memory.Strings.Get<long>(KEY);

            Assert.Equal(LVALUE, actual.Value);
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
            MemoryValue<string> actual = Cache.Memory.Strings.Get<string>(KEY);

            Assert.Equal(SVALUE, actual.Value);
            Thread.Sleep(110);

            actual = Cache.Memory.Strings.Get<string>(KEY);
            Assert.False(actual.Exists);
            Assert.Equal(null, actual.Value);
        }

        [Fact]
        public void SlidingExpiration()
        {
            Cache.Memory.Strings.SetSliding(KEY, SVALUE, TimeSpan.FromSeconds(2));

            MemoryValue<string> actual = Cache.Memory.Strings.Get<string>(KEY);

            Assert.True(actual.Exists);
            Assert.Equal(SVALUE, actual.Value);

            Thread.Sleep(1900);
            actual = Cache.Memory.Strings.Get<string>(KEY);
            Assert.True(actual.Exists);

            Thread.Sleep(200);
            actual = Cache.Memory.Strings.Get<string>(KEY);
            Assert.True(actual.Exists);

            Thread.Sleep(2001);

            actual = Cache.Memory.Strings.Get<string>(KEY);
            Assert.False(actual.Exists);
            Assert.Equal(null, actual.Value);
        }
    }
}