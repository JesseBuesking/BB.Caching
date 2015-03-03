namespace BB.Caching.Tests.Caching
{
    using System;
    using System.Threading;

    using BB.Caching.Caching;

    using Xunit;

    public class CoreTests : IUseFixture<DefaultTestFixture>, IDisposable
    {
        private const string KEY = "CoreTests.Key";

        private const string VALUE = "CoreTests.Value";

        private const string VALUE2 = "CoreTests.Value2";

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
        public void DeleteMemory()
        {
            Assert.False(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.False(Cache.Exists(KEY, Cache.Store.Redis));

            Cache.Set(KEY, VALUE, Cache.Store.Memory);

            Assert.True(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.False(Cache.Exists(KEY, Cache.Store.Redis));

            Cache.Delete(KEY, Cache.Store.Memory);

            Assert.False(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.False(Cache.Exists(KEY, Cache.Store.Redis));
        }

        [Fact]
        public void DeleteRedis()
        {
            Assert.False(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.False(Cache.Exists(KEY, Cache.Store.Redis));

            Cache.Set(KEY, VALUE, Cache.Store.Redis);

            Assert.False(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.True(Cache.Exists(KEY, Cache.Store.Redis));

            Cache.Delete(KEY, Cache.Store.Redis);

            Assert.False(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.False(Cache.Exists(KEY, Cache.Store.Redis));
        }

        [Fact]
        public void DeleteMemoryAndRedis()
        {
            Assert.False(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.False(Cache.Exists(KEY, Cache.Store.Redis));

            Cache.Set(KEY, VALUE, Cache.Store.MemoryAndRedis);

            Assert.True(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.True(Cache.Exists(KEY, Cache.Store.Redis));

            Cache.Delete(KEY, Cache.Store.MemoryAndRedis);

            Assert.False(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.False(Cache.Exists(KEY, Cache.Store.Redis));
        }

        [Fact]
        public void DeleteMemoryAsync()
        {
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);

            Cache.SetAsync(KEY, VALUE, Cache.Store.Memory).Wait();

            Assert.True(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);

            Cache.DeleteAsync(KEY, Cache.Store.Memory).Wait();

            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);
        }

        [Fact]
        public void DeleteRedisAsync()
        {
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);

            Cache.SetAsync(KEY, VALUE, Cache.Store.Redis).Wait();

            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.True(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);

            Cache.DeleteAsync(KEY, Cache.Store.Redis).Wait();

            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);
        }

        [Fact]
        public void DeleteMemoryAndRedisAsync()
        {
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);

            Cache.SetAsync(KEY, VALUE, Cache.Store.MemoryAndRedis).Wait();

            Assert.True(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.True(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);

            Cache.DeleteAsync(KEY, Cache.Store.MemoryAndRedis).Wait();

            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);
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

            MemoryValue<string> result1 =
                Cache.GetAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory).Result;
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

            MemoryValue<string> result1 =
                Cache.GetAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis).Result;
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

        [Fact]
        public void GetSetMemory()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Memory);

            MemoryValue<string> mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            mem = Cache.GetSet(KEY, VALUE2, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE2, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Redis);

            MemoryValue<string> mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.GetSet(KEY, VALUE2, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE2, red.Value);
        }

        [Fact]
        public void GetSetMemoryAndRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.MemoryAndRedis);

            MemoryValue<string> mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            MemoryValue<string> both = Cache.GetSet(KEY, VALUE2, Cache.Store.MemoryAndRedis);

            Assert.True(both.Exists);
            Assert.Equal(VALUE, both.Value);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE2, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE2, red.Value);
        }

        [Fact]
        public void GetSetMemoryAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Memory).Wait();

            MemoryValue<string> mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            mem = Cache.GetSetAsync(KEY, VALUE2, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE2, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Redis).Wait();

            MemoryValue<string> mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetSetAsync(KEY, VALUE2, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE2, red.Value);
        }

        [Fact]
        public void GetSetMemoryAndRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.MemoryAndRedis).Wait();

            MemoryValue<string> mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            MemoryValue<string> both = Cache.GetSetAsync(KEY, VALUE2, Cache.Store.MemoryAndRedis).Result;

            Assert.True(both.Exists);
            Assert.Equal(VALUE, both.Value);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE2, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE2, red.Value);
        }

        [Fact]
        public void GetSetAbsoluteMemory()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Memory);

            MemoryValue<string> mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            mem = Cache.GetSet(KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE2, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            Thread.Sleep(1200);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetAbsoluteRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Redis);

            MemoryValue<string> mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.GetSet(KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE2, red.Value);

            Thread.Sleep(1200);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetAbsoluteMemoryAndRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.MemoryAndRedis);

            MemoryValue<string> mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            MemoryValue<string> both = Cache.GetSet(KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis);

            Assert.True(both.Exists);
            Assert.Equal(VALUE, both.Value);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE2, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE2, red.Value);

            Thread.Sleep(1200);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetAbsoluteMemoryAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Memory).Wait();

            MemoryValue<string> mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            mem = Cache.GetSetAsync(KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE2, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            Thread.Sleep(1200);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetAbsoluteRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Redis).Wait();

            MemoryValue<string> mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetSetAsync(KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE2, red.Value);

            Thread.Sleep(1200);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetAbsoluteMemoryAndRedisAsync()
        {
            Cache.Set(KEY, VALUE, Cache.Store.MemoryAndRedis);

            MemoryValue<string> mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            MemoryValue<string> both =
                Cache.GetSetAsync(KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis).Result;

            Assert.True(both.Exists);
            Assert.Equal(VALUE, both.Value);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE2, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE2, red.Value);

            Thread.Sleep(1200);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetSlidingMemory()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Memory);

            MemoryValue<string> mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            mem = Cache.GetSetSliding(KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                mem = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory);
                red = Cache.Get<string>(KEY, Cache.Store.Redis);

                Assert.True(mem.Exists);
                Assert.Equal(VALUE2, mem.Value);
                Assert.False(red.Exists);
                Assert.Equal(null, red.Value);
            }

            Thread.Sleep(1200);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetSlidingRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.Redis);

            MemoryValue<string> mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.GetSetSliding(KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                mem = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory);
                red = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis);

                Assert.False(mem.Exists);
                Assert.Equal(null, mem.Value);
                Assert.True(red.Exists);
                Assert.Equal(VALUE2, red.Value);
            }

            Thread.Sleep(1200);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetSlidingMemoryAndRedis()
        {
            Cache.Set(KEY, VALUE, Cache.Store.MemoryAndRedis);

            MemoryValue<string> mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            MemoryValue<string> red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            MemoryValue<string> both = Cache.GetSetSliding(
                KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis);

            Assert.True(both.Exists);
            Assert.Equal(VALUE, both.Value);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                mem = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory);
                red = Cache.GetSliding<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis);

                Assert.True(mem.Exists);
                Assert.Equal(VALUE2, mem.Value);
                Assert.True(red.Exists);
                Assert.Equal(VALUE2, red.Value);
            }

            Thread.Sleep(1200);

            mem = Cache.Get<string>(KEY, Cache.Store.Memory);
            red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetSlidingMemoryAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Memory).Wait();

            MemoryValue<string> mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> red = Cache.Get<string>(KEY, Cache.Store.Redis);

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            mem = Cache.GetSetSlidingAsync(KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                mem = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory).Result;
                red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

                Assert.True(mem.Exists);
                Assert.Equal(VALUE2, mem.Value);
                Assert.False(red.Exists);
                Assert.Equal(null, red.Value);
            }

            Thread.Sleep(1200);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetSlidingRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.Redis).Wait();

            MemoryValue<string> mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetSetSlidingAsync(KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                mem = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory).Result;
                red = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis).Result;

                Assert.False(mem.Exists);
                Assert.Equal(null, mem.Value);
                Assert.True(red.Exists);
                Assert.Equal(VALUE2, red.Value);
            }

            Thread.Sleep(1200);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void GetSetSlidingMemoryAndRedisAsync()
        {
            Cache.SetAsync(KEY, VALUE, Cache.Store.MemoryAndRedis).Wait();

            MemoryValue<string> mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            MemoryValue<string> red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.True(mem.Exists);
            Assert.Equal(VALUE, mem.Value);
            Assert.True(red.Exists);
            Assert.Equal(VALUE, red.Value);

            MemoryValue<string> both = Cache.GetSetSlidingAsync(
                KEY, VALUE2, TimeSpan.FromSeconds(1), Cache.Store.MemoryAndRedis).Result;

            Assert.True(both.Exists);
            Assert.Equal(VALUE, both.Value);

            for (int i = 0; i < 3; ++i)
            {
                Thread.Sleep(800);

                mem = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Memory).Result;
                red = Cache.GetSlidingAsync<string>(KEY, TimeSpan.FromSeconds(1), Cache.Store.Redis).Result;

                Assert.True(mem.Exists);
                Assert.Equal(VALUE2, mem.Value);
                Assert.True(red.Exists);
                Assert.Equal(VALUE2, red.Value);
            }

            Thread.Sleep(1200);

            mem = Cache.GetAsync<string>(KEY, Cache.Store.Memory).Result;
            red = Cache.GetAsync<string>(KEY, Cache.Store.Redis).Result;

            Assert.False(mem.Exists);
            Assert.Equal(null, mem.Value);
            Assert.False(red.Exists);
            Assert.Equal(null, red.Value);
        }

        [Fact]
        public void BroadcastDelete()
        {
            Assert.False(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.False(Cache.Exists(KEY, Cache.Store.Redis));

            Cache.Set(KEY, VALUE, Cache.Store.MemoryAndRedis);

            Assert.True(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.True(Cache.Exists(KEY, Cache.Store.Redis));

            Cache.BroadcastDelete(KEY);

            Assert.False(Cache.Exists(KEY, Cache.Store.Memory));
            Assert.False(Cache.Exists(KEY, Cache.Store.Redis));
        }

        [Fact]
        public void BroadcastDeleteAsync()
        {
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);

            Cache.SetAsync(KEY, VALUE, Cache.Store.MemoryAndRedis).Wait();

            Assert.True(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.True(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);

            Cache.BroadcastDeleteAsync(KEY).Wait();

            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Memory).Result);
            Assert.False(Cache.ExistsAsync(KEY, Cache.Store.Redis).Result);
        }
    }
}
