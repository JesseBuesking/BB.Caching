using System;
using System.Threading;
using Xunit;

namespace BB.Caching.Tests.CacheTests
{
    public class CompressionTests
    {
        [Fact]
        public void SetAndGet()
        {
            const string key = "c-sag-key";
            const string value = "I am a long string. I am a long string. I am a long string. I am a long string. ";

#pragma warning disable 168
            byte[] compressed = Cache.Memory.SetCompress(key, value);
#pragma warning restore 168
            string actual;

            Assert.True(Cache.Memory.TryGetDecompress(key, out actual));
            Assert.Equal(value, actual);
        }

        [Fact]
        public void SetAndGetAsync()
        {
            const string key = "c-saga-key";
            const string value = "I am a long string. I am a long string. I am a long string. I am a long string. ";

#pragma warning disable 168
            byte[] compressed = Cache.Memory.SetCompressAsync(key, value).Result;
#pragma warning restore 168
            var actual = Cache.Memory.TryGetStringDecompressAsync(key);

            Assert.False(actual.IsNil);
            Assert.Equal(value, actual.Value);
        }

        [Fact]
        public void SetAndGetAbsoluteExpiration()
        {
            const string key = "c-sagae-key";
            const string value = "c-sagae-value";

#pragma warning disable 168
            byte[] compressed = Cache.Memory.SetCompress(key, value, TimeSpan.FromMilliseconds(100));
#pragma warning restore 168
            string actual;

            Assert.True(Cache.Memory.TryGetDecompress(key, out actual));
            Assert.Equal(value, actual);

            Thread.Sleep(110);

            Assert.False(Cache.Memory.TryGetDecompress(key, out actual));
            Assert.Equal(null, actual);
        }

        [Fact]
        public void SetAndGetAbsoluteExpirationAsync()
        {
            const string key = "c-sagaea-key";
            const string value = "c-sagaea-value";

#pragma warning disable 168
            byte[] compressed = Cache.Memory.SetCompressAsync(key, value, TimeSpan.FromMilliseconds(100)).Result;
#pragma warning restore 168
            var actual = Cache.Memory.TryGetStringDecompressAsync(key);

            Assert.False(actual.IsNil);
            Assert.Equal(value, actual.Value);

            Thread.Sleep(110);

            actual = Cache.Memory.TryGetStringDecompressAsync(key);
            Assert.True(actual.IsNil);
            Assert.Equal(null, actual.Value);
        }

        [Fact]
        public void SetAndGetSlidingExpiration()
        {
            const string key = "c-sagse-key";
            const string value = "c-sagse-value";

#pragma warning disable 168
            byte[] compressed = Cache.Memory.SetCompressSliding(key, value, TimeSpan.FromSeconds(2));
#pragma warning restore 168
            string actual;

            Assert.True(Cache.Memory.TryGetDecompress(key, out actual));
            Assert.Equal(value, actual);

            Thread.Sleep(1900);
            Assert.True(Cache.Memory.TryGetDecompress(key, out actual));

            Thread.Sleep(200);
            Assert.True(Cache.Memory.TryGetDecompress(key, out actual));

            Thread.Sleep(2001);

            Assert.False(Cache.Memory.TryGetDecompress(key, out actual));
            Assert.Equal(null, actual);
        }

        [Fact]
        public void SetAndGetSlidingExpirationAsync()
        {
            const string key = "c-sagsea-key";
            const string value = "c-sagsea-value";

#pragma warning disable 168
            byte[] compressed = Cache.Memory.SetCompressSlidingAsync(key, value, TimeSpan.FromSeconds(2)).Result;
#pragma warning restore 168
            var actual = Cache.Memory.TryGetStringDecompressAsync(key);

            Assert.False(actual.IsNil);
            Assert.Equal(value, actual.Value);

            Thread.Sleep(1900);
            actual = Cache.Memory.TryGetStringDecompressAsync(key);
            Assert.False(actual.IsNil);

            Thread.Sleep(200);
            actual = Cache.Memory.TryGetStringDecompressAsync(key);
            Assert.False(actual.IsNil);

            Thread.Sleep(2001);

            actual = Cache.Memory.TryGetStringDecompressAsync(key);
            Assert.True(actual.IsNil);
            Assert.Equal(null, actual.Value);
        }
    }
}