using System;
using System.Threading;
using BB.Caching.Caching.Memory;
using Xunit;

namespace BB.Caching.Tests.Compression
{
    public class CompressionTests
    {
        [Fact]
        public void SetAndGet()
        {
            const string key = "c-sag-key";
            const string value = "I am a long string. I am a long string. I am a long string. I am a long string. ";

            InMemoryCache.SetCompress(key, value);
            string actual;

            Assert.True(InMemoryCache.TryGetDecompress(key, out actual));
            Assert.Equal(value, actual);
        }

        [Fact]
        public void SetAndGetAsync()
        {
            const string key = "c-saga-key";
            const string value = "I am a long string. I am a long string. I am a long string. I am a long string. ";

            InMemoryCache.SetCompressAsync(key, value).Wait();
            var actual = InMemoryCache.TryGetStringDecompressAsync(key);

            Assert.False(actual.IsNil);
            Assert.Equal(value, actual.Value);
        }

        [Fact]
        public void SetAndGetAbsoluteExpiration()
        {
            const string key = "c-sagae-key";
            const string value = "c-sagae-value";

            InMemoryCache.SetCompress(key, value, TimeSpan.FromMilliseconds(100));
            string actual;

            Assert.True(InMemoryCache.TryGetDecompress(key, out actual));
            Assert.Equal(value, actual);

            Thread.Sleep(110);

            Assert.False(InMemoryCache.TryGetDecompress(key, out actual));
            Assert.Equal(null, actual);
        }

        [Fact]
        public void SetAndGetAbsoluteExpirationAsync()
        {
            const string key = "c-sagaea-key";
            const string value = "c-sagaea-value";

            InMemoryCache.SetCompressAsync(key, value, TimeSpan.FromMilliseconds(100)).Wait();
            var actual = InMemoryCache.TryGetStringDecompressAsync(key);

            Assert.False(actual.IsNil);
            Assert.Equal(value, actual.Value);

            Thread.Sleep(110);

            actual = InMemoryCache.TryGetStringDecompressAsync(key);
            Assert.True(actual.IsNil);
            Assert.Equal(null, actual.Value);
        }

        [Fact]
        public void SetAndGetSlidingExpiration()
        {
            const string key = "c-sagse-key";
            const string value = "c-sagse-value";

            InMemoryCache.SetCompressSliding(key, value, TimeSpan.FromSeconds(2));
            string actual;

            Assert.True(InMemoryCache.TryGetDecompress(key, out actual));
            Assert.Equal(value, actual);

            Thread.Sleep(1900);
            Assert.True(InMemoryCache.TryGetDecompress(key, out actual));

            Thread.Sleep(200);
            Assert.True(InMemoryCache.TryGetDecompress(key, out actual));

            Thread.Sleep(2001);

            Assert.False(InMemoryCache.TryGetDecompress(key, out actual));
            Assert.Equal(null, actual);
        }

        [Fact]
        public void SetAndGetSlidingExpirationAsync()
        {
            const string key = "c-sagsea-key";
            const string value = "c-sagsea-value";

            InMemoryCache.SetCompressSlidingAsync(key, value, TimeSpan.FromSeconds(2)).Wait();
            var actual = InMemoryCache.TryGetStringDecompressAsync(key);

            Assert.False(actual.IsNil);
            Assert.Equal(value, actual.Value);

            Thread.Sleep(1900);
            actual = InMemoryCache.TryGetStringDecompressAsync(key);
            Assert.False(actual.IsNil);

            Thread.Sleep(200);
            actual = InMemoryCache.TryGetStringDecompressAsync(key);
            Assert.False(actual.IsNil);

            Thread.Sleep(2001);

            actual = InMemoryCache.TryGetStringDecompressAsync(key);
            Assert.True(actual.IsNil);
            Assert.Equal(null, actual.Value);
        }
    }
}