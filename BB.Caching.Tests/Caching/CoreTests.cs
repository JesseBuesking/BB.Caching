using System;
using System.Threading;
using BB.Caching.Caching;
using BB.Caching.Redis;
using Xunit;

namespace BB.Caching.Tests.Caching
{
    public class CoreTests : IUseFixture<DefaultTestFixture>, IDisposable
    {
        private const string KEY = "CoreTests.Key";

        private const string VALUE = "CoreTests.Value";

        public class CoreTestsFixture : IDisposable
        {
            public CoreTestsFixture()
            {
                try
                {
                    Cache.Prepare();
                }
                catch (PubSub.ChannelAlreadySubscribedException)
                { }

                Cache.Memory.Strings.Delete(KEY);
                Cache.Shared.Keys.Delete(KEY);
            }

            public void Dispose()
            {
                Cache.Memory.Strings.Delete(KEY);
                Cache.Shared.Keys.Delete(KEY);
            }
        }

        public CoreTests()
        {
            Cache.Memory.Strings.Delete(KEY);
            Cache.Shared.Keys.Delete(KEY);
        }

        public void Dispose()
        {
            Cache.Memory.Strings.Delete(KEY);
            Cache.Shared.Keys.Delete(KEY);
        }

        [Fact]
        public void ExistsMemory()
        {
            bool exists = Cache.Exists(KEY, Cache.Store.Memory);
            Assert.False(exists);

            Cache.Set(KEY, VALUE, Cache.Store.Memory);
            exists = Cache.Exists(KEY, Cache.Store.Memory);
            Assert.True(exists);
        }

        [Fact]
        public void ExistsRedis()
        {
            bool exists = Cache.Exists(KEY, Cache.Store.Redis);
            Assert.False(exists);

            Cache.Set(KEY, VALUE, Cache.Store.Redis);
            exists = Cache.Exists(KEY, Cache.Store.Redis);
            Assert.True(exists);
        }

        [Fact]
        public void ExistsMemoryAndRedis()
        {
            bool exists = Cache.Exists(KEY, Cache.Store.MemoryAndRedis);
            Assert.False(exists);

            Cache.Set(KEY, VALUE, Cache.Store.MemoryAndRedis);
            exists = Cache.Exists(KEY, Cache.Store.MemoryAndRedis);
            Assert.True(exists);
        }

        [Fact]
        public void ExistsMemoryAsync()
        {
            bool exists = Cache.ExistsAsync(KEY, Cache.Store.Memory).Result;
            Assert.False(exists);

            Cache.SetAsync(KEY, VALUE, Cache.Store.Memory).Wait();
            exists = Cache.ExistsAsync(KEY, Cache.Store.Memory).Result;
            Assert.True(exists);
        }

        [Fact]
        public void ExistsRedisAsync()
        {
            bool exists = Cache.ExistsAsync(KEY, Cache.Store.Redis).Result;
            Assert.False(exists);

            Cache.SetAsync(KEY, VALUE, Cache.Store.Redis).Wait();
            exists = Cache.ExistsAsync(KEY, Cache.Store.Redis).Result;
            Assert.True(exists);
        }

        [Fact]
        public void ExistsMemoryAndRedisAsync()
        {
            bool exists = Cache.ExistsAsync(KEY, Cache.Store.MemoryAndRedis).Result;
            Assert.False(exists);

            Cache.SetAsync(KEY, VALUE, Cache.Store.MemoryAndRedis).Wait();
            exists = Cache.ExistsAsync(KEY, Cache.Store.MemoryAndRedis).Result;
            Assert.True(exists);
        }

        [Fact]
        public void ExpireMemory()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Memory);

            MemoryValue<string> result1 = Cache.Get<string>(KEY, Cache.Store.Memory);
            Cache.Expire(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory);
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.False(redisValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void ExpireRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Redis);

