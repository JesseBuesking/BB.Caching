using System;
using System.Threading;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class CompactionTests
    {
        public class TestObject
        {
            public long Long
            {
                get;
                set;
            }

            public string String
            {
                get;
                set;
            }
        }

        [Fact]
        public void SetAndGet()
        {
            const string key = "s-sag-key";
            var value = new TestObject
                {
                    Long = 1234L,
                    String = "I am a string!"
                };
            byte[] compressed = Caching.Cache.Memory.SetCompact(key, value);
            TestObject actual;

            Assert.True(Caching.Cache.Memory.TryGetDecompact(key, out actual));
            Assert.Equal(value.Long, actual.Long);
            Assert.Equal(value.String, actual.String);
        }

        [Fact]
        public void SetAndGetAsync()
        {
            const string key = "s-saga-key";
            var value = new TestObject
                {
                    Long = 1234L,
                    String = "I am a string!"
                };
            byte[] compressed = Caching.Cache.Memory.SetCompactAsync(key, value).Result;
            var actual = Caching.Cache.Memory.TryGetDecompactAsync<TestObject>(key);

            Assert.False(actual.IsNil);
            Assert.Equal(value.Long, actual.Value.Long);
            Assert.Equal(value.String, actual.Value.String);
        }

        [Fact]
        public void SetAndGetAbsoluteExpiration()
        {
            const string key = "c-sagae-key";
            var value = new TestObject
                {
                    Long = 1234L,
                    String = "I am a string!"
                };
            byte[] compressed = Caching.Cache.Memory.SetCompact(key, value, TimeSpan.FromMilliseconds(100));
            TestObject actual;

            Assert.True(Caching.Cache.Memory.TryGetDecompact(key, out actual));
            Assert.Equal(value.Long, actual.Long);
            Assert.Equal(value.String, actual.String);

            Thread.Sleep(110);

            Assert.False(Caching.Cache.Memory.TryGetDecompact(key, out actual));
            Assert.Equal(null, actual);
        }

        [Fact]
        public void SetAndGetAbsoluteExpirationAsync()
        {
            const string key = "c-sagaea-key";
            var value = new TestObject
                {
                    Long = 1234L,
                    String = "I am a string!"
                };
            byte[] compressed = Caching.Cache.Memory.SetCompactAsync(key, value, TimeSpan.FromMilliseconds(100)).Result;
            var actual = Caching.Cache.Memory.TryGetDecompactAsync<TestObject>(key);

            Assert.False(actual.IsNil);
            Assert.Equal(value.Long, actual.Value.Long);
            Assert.Equal(value.String, actual.Value.String);

            Thread.Sleep(110);

            actual = Caching.Cache.Memory.TryGetDecompactAsync<TestObject>(key);
            Assert.True(actual.IsNil);
            Assert.Equal(null, actual.Value);
        }

        [Fact]
        public void SetAndGetSlidingExpiration()
        {
            const string key = "c-sagse-key";
            var value = new TestObject
                {
                    Long = 1234L,
                    String = "I am a string!"
                };
            byte[] compressed = Caching.Cache.Memory.SetCompactSliding(key, value, TimeSpan.FromSeconds(2));
            TestObject actual;

            Assert.True(Caching.Cache.Memory.TryGetDecompact(key, out actual));
            Assert.Equal(value.Long, actual.Long);
            Assert.Equal(value.String, actual.String);

            Thread.Sleep(1900);
            Assert.True(Caching.Cache.Memory.TryGetDecompact(key, out actual));

            Thread.Sleep(200);
            Assert.True(Caching.Cache.Memory.TryGetDecompact(key, out actual));

            Thread.Sleep(2001);

            Assert.False(Caching.Cache.Memory.TryGetDecompact(key, out actual));
            Assert.Equal(null, actual);
        }

        [Fact]
        public void SetAndGetSlidingExpirationAsync()
        {
            const string key = "c-sagsea-key";
            var value = new TestObject
                {
                    Long = 1234L,
                    String = "I am a string!"
                };
            byte[] compressed = Caching.Cache.Memory.SetCompactSlidingAsync(key, value, TimeSpan.FromSeconds(2)).Result;
            var actual = Caching.Cache.Memory.TryGetDecompactAsync<TestObject>(key);

            Assert.False(actual.IsNil);
            Assert.Equal(value.Long, actual.Value.Long);
            Assert.Equal(value.String, actual.Value.String);

            Thread.Sleep(1900);
            actual = Caching.Cache.Memory.TryGetDecompactAsync<TestObject>(key);
            Assert.False(actual.IsNil);

            Thread.Sleep(200);
            actual = Caching.Cache.Memory.TryGetDecompactAsync<TestObject>(key);
            Assert.False(actual.IsNil);

            Thread.Sleep(2001);

            actual = Caching.Cache.Memory.TryGetDecompactAsync<TestObject>(key);
            Assert.True(actual.IsNil);
            Assert.Equal(null, actual.Value);
        }
    }
}