            MemoryValue<string> result1 = Cache.Get<string>(KEY, Cache.Store.Redis);
            Cache.Expire(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis);
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);
            Assert.False(memoryValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void ExpireMemoryAndRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.MemoryAndRedis);

            MemoryValue<string> result1 = Cache.Get<string>(KEY, Cache.Store.MemoryAndRedis);
            Cache.Expire(KEY, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis);
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void ExpireMemoryAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Memory).Wait();

            MemoryValue<string> result1 = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            Cache.Expire(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory);
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.False(redisValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void ExpireRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Redis).Wait();

            MemoryValue<string> result1 = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;
            Cache.Expire(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis);
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);
            Assert.False(memoryValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void ExpireMemoryAndRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.MemoryAndRedis).Wait();

            MemoryValue<string> result1 = Cache.GetAsync<string>(KEY, Cache.Store.MemoryAndRedis).Result;
            Cache.Expire(KEY, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis);
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void GetExpireMemory()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Memory);

            MemoryValue<string> result1 = Cache.Get<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory);
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.False(redisValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void GetExpireRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Redis);

            MemoryValue<string> result1 = Cache.Get<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis);
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);
            Assert.False(memoryValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void GetExpireMemoryAndRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.MemoryAndRedis);

            MemoryValue<string> result1 = Cache.Get<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis);
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void GetExpireMemoryAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Memory).Wait();

            MemoryValue<string> result1 = Cache.GetAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory).Result;
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.False(redisValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void GetExpireRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Redis).Wait();

            MemoryValue<string> result1 = Cache.GetAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis).Result;
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);
            Assert.False(memoryValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void GetExpireMemoryAndRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.MemoryAndRedis).Wait();

            MemoryValue<string> result1 = Cache.GetAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis).Result;
            Assert.Equal(VALUE, result1.Value);

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetMemory()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Memory);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);

            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Redis);

            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);

            Assert.False(memoryValue.Exists);
        }

        [Fact]
        public void SetMemoryAndRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.MemoryAndRedis);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);

            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);
        }

        [Fact]
        public void SetMemoryAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Memory).Wait();

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);

            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Redis).Wait();

            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;

            Assert.False(memoryValue.Exists);
        }

        [Fact]
        public void SetMemoryAndRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.MemoryAndRedis).Wait();

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);

            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);
        }

        [Fact]
        public void SetMemoryAbsolute()
        {
            Cache.Set(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.Memory);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.False(redisValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetRedisAbsolute()
        {
            Cache.Set(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.Redis);

            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);
            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);
            Assert.False(memoryValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetMemoryAndRedisAbsolute()
        {
            Cache.Set(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetMemoryAbsoluteAsync()
        {
            Cache.SetAsync(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.Memory).Wait();

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.False(redisValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetRedisAbsoluteAsync()
        {
            Cache.SetAsync(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.Redis).Wait();

            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;
            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);
            Assert.False(memoryValue.Exists);

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetMemoryAndRedisAbsoluteAsync()
        {
            Cache.SetAsync(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis).Wait();

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetMemorySliding()
        {
            Cache.SetSliding(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.Memory);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.False(redisValue.Exists);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                memoryValue = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory);
                redisValue = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis);

                Assert.True(memoryValue.Exists);
                Assert.Equal(VALUE, memoryValue.Value);
                Assert.False(redisValue.Exists);
            }

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetRedisSliding()
        {
            Cache.SetSliding(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.Redis);

            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);
            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);
            Assert.False(memoryValue.Exists);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                redisValue = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis);
                memoryValue = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory);

                Assert.True(redisValue.Exists);
                Assert.Equal(VALUE, redisValue.Value);
                Assert.False(memoryValue.Exists);
            }

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetMemoryAndRedisSliding()
        {
            Cache.SetSliding(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis);

            MemoryValue<string> memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                redisValue = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis);
                memoryValue = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory);

                Assert.True(memoryValue.Exists);
                Assert.Equal(VALUE, memoryValue.Value);
                Assert.True(redisValue.Exists);
                Assert.Equal(VALUE, redisValue.Value);
            }

            Thread.Sleep(1200);

            memoryValue = Cache.Get<string>(KEY, Cache.Store.Memory);
            redisValue = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetMemorySlidingAsync()
        {
            Cache.SetSlidingAsync(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.Memory).Wait();

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.False(redisValue.Exists);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                memoryValue = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory).Result;
                redisValue = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis).Result;

                Assert.True(memoryValue.Exists);
                Assert.Equal(VALUE, memoryValue.Value);
                Assert.False(redisValue.Exists);
            }

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetRedisSlidingAsync()
        {
            Cache.SetSlidingAsync(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.Redis).Wait();

            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;
            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;

            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);
            Assert.False(memoryValue.Exists);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                redisValue = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis).Result;
                memoryValue = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory).Result;

                Assert.True(redisValue.Exists);
                Assert.Equal(VALUE, redisValue.Value);
                Assert.False(memoryValue.Exists);
            }

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        [Fact]
        public void SetMemoryAndRedisSlidingAsync()
        {
            Cache.SetSlidingAsync(KEY, VALUE, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis).Wait();

            MemoryValue<string> memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(memoryValue.Exists);
            Assert.Equal(VALUE, memoryValue.Value);
            Assert.True(redisValue.Exists);
            Assert.Equal(VALUE, redisValue.Value);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                redisValue = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis).Result;
                memoryValue = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory).Result;

                Assert.True(memoryValue.Exists);
                Assert.Equal(VALUE, memoryValue.Value);
                Assert.True(redisValue.Exists);
                Assert.Equal(VALUE, redisValue.Value);
            }

            Thread.Sleep(1200);

            memoryValue = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            redisValue = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(memoryValue.Exists);
            Assert.False(redisValue.Exists);
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }
    }
}
