using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BB.Caching.Connection;
using Xunit;

namespace BB.Caching.Tests
{
    public class CacheTests
    {
        public class Compression
        {
            [Fact]
            public void SetAndGet()
            {
                const string key = "c-sag-key";
                const string value = "I am a long string. I am a long string. I am a long string. I am a long string. ";

                byte[] compressed = Cache.Memory.SetCompress(key, value);
                string actual;

                Assert.True(Cache.Memory.TryGetDecompress(key, out actual));
                Assert.Equal(value, actual);
            }

            [Fact]
            public void SetAndGetAsync()
            {
                const string key = "c-saga-key";
                const string value = "I am a long string. I am a long string. I am a long string. I am a long string. ";

                byte[] compressed = Cache.Memory.SetCompressAsync(key, value).Result;
                var actual = Cache.Memory.TryGetStringDecompressAsync(key);

                Assert.False(actual.IsNil);
                Assert.Equal(value, actual.Value);
            }

            [Fact]
            public void SetAndGetAbsoluteExpiration()
            {
                const string key = "c-sagae-key";
                const string value = "c-sagae-value";

                byte[] compressed = Cache.Memory.SetCompress(key, value, TimeSpan.FromMilliseconds(100));
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

                byte[] compressed = Cache.Memory.SetCompressAsync(key, value, TimeSpan.FromMilliseconds(100)).Result;
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

                byte[] compressed = Cache.Memory.SetCompressSliding(key, value, TimeSpan.FromSeconds(2));
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

                byte[] compressed = Cache.Memory.SetCompressSlidingAsync(key, value, TimeSpan.FromSeconds(2)).Result;
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

        public class Compaction
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
                byte[] compressed = Cache.Memory.SetCompact(key, value);
                TestObject actual;

                Assert.True(Cache.Memory.TryGetDecompact(key, out actual));
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
                byte[] compressed = Cache.Memory.SetCompactAsync(key, value).Result;
                var actual = Cache.Memory.TryGetDecompactAsync<TestObject>(key);

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
                byte[] compressed = Cache.Memory.SetCompact(key, value, TimeSpan.FromMilliseconds(100));
                TestObject actual;

                Assert.True(Cache.Memory.TryGetDecompact(key, out actual));
                Assert.Equal(value.Long, actual.Long);
                Assert.Equal(value.String, actual.String);

                Thread.Sleep(110);

                Assert.False(Cache.Memory.TryGetDecompact(key, out actual));
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
                byte[] compressed = Cache.Memory.SetCompactAsync(key, value, TimeSpan.FromMilliseconds(100)).Result;
                var actual = Cache.Memory.TryGetDecompactAsync<TestObject>(key);

                Assert.False(actual.IsNil);
                Assert.Equal(value.Long, actual.Value.Long);
                Assert.Equal(value.String, actual.Value.String);

                Thread.Sleep(110);

                actual = Cache.Memory.TryGetDecompactAsync<TestObject>(key);
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
                byte[] compressed = Cache.Memory.SetCompactSliding(key, value, TimeSpan.FromSeconds(2));
                TestObject actual;

                Assert.True(Cache.Memory.TryGetDecompact(key, out actual));
                Assert.Equal(value.Long, actual.Long);
                Assert.Equal(value.String, actual.String);

                Thread.Sleep(1900);
                Assert.True(Cache.Memory.TryGetDecompact(key, out actual));

                Thread.Sleep(200);
                Assert.True(Cache.Memory.TryGetDecompact(key, out actual));

                Thread.Sleep(2001);

                Assert.False(Cache.Memory.TryGetDecompact(key, out actual));
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
                byte[] compressed = Cache.Memory.SetCompactSlidingAsync(key, value, TimeSpan.FromSeconds(2)).Result;
                var actual = Cache.Memory.TryGetDecompactAsync<TestObject>(key);

                Assert.False(actual.IsNil);
                Assert.Equal(value.Long, actual.Value.Long);
                Assert.Equal(value.String, actual.Value.String);

                Thread.Sleep(1900);
                actual = Cache.Memory.TryGetDecompactAsync<TestObject>(key);
                Assert.False(actual.IsNil);

                Thread.Sleep(200);
                actual = Cache.Memory.TryGetDecompactAsync<TestObject>(key);
                Assert.False(actual.IsNil);

                Thread.Sleep(2001);

                actual = Cache.Memory.TryGetDecompactAsync<TestObject>(key);
                Assert.True(actual.IsNil);
                Assert.Equal(null, actual.Value);
            }
        }

        public class Memory
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

        public class Config
        {
            public class ConfigDummy
            {
                public int One
                {
                    get;
                    set;
                }

                public int Two
                {
                    get;
                    set;
                }
            }

            public string Key1 = "key1";

            public ConfigDummy Value1 = new ConfigDummy
                {
                    One = 1,
                    Two = 2
                };

            public string Key2 = "key2";

            public string Value2 = "value2";

            public Config()
            {
                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

                Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27", 6379));

                Cache.Config.Prepare();

                Cache.Shared.Keys.Remove(this.Key1).Wait();
                Cache.Shared.Keys.Remove(this.Key2).Wait();
            }

            public void Dispose()
            {
                Cache.Shared.Keys.Remove(this.Key1).Wait();
                Cache.Shared.Keys.Remove(this.Key2).Wait();
            }

            [Fact]
            public void Set()
            {
                bool isSet = false;
                ConfigDummy value = null;
                Cache.Config.SubscribeChange(this.Key1, async () =>
                    {
                        isSet = true;
                        value = await Cache.Config.GetAsync<ConfigDummy>(this.Key1);
                    });
                Cache.Config.Set(this.Key1, this.Value1);

                while (!isSet)
                    Thread.Sleep(100);

                Assert.True(isSet);
                Assert.Equal(this.Value1.One, value.One);
                Assert.Equal(this.Value1.Two, value.Two);

                Cache.Config.Set(this.Key2, this.Value2, false);

                ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(this.Key1);
                string value2 = Cache.Config.Get<string>(this.Key2);

                Assert.Equal(this.Value1.One, configDummy.One);
                Assert.Equal(this.Value1.Two, configDummy.Two);
                Assert.Equal(this.Value2, value2);

                Cache.Config.Remove(this.Key2, false);
                value2 = Cache.Config.Get<string>(this.Key2);
                Assert.Equal(null, value2);
            }

            [Fact]
            public void SetAsync()
            {
                Cache.Config.SetAsync(this.Key1, this.Value1, false).Wait();
                Cache.Config.SetAsync(this.Key2, this.Value2, false).Wait();

                ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(this.Key1).Result;
                string value2 = Cache.Config.GetAsync<string>(this.Key2).Result;

                Assert.Equal(this.Value1.One, configDummy.One);
                Assert.Equal(this.Value1.Two, configDummy.Two);
                Assert.Equal(this.Value2, value2);

                Cache.Config.RemoveAsync(this.Key2, false).Wait();
                value2 = Cache.Config.GetAsync<string>(this.Key2).Result;
                Assert.Equal(null, value2);
            }

            [Fact]
            public void Get()
            {
                Cache.Config.Set(this.Key1, this.Value1, false);
                Cache.Config.Set(this.Key2, this.Value2, false);

                ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(this.Key1);
                string value2 = Cache.Config.Get<string>(this.Key2);

                Assert.Equal(this.Value1.One, configDummy.One);
                Assert.Equal(this.Value1.Two, configDummy.Two);
                Assert.Equal(this.Value2, value2);

                Cache.Config.Remove(this.Key2, false);
                value2 = Cache.Config.Get<string>(this.Key2);
                Assert.Equal(null, value2);
            }

            [Fact]
            public void GetAsync()
            {
                Cache.Config.SetAsync(this.Key1, this.Value1, false).Wait();
                Cache.Config.SetAsync(this.Key2, this.Value2, false).Wait();

                ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(this.Key1).Result;
                string value2 = Cache.Config.GetAsync<string>(this.Key2).Result;

                Assert.Equal(this.Value1.One, configDummy.One);
                Assert.Equal(this.Value1.Two, configDummy.Two);
                Assert.Equal(this.Value2, value2);

                Cache.Config.RemoveAsync(this.Key2, false).Wait();
                value2 = Cache.Config.GetAsync<string>(this.Key2).Result;
                Assert.Equal(null, value2);
            }

            [Fact]
            public void Remove()
            {
                Cache.Config.Set(this.Key1, this.Value1, false);
                Cache.Config.Set(this.Key2, this.Value2, false);

                ConfigDummy configDummy = Cache.Config.Get<ConfigDummy>(this.Key1);
                string value2 = Cache.Config.Get<string>(this.Key2);

                Assert.Equal(this.Value1.One, configDummy.One);
                Assert.Equal(this.Value1.Two, configDummy.Two);
                Assert.Equal(this.Value2, value2);

                Cache.Config.Remove(this.Key2, false);
                value2 = Cache.Config.Get<string>(this.Key2);
                Assert.Equal(null, value2);
            }

            [Fact]
            public void RemoveAsync()
            {
                Cache.Config.SetAsync(this.Key1, this.Value1, false).Wait();
                Cache.Config.SetAsync(this.Key2, this.Value2, false).Wait();

                ConfigDummy configDummy = Cache.Config.GetAsync<ConfigDummy>(this.Key1).Result;
                string value2 = Cache.Config.GetAsync<string>(this.Key2).Result;

                Assert.Equal(this.Value1.One, configDummy.One);
                Assert.Equal(this.Value1.Two, configDummy.Two);
                Assert.Equal(this.Value2, value2);

                Cache.Config.RemoveAsync(this.Key2, false).Wait();
                value2 = Cache.Config.GetAsync<string>(this.Key2).Result;
                Assert.Equal(null, value2);
            }
        }

        public class Statistic
        {
            public string Key1 = "key1";

            public Statistic()
            {
                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

                Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27", 6379));

                Cache.Statistic.Prepare();

                Cache.Shared.Keys.Remove(this.Key1).Wait();
            }

            public void Dispose()
            {
                Cache.Shared.Keys.Remove(this.Key1).Wait();
            }

            [Fact]
            public void SetAndGet()
            {
                Cache.Statistic.SetStatistic(this.Key1, 1.0).Wait();
                Cache.Statistic.SetStatistic(this.Key1, 2.0).Wait();
                Cache.Statistic.SetStatistic(this.Key1, 3.0).Wait();
                Cache.Statistic.SetStatistic(this.Key1, 4.0).Wait();
                Cache.Statistic.SetStatistic(this.Key1, 5.0).Wait();

                var stat = Cache.Statistic.GetStatistic(this.Key1).Result;

                Assert.Equal(5.0, stat.MaximumValue);
                Assert.Equal(3.0, stat.Mean);
                Assert.Equal(1.0, stat.MinimumValue);
                Assert.Equal(5, stat.NumberOfValues);
                Assert.Equal(Math.Sqrt(5.0d/2.0d), stat.PopulationStandardDeviation);
                Assert.Equal(5.0d/2.0d, stat.PopulationVariance);

                stat = Cache.Statistic.GetStatistic(this.Key1).Result;

                Assert.Equal(5.0, stat.MaximumValue);
                Assert.Equal(3.0, stat.Mean);
                Assert.Equal(1.0, stat.MinimumValue);
                Assert.Equal(5, stat.NumberOfValues);
                Assert.Equal(Math.Sqrt(5.0d/2.0d), stat.PopulationStandardDeviation);
                Assert.Equal(5.0d/2.0d, stat.PopulationVariance);
            }

            [Fact]
            public void Performance()
            {
                const int asyncAmount = 30000;
                var asyncMs = Get(asyncAmount, this.Key1);

                Console.WriteLine("{0:#,##0.0#} async ops per ms", (float) asyncAmount/asyncMs);
                Console.WriteLine();
                Console.WriteLine("async ({0:#,##0}): {1:#,##0}ms", asyncAmount, asyncMs);
            }

            private static long Get(int amount, string key)
            {
                var tasks = new Task[amount];
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < amount; i++)
                    tasks[i] = Cache.Statistic.SetStatistic(key, 1.0);

                Task.WhenAll(tasks);

                return sw.ElapsedMilliseconds;
            }
        }

        public class RateLimiter
        {
            public string Key1 = "key1";

            public RateLimiter()
            {
                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

                Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27", 6379));

                Cache.RateLimiter.Prepare();

                Cache.Shared.Keys.Remove(this.Key1).Wait();
            }

            public void Dispose()
            {
                Cache.Shared.Keys.Remove(this.Key1).Wait();
            }

            [Fact]
            public void Performance()
            {
                const int asyncAmount = 30000;
                var asyncMs = Get(asyncAmount, this.Key1);

                Console.WriteLine("{0:#,##0.0#} async ops per ms", (float) asyncAmount/asyncMs);
                Console.WriteLine();
                Console.WriteLine("async ({0:#,##0}): {1:#,##0}ms", asyncAmount, asyncMs);
            }

            private static long Get(int amount, string key)
            {
                var tasks = new Task<object>[amount];
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < amount; i++)
                {
                    tasks[i] = Cache.RateLimiter.Increment(
                        key, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), amount, 1);
                }

                Task.WhenAll(tasks);

                long total = sw.ElapsedMilliseconds;

                long count = Cache.Shared.Hashes.GetAll(key).Result
                    .Where(kvp => "L" != kvp.Key)
                    .Select(kvp => long.Parse(Encoding.UTF8.GetString(kvp.Value)))
                    .Sum();

//                Assert.True(amount - (amount * .2) < count);
                return total;
            }

            [Fact]
            public void Increment()
            {
                TimeSpan span = TimeSpan.FromSeconds(5);
                TimeSpan bucketSize = TimeSpan.FromSeconds(1);

                object result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 2).Result;
                Assert.Equal(2, (long) result);

                Thread.Sleep(1000);
                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 2).Result;
                Assert.Equal(4, (long) result);

                Thread.Sleep(1000);
                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 2).Result;
                Assert.Equal(6, (long) result);

                Thread.Sleep(1000);
                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 2).Result;
                Assert.Equal(8, (long) result);

                Thread.Sleep(2000);
                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 1).Result;
                Assert.Equal(7, (long) result);

                Thread.Sleep(2000);
                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 1).Result;
                Assert.Equal(4, (long) result);

                Thread.Sleep(2000);
                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 1).Result;
                Assert.Equal(3, (long) result);

                Thread.Sleep(2000);
                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 1).Result;
                Assert.Equal(3, (long) result);
            }

            [Fact]
            public void Edge()
            {
                TimeSpan span = TimeSpan.FromSeconds(4);
                TimeSpan bucketSize = TimeSpan.FromSeconds(1);

                object result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 2).Result;
                Assert.Equal(2, (long) result);
                Thread.Sleep(2000);

                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 2).Result;
                Assert.Equal(4, (long) result);
                Thread.Sleep(2000);

                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 2).Result;
                Assert.Equal(4, (long) result);
                Thread.Sleep(2000);

                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 2).Result;
                Assert.Equal(4, (long) result);
                Thread.Sleep(2000);

                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 2).Result;
                Assert.Equal(4, (long) result);
                Thread.Sleep(2000);

                result = Cache.RateLimiter.Increment(this.Key1, span, bucketSize, 10, 2).Result;
                Assert.Equal(4, (long) result);
                Thread.Sleep(2000);
            }
        }

        public class BloomFilter
        {
            public string Key1 = "key1";

            public BloomFilter()
            {
                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

                Cache.Shared.AddRedisConnectionGroup(
                    new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

                Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27", 6379));

                Cache.BloomFilter.Prepare();

                Cache.Shared.Keys.Remove(this.Key1).Wait();
            }

            public void Dispose()
            {
                Cache.Shared.Keys.Remove(this.Key1).Wait();
            }

            [Fact]
            public void Add()
            {
                var bloomFilter = new Cache.BloomFilter();
                bloomFilter.Add(this.Key1, "test");

                var b = bloomFilter.IsSet(this.Key1, "test").Result;
                Assert.True(b);

                b = bloomFilter.IsSet(this.Key1, "again").Result;
                Assert.False(b);
            }

            [Fact]
            public void SetPerformance()
            {
                const int asyncAmount = 30000;
                var asyncMs = Set(asyncAmount, this.Key1, "test");

                Console.WriteLine("{0:#,##0.0#} async ops per ms", (float) asyncAmount/asyncMs);
                Console.WriteLine();
                Console.WriteLine("async ({0:#,##0}): {1:#,##0}ms", asyncAmount, asyncMs);
            }

            [Fact]
            public void GetPerformance()
            {
                const int asyncAmount = 30000;
                var bloomFilter = new Cache.BloomFilter();
                bloomFilter.Add(this.Key1, "test");
                var asyncMs = Get(asyncAmount, this.Key1, "test");

                Console.WriteLine("{0:#,##0.0#} async ops per ms", (float) asyncAmount/asyncMs);
                Console.WriteLine();
                Console.WriteLine("async ({0:#,##0}): {1:#,##0}ms", asyncAmount, asyncMs);
            }

            private static long Set(int amount, string key, string value)
            {
                var bloomFilter = new Cache.BloomFilter(amount, 0.001f);
                var tasks = new Task[amount];
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < amount; i++)
                    tasks[i] = bloomFilter.Add(key, value);

                Task.WhenAll(tasks);

                return sw.ElapsedMilliseconds;
            }

            private static long Get(int amount, string key, string value)
            {
                var bloomFilter = new Cache.BloomFilter(amount, 0.001f);
                var tasks = new Task<bool>[amount];
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < amount; i++)
                    tasks[i] = bloomFilter.IsSet(key, value);

                Task.WhenAll(tasks);

                return sw.ElapsedMilliseconds;
            }

            [Fact]
            public void FalsePositiveTest()
            {
                const int stringLength = 3;
                const float falsePositivePercentage = 0.001f;
                int half = (int)Math.Pow(26, stringLength)/2;
                var bloomFilter = new Cache.BloomFilter(half, falsePositivePercentage);
                float fpPercentage = BloomTest(bloomFilter, 3, this.Key1);

                Console.WriteLine("fp: {0:#0.####}, {0:#0.####%}, or 1 in {1:#,###.0#}", fpPercentage, 1/fpPercentage);
                Assert.True(falsePositivePercentage*2 >= fpPercentage);
            }

            private static float BloomTest(Cache.BloomFilter bloomFilter, int stringLength, string key)
            {
                var values = new List<string>();
                for (int i = 0; i < bloomFilter.Option.NumberOfItems*2; i++)
                {
                    int z = i;
                    string s = "";
                    for (int j = 1; j < stringLength + 1; ++j)
                    {
                        s = (char)(z%26 + 65) + s;
                        z = z/26;
                    }
                    bloomFilter.Add(key, s);

                    ++i;
                    z = i;

                    s = "";
                    for (int j = 1; j < stringLength + 1; ++j)
                    {
                        s = (char)(z%26 + 65) + s;
                        z = z/26;
                    }
                    values.Add(s);
                }

                int fpCount = 0;
                foreach (string value in values)
                    fpCount += bloomFilter.IsSet(key, value).Result ? 1 : 0;

                return ((float)fpCount)/bloomFilter.Option.NumberOfItems;
            }
        }

        public class Shared
        {
            public class Keyss : IDisposable
            {
                private readonly Dictionary<string, string> KVPs = new Dictionary<string, string>
                    {
                        {"key1", "0"},
                        {"key2", "1"},
                        {"key003", "2"},
                        {"key4", "3"}
                    };

                private string Key
                {
                    get { return this.KVPs.First().Key; }
                }

                private string[] Keys
                {
                    get { return this.KVPs.Keys.ToArray(); }
                }

                private string Value
                {
                    get { return this.KVPs.First().Value; }
                }

                private string[] Values
                {
                    get { return this.KVPs.Values.ToArray(); }
                }

                public Keyss()
                {
                    Cache.Shared.AddRedisConnectionGroup(
                        new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379,
                            // Needed for DebugObject
                            allowAdmin: true)));

                    Cache.Shared.AddRedisConnectionGroup(
                        new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

                    Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27", 6379));

                    Cache.Shared.Keys.Remove(this.Keys).Wait();
                    foreach (var key in this.KVPs.Keys)
                        Assert.False(Cache.Shared.Keys.Exists(key).Result);
                }

                public void Dispose()
                {
                    Cache.Shared.Keys.Remove(this.Keys).Wait();
                    foreach (var key in this.KVPs.Keys)
                        Assert.False(Cache.Shared.Keys.Exists(key).Result);
                }

                [Fact]
                public void Remove()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

                    Assert.True(Cache.Shared.Keys.Remove(this.Key).Result);
                    Assert.False(Cache.Shared.Keys.Exists(this.Key).Result);
                }

                [Fact]
                public void RemoveMultiple()
                {
                    foreach (var kvp in this.KVPs)
                        Cache.Shared.Strings.Set(kvp.Key, kvp.Value).Wait();

                    foreach (var key in this.KVPs.Keys)
                        Assert.True(Cache.Shared.Keys.Exists(key).Result);

                    Assert.Equal(this.KVPs.Count, Cache.Shared.Keys.Remove(this.Keys).Result);

                    foreach (var key in this.KVPs.Keys)
                        Assert.False(Cache.Shared.Keys.Exists(key).Result);
                }

                [Fact]
                public void Exists()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

                    Assert.True(Cache.Shared.Keys.Remove(this.Key).Result);
                    Assert.False(Cache.Shared.Keys.Exists(this.Key).Result);
                }

                [Fact]
                public void Expire()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

                    Thread.Sleep(1000);
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

                    Cache.Shared.Keys.Expire(this.Key, TimeSpan.FromSeconds(2));
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

                    Thread.Sleep(1000);
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

                    Thread.Sleep(1001);
                    Assert.False(Cache.Shared.Keys.Exists(this.Key).Result);
                }

                [Fact]
                public void Persist()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

                    Thread.Sleep(1000);
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

                    Cache.Shared.Keys.Expire(this.Key, TimeSpan.FromSeconds(2));
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

                    Thread.Sleep(1000);
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);

                    Assert.True(Cache.Shared.Keys.Persist(this.Key).Result);

                    Thread.Sleep(1001);
                    Assert.True(Cache.Shared.Keys.Exists(this.Key).Result);
                }

                [Fact]
                public void Find()
                {
                    Cache.Shared.Strings.Set(this.KVPs).Wait();
                    string[] keys = Cache.Shared.Keys.Find("key").Result;

                    Assert.Equal(0, keys.Length);

                    keys = Cache.Shared.Keys.Find("key*").Result;

                    Assert.Equal(4, keys.Length);
                }

                [Fact(Skip="Skipping")]
                public void Random()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();

                    int i = 0;
                    while (true)
                    {
                        ++i;
                        string key = Cache.Shared.Keys.Random().Result;
                        if (null == key)
                        {
                            if (1000 < i)
                                Assert.True(false, "didn't find the value");
                            continue;
                        }
                        Assert.Equal(this.Key, key);
                        break;
                    }
                }

                [Fact]
                public void TimeToLive()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    Assert.Equal(-1, Cache.Shared.Keys.TimeToLive(this.Key).Result);
                    Cache.Shared.Keys.Expire(this.Key, TimeSpan.FromSeconds(3)).Wait();
                    Assert.Equal(3, Cache.Shared.Keys.TimeToLive(this.Key).Result);
                    Thread.Sleep(2000);
                    Assert.Equal(1, Cache.Shared.Keys.TimeToLive(this.Key).Result);
                    Thread.Sleep(1100);
                    Assert.False(Cache.Shared.Keys.Exists(this.Key).Result);
                }

                [Fact]
                public void Type()
                {
                    // TODO test the other types once they're implemented
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    Assert.Equal("string", Cache.Shared.Keys.Type(this.Key).Result);
                }

                [Fact(Skip = "changes depending on tests")]
                public void GetLength()
                {
                    // 1 b/c bb.cache.config
                    Assert.Equal(1, Cache.Shared.Keys.GetLength().Result);
                    Cache.Shared.Strings.Set(this.KVPs);
                    Assert.Equal(this.KVPs.Count + 1, Cache.Shared.Keys.GetLength().Result);
                }

                [Fact]
                public void DebugObject()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    string result = Cache.Shared.Keys.DebugObject(this.Key).Result;
                    Assert.True(result.Contains("encoding:int serializedlength:2"));
                }

                [Fact]
                public void Invalidate()
                {
                    Cache.Memory.Set(this.Key, this.Value);

                    string value;
                    Assert.True(Cache.Memory.TryGet(this.Key, out value));
                    Assert.Equal(this.Value, value);

                    long receivedBy = Cache.Shared.Keys.Invalidate(this.Key).Result;

                    Assert.False(Cache.Memory.TryGet(this.Key, out value));
                    //Assert.Equal(1, receivedBy);
                }

                [Fact]
                public void InvalidateMultiple()
                {
                    foreach (var kvp in this.KVPs)
                        Cache.Memory.Set(kvp.Key, kvp.Value);

                    string value;
                    foreach (var kvp in this.KVPs)
                    {
                        Assert.True(Cache.Memory.TryGet(this.Key, out value));
                        Assert.Equal(this.Value, value);
                    }

                    long receivedBy = Cache.Shared.Keys.Invalidate(this.Keys).Result;
                    //Assert.Equal(1, receivedBy);

                    foreach (var kvp in this.KVPs)
                        Assert.False(Cache.Memory.TryGet(this.Key, out value));
                }
            }

            public class Strings
            {
                private readonly Dictionary<string, string> KVPs = new Dictionary<string, string>
                    {
                        {"key1", "0"},
                        {"key2", "1"},
                        {"key003", "2"},
                        {"key4", "3"}
                    };

                private string Key
                {
                    get { return this.KVPs.First().Key; }
                }

                private string[] Keys
                {
                    get { return this.KVPs.Keys.ToArray(); }
                }

                private string Value
                {
                    get { return this.KVPs.First().Value; }
                }

                private string[] Values
                {
                    get { return this.KVPs.Values.ToArray(); }
                }

                public Strings()
                {
                    Cache.Shared.AddRedisConnectionGroup(
                        new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

                    Cache.Shared.AddRedisConnectionGroup(
                        new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

                    Cache.Shared.Keys.Remove(this.Keys).Wait();
                    foreach (var key in this.KVPs.Keys)
                        Assert.False(Cache.Shared.Keys.Exists(key).Result);
                }

                public void Dispose()
                {
                    Cache.Shared.Keys.Remove(this.Keys).Wait();
                    foreach (var key in this.KVPs.Keys)
                        Assert.False(Cache.Shared.Keys.Exists(key).Result);
                }

                [Fact]
                public void GetPerformance()
                {
                    const int asyncAmount = 100000;
                    const int divisor = 10;
                    const int syncAmount = asyncAmount/divisor;
                    const string key = "s-s-gp-key";
                    const string value = "s-s-gp-value";

                    Cache.Shared.Keys.Remove(key).Wait();
                    Cache.Shared.Strings.Set(key, value).Wait();

                    var asyncMs = Get(asyncAmount, key, value);

                    Stopwatch sw = Stopwatch.StartNew();
                    for (int i = 0; i < syncAmount; i++)
                        Assert.Equal(value, Cache.Shared.Strings.GetString(key).Value);
                    long syncMs = sw.ElapsedMilliseconds;

                    Console.WriteLine("{0:#,##0.0#} async reads per ms", (float) asyncAmount/asyncMs);
                    Console.WriteLine();
                    Console.WriteLine("shared async vs sync: {0:#,##0.0#}%", ((float) asyncMs/(syncMs*divisor))*100);
                    Console.WriteLine("async ({0:#,##0}): {1:#,##0}ms", asyncAmount, asyncMs);
                    Console.WriteLine("sync ({0:#,##0}): {1:#,##0}ms", syncAmount, syncMs);
                    Assert.True(asyncMs < (syncMs*divisor));

                    Cache.Shared.Keys.Remove(key).Wait();
                }

                [Fact]
                public void AsyncBenchmark()
                {
                    const int asyncAmount = 100000;
                    const string key = "s-s-gp-key";
                    const string value = "s-s-gp-value";

                    Cache.Shared.Keys.Remove(key).Wait();
                    Cache.Shared.Strings.Set(key, value).Wait();

                    var asyncMs = Get(asyncAmount, key, value);

                    Console.WriteLine("{0:#,##0.0#} async reads per ms", (float) asyncAmount/asyncMs);
                    Console.WriteLine();
                    Console.WriteLine("async ({0:#,##0}): {1:#,##0}ms", asyncAmount, asyncMs);

                    Cache.Shared.Keys.Remove(key).Wait();
                }

                private static long Get(int amount, string key, string value)
                {
                    var tasks = new Wrapper<string, string>[amount];
                    Stopwatch sw = Stopwatch.StartNew();
                    for (int i = 0; i < amount; i++)
                        tasks[i] = Cache.Shared.Strings.GetString(key);
                    for (int i = 0; i < amount; i++)
                    {
                        Assert.False(tasks[i].IsNil);
                        Assert.Equal(value, tasks[i].Value);
                    }
                    return sw.ElapsedMilliseconds;
                }

                [Fact]
                public void AppendString()
                {
                    Assert.Equal(this.Value.Length, Cache.Shared.Strings.Append(this.Key, this.Value).Result);
                    Assert.Equal(this.Value, Cache.Shared.Strings.GetString(this.Key).Value);
                    Assert.Equal(this.Value.Length*2, Cache.Shared.Strings.Append(this.Key, this.Value).Result);
                    Assert.Equal(this.Value + this.Value, Cache.Shared.Strings.GetString(this.Key).Value);
                }

                [Fact]
                public void AppendBytes()
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
                    Assert.Equal(bytes.Length, Cache.Shared.Strings.Append(this.Key, bytes).Result);
                    Assert.Equal(this.Value, Cache.Shared.Strings.GetString(this.Key).Value);
                    Assert.Equal(bytes.Length*2, Cache.Shared.Strings.Append(this.Key, bytes).Result);
                    Assert.Equal(Encoding.UTF8.GetBytes(this.Value + this.Value),
                        Cache.Shared.Strings.GetByteArray(this.Key).Value);
                }

                [Fact]
                public void DecrementLong()
                {
                    Cache.Shared.Strings.Decrement(this.Key).Wait();
                    Assert.Equal(-1, Cache.Shared.Strings.GetInt64(this.Key).Value);
                    Cache.Shared.Strings.Decrement(this.Key, 3).Wait();
                    Assert.Equal(-4, Cache.Shared.Strings.GetInt64(this.Key).Value);
                }

                [Fact]
                public void IncrementLong()
                {
                    Cache.Shared.Strings.Increment(this.Key).Wait();
                    Assert.Equal(1, Cache.Shared.Strings.GetInt64(this.Key).Value);
                    Cache.Shared.Strings.Increment(this.Key, 3).Wait();
                    Assert.Equal(4, Cache.Shared.Strings.GetInt64(this.Key).Value);
                }

                [Fact]
                public void GetByteArray()
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
                    Cache.Shared.Strings.Set(this.Key, bytes).Wait();
                    var result = Cache.Shared.Strings.GetByteArray(this.Key);

                    Assert.False(result.IsNil);
                    Assert.Equal(bytes, result.Value);
                }

                [Fact]
                public void GetString()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    var result = Cache.Shared.Strings.GetString(this.Key);

                    Assert.False(result.IsNil);
                    Assert.Equal(this.Value, result.Value);
                }

                [Fact]
                public void GetLong()
                {
                    long value = long.Parse(this.Value);
                    Cache.Shared.Strings.Set(this.Key, value).Wait();
                    var result = Cache.Shared.Strings.GetInt64(this.Key);

                    Assert.False(result.IsNil);
                    Assert.Equal(value, result.Value);
                }

                [Fact]
                public void GetByteArraySubset()
                {
                    const string value = "hello";
                    byte[] bytes = Encoding.UTF8.GetBytes(value);
                    Cache.Shared.Strings.Set(this.Key, bytes).Wait();

                    byte[] subset = new byte[3];
                    Array.Copy(bytes, 0, subset, 0, 3);

                    var result = Cache.Shared.Strings.GetByteArray(this.Key, 0, 2);
                    Assert.False(result.IsNil);
                    Assert.Equal(subset, result.Value);
                }

                [Fact]
                public void GetStringSubset()
                {
                    const string value = "hello";
                    Cache.Shared.Strings.Set(this.Key, value).Wait();

                    var result = Cache.Shared.Strings.GetString(this.Key, 0, 2);
                    Assert.False(result.IsNil);
                    Assert.Equal(value.Substring(0, 3), result.Value);
                }

                [Fact]
                public void GetMultipleByteArray()
                {
                    var dictionary = this.KVPs.ToDictionary(k => k.Key, k => Encoding.UTF8.GetBytes(k.Value));
                    Cache.Shared.Strings.Set(dictionary).Wait();

                    var results = Cache.Shared.Strings.GetByteArray(this.KVPs.Keys.ToArray()).Result;
                    int i = 0;
                    foreach (var result in results)
                    {
                        Assert.False(result.IsNil);
                        Assert.True(dictionary.ElementAt(i).Value.SequenceEqual(result.Value));
                        ++i;
                    }
                }

                [Fact]
                public void GetMultipleStrings()
                {
                    Cache.Shared.Strings.Set(this.KVPs).Wait();

                    var results = Cache.Shared.Strings.GetString(this.KVPs.Keys.ToArray()).Result;
                    int i = 0;
                    foreach (var result in results)
                    {
                        Assert.False(result.IsNil);
                        Assert.Equal(this.KVPs.ElementAt(i).Value, result.Value);
                        ++i;
                    }
                }

                [Fact]
                public void GetSetByteArray()
                {
                    byte[] first = Encoding.UTF8.GetBytes("0");
                    byte[] second = Encoding.UTF8.GetBytes("1");

                    Cache.Shared.Strings.Set(this.Key, first).Wait();
                    byte[] result = Cache.Shared.Strings.GetSet(this.Key, second).Value;
                    Assert.Equal(first, result);
                    Assert.Equal(second, Cache.Shared.Strings.GetByteArray(this.Key).Value);
                }

                [Fact]
                public void GetSetString()
                {
                    const string first = "0";
                    const string second = "1";

                    Cache.Shared.Strings.Set(this.Key, first).Wait();
                    string result = Cache.Shared.Strings.GetSet(this.Key, second).Value;
                    Assert.Equal(first, result);
                    Assert.Equal(second, Cache.Shared.Strings.GetString(this.Key).Value);
                }

                [Fact]
                public void SetString()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    Assert.Equal(this.Value, Cache.Shared.Strings.GetString(this.Key).Value);
                }

                [Fact]
                public void SetLong()
                {
                    Cache.Shared.Strings.Set(this.Key, 2L).Wait();
                    Assert.Equal(2L, Cache.Shared.Strings.GetInt64(this.Key).Value);
                }

                [Fact]
                public void SetByteArray()
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
                    Cache.Shared.Strings.Set(this.Key, bytes).Wait();
                    Assert.Equal(bytes, Cache.Shared.Strings.GetByteArray(this.Key).Value);
                }

                [Fact]
                public void SetStringExpires()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value, TimeSpan.FromSeconds(2)).Wait();
                    Assert.False(Cache.Shared.Strings.GetString(this.Key).IsNil);
                    Thread.Sleep(2010);
                    Assert.True(Cache.Shared.Strings.GetString(this.Key).IsNil);
                }

                [Fact]
                public void SetByteArrayExpires()
                {
                    Cache.Shared.Strings.Set(this.Key, Encoding.UTF8.GetBytes(this.Value), TimeSpan.FromSeconds(2)).Wait();
                    Assert.False(Cache.Shared.Strings.GetByteArray(this.Key).IsNil);
                    Thread.Sleep(2010);
                    Assert.True(Cache.Shared.Strings.GetByteArray(this.Key).IsNil);
                }

                [Fact]
                public void SetStringOffset()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    Cache.Shared.Strings.Set(this.Key, 1, this.Value).Wait();
                    Assert.Equal(this.Value.Substring(0, 1) + this.Value, Cache.Shared.Strings.GetString(this.Key).Value);
                }

                [Fact]
                public void SetByteArrayOffset()
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
                    Cache.Shared.Strings.Set(this.Key, bytes).Wait();
                    Cache.Shared.Strings.Set(this.Key, 1, bytes).Wait();
                    byte[] result = new byte[2];
                    Buffer.BlockCopy(bytes, 0, result, 0, 1);
                    Buffer.BlockCopy(bytes, 0, result, 1, 1);
                    Assert.Equal(result, Cache.Shared.Strings.GetByteArray(this.Key).Value);
                }

                [Fact]
                public void SetMultipleStrings()
                {
                    Cache.Shared.Strings.Set(this.KVPs).Wait();
                    foreach (var kvp in this.KVPs)
                        Assert.Equal(kvp.Value, Cache.Shared.Strings.GetString(kvp.Key).Value);
                }

                [Fact]
                public void SetMultipleByteArrays()
                {
                    var dict = this.KVPs.ToDictionary(kvp => kvp.Key, kvp => Encoding.UTF8.GetBytes(kvp.Value));

                    Cache.Shared.Strings.Set(dict).Wait();
                    foreach (var kvp in dict)
                        Assert.Equal(kvp.Value, Cache.Shared.Strings.GetByteArray(kvp.Key).Value);
                }

                [Fact]
                public void SetMultipleStringsIfNotExists()
                {
                    Cache.Shared.Strings.Set(this.KVPs).Wait();
                    Assert.False(Cache.Shared.Strings.SetIfNotExists(this.KVPs).Result);
                }

                [Fact]
                public void SetMultipleByteArraysIfNotExists()
                {
                    var dict = this.KVPs.ToDictionary(kvp => kvp.Key, kvp => Encoding.UTF8.GetBytes(kvp.Value));

                    Cache.Shared.Strings.Set(dict).Wait();
                    Assert.False(Cache.Shared.Strings.SetIfNotExists(dict).Result);
                }

                [Fact]
                public void SetStringIfNotExists()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    Assert.False(Cache.Shared.Strings.SetIfNotExists(this.Key, this.Value).Result);
                }

                [Fact]
                public void SetByteArrayIfNotExists()
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(this.Value);
                    Cache.Shared.Strings.Set(this.Key, bytes).Wait();
                    Assert.False(Cache.Shared.Strings.SetIfNotExists(this.Key, bytes).Result);
                }

                [Fact]
                public void GetBit()
                {
                    Cache.Shared.Strings.SetBit(this.Key, 3, true);
                    Assert.True(Cache.Shared.Strings.GetBit(this.Key, 3).Result);
                    Cache.Shared.Strings.SetBit(this.Key, 3, false);
                    Assert.False(Cache.Shared.Strings.GetBit(this.Key, 3).Result);
                }

                [Fact]
                public void GetLength()
                {
                    Cache.Shared.Strings.Set(this.Key, this.Value).Wait();
                    Assert.Equal(this.Value.Length, Cache.Shared.Strings.GetLength(this.Key).Result);
                }

                [Fact]
                public void SetBit()
                {
                    Cache.Shared.Strings.SetBit(this.Key, 3, true);
                    Assert.True(Cache.Shared.Strings.GetBit(this.Key, 3).Result);
                    Cache.Shared.Strings.SetBit(this.Key, 3, false);
                    Assert.False(Cache.Shared.Strings.GetBit(this.Key, 3).Result);
                }

                [Fact]
                public void CountSetBits()
                {
                    Cache.Shared.Strings.SetBit(this.Key, 3, true);
                    Assert.Equal(1, Cache.Shared.Strings.CountSetBits(this.Key).Result);
                    Cache.Shared.Strings.SetBit(this.Key, 10, true);
                    Assert.Equal(2, Cache.Shared.Strings.CountSetBits(this.Key).Result);
                    Cache.Shared.Strings.SetBit(this.Key, 13, true);
                    Assert.Equal(3, Cache.Shared.Strings.CountSetBits(this.Key).Result);
                }

//                [Fact]
//                public void BitwiseAnd()
//                {
//                    string firstKey = Keys.ElementAt(1);
//                    string secondKey = Keys.ElementAt(2);
//                    string thirdKey = Keys.ElementAt(3);
//
//                    Cache.Shared.Strings.SetBit(firstKey, 3, true);
//                    Cache.Shared.Strings.SetBit(secondKey, 4, true);
//                    Cache.Shared.Strings.BitwiseAnd(thirdKey, new [] {firstKey, secondKey});
//
//                    Assert.Equal(0, Cache.Shared.Strings.CountSetBits(thirdKey).Result);
//                }

                [Fact]
                public void TakeLock()
                {
                    Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(2)).Result);
                    Assert.False(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(2)).Result);

                    Cache.Shared.Strings.ReleaseLock(this.Key).Wait();
                    Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(2)).Result);

                    Thread.Sleep(2010);
                    Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(2)).Result);
                }

                [Fact]
                public void ReleaseLock()
                {
                    Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(2)).Result);
                    Assert.False(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(2)).Result);

                    Cache.Shared.Strings.ReleaseLock(this.Key).Wait();
                    Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(2)).Result);

                    Thread.Sleep(2010);
                    Assert.True(Cache.Shared.Strings.TakeLock(this.Key, this.Value, TimeSpan.FromSeconds(2)).Result);
                }

                [Fact]
                public void CompressionTest()
                {
                    const string expected = "abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd abcd";
                    byte[] compressSet = Cache.Compression.Compress(expected);
                    Cache.Shared.Strings.Set(this.Key, compressSet).Wait();

                    byte[] compressGet = Cache.Shared.Strings.GetByteArray(this.Key).Value;
                    Assert.Equal(expected, Cache.Compression.DecompressString(compressGet));
                }
            }

            public class Hashes
            {
                private readonly Dictionary<string, Dictionary<string, string>> KVPs =
                    new Dictionary<string, Dictionary<string, string>>
                        {
                            {"key1", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                            {"key2", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                            {"key003", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                            {"key4", new Dictionary<string, string> {{"field1", "0"}, {"field2", "1"}}},
                        };

                private Dictionary<string, Dictionary<string, byte[]>> KVPsByteArrays
                {
                    get
                    {
                        return this.KVPs.ToDictionary(k => k.Key, v => v.Value.ToDictionary(
                            k => k.Key, w => Encoding.UTF8.GetBytes(w.Value)));
                    }
                }

                private string Key
                {
                    get { return this.KVPs.First().Key; }
                }

                private string[] Keys
                {
                    get { return this.KVPs.Keys.ToArray(); }
                }

                private Dictionary<string, string> Value
                {
                    get { return this.KVPs.First().Value; }
                }

                public Hashes()
                {
                    Cache.Shared.AddRedisConnectionGroup(
                        new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

                    Cache.Shared.AddRedisConnectionGroup(
                        new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

                    Cache.Shared.Keys.Remove(this.Keys).Wait();
                    foreach (var key in this.KVPs.Keys)
                        Assert.False(Cache.Shared.Keys.Exists(key).Result);
                }

                public void Dispose()
                {
                    Cache.Shared.Keys.Remove(this.Keys).Wait();
                    foreach (var key in this.KVPs.Keys)
                        Assert.False(Cache.Shared.Keys.Exists(key).Result);
                }

                [Fact]
                public void Remove()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, this.Value.ElementAt(0).Value).Wait();
                    Assert.True(Cache.Shared.Hashes.Remove(this.Key, this.Value.ElementAt(0).Key).Result);
                    Assert.False(Cache.Shared.Hashes.Remove(this.Key, this.Value.ElementAt(0).Key).Result);
                }

                [Fact]
                public void RemoveMultiple()
                {
                    var first = this.KVPsByteArrays.First();

                    Cache.Shared.Hashes.Set(first.Key, first.Value).Wait();
                    Assert.Equal(2, Cache.Shared.Hashes.Remove(first.Key, first.Value.Keys.ToArray()).Result);
                    Assert.Equal(0, Cache.Shared.Hashes.Remove(first.Key, first.Value.Keys.ToArray()).Result);
                }

                [Fact]
                public void Exists()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, this.Value.ElementAt(0).Value).Wait();
                    Assert.True(Cache.Shared.Hashes.Exists(this.Key, this.Value.ElementAt(0).Key).Result);

                    Assert.True(Cache.Shared.Hashes.Remove(this.Key, this.Value.ElementAt(0).Key).Result);

                    Assert.False(Cache.Shared.Hashes.Exists(this.Key, this.Value.ElementAt(0).Key).Result);
                }

                [Fact]
                public void GetString()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, this.Value.ElementAt(0).Value).Wait();
                    string value = Cache.Shared.Hashes.GetString(this.Key, this.Value.ElementAt(0).Key).Value;

                    Assert.Equal(this.Value.ElementAt(0).Value, value);
                }

                [Fact]
                public void GetLong()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Value, "2").Wait();
                    long value = Cache.Shared.Hashes.GetInt64(this.Key, this.Value.ElementAt(0).Value).Value;

                    Assert.Equal(2L, value);
                }

                [Fact]
                public void GetByteArray()
                {
                    byte[] expected = Encoding.UTF8.GetBytes(this.Value.ElementAt(0).Value);
                    Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, expected).Wait();
                    byte[] value = Cache.Shared.Hashes.GetByteArray(this.Key, this.Value.ElementAt(0).Key).Value;

                    Assert.Equal(expected, value);
                }

                [Fact]
                public void GetMultipleStrings()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();

                    string[] actuals = Cache.Shared.Hashes.GetString(this.Key, this.Value.Keys.ToArray()).Value;

                    int i = 0;
                    foreach (string key in this.Value.Keys)
                        Assert.Equal(this.Value[key], actuals[i++]);
                }

                [Fact]
                public void GetMultipleByteArray()
                {
                    var value = this.KVPsByteArrays.First().Value;

                    Cache.Shared.Hashes.Set(this.Key, value).Wait();

                    byte[][] actuals = Cache.Shared.Hashes.GetByteArray(this.Key, value.Keys.ToArray()).Value;

                    int i = 0;
                    foreach (string key in value.Keys)
                        Assert.Equal(value[key], actuals[i++]);
                }

                [Fact]
                public void GetAll()
                {
                    var value = this.KVPsByteArrays.First().Value;

                    Cache.Shared.Hashes.Set(this.Key, value).Wait();

                    var actuals = Cache.Shared.Hashes.GetAll(this.Key).Result;

                    int i = 0;
                    foreach (var kvp in value)
                    {
                        Assert.Equal(kvp.Key, actuals.ElementAt(i).Key);
                        Assert.Equal(kvp.Value, actuals.ElementAt(i).Value);
                        ++i;
                    }
                }

                [Fact]
                public void IncrementLong()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
                    Assert.Equal(0, Cache.Shared.Hashes.GetInt64(this.Key, this.Value.ElementAt(0).Key).Value);

                    Cache.Shared.Hashes.Increment(this.Key, this.Value.ElementAt(0).Key, 2).Wait();
                    Assert.Equal(2, Cache.Shared.Hashes.GetInt64(this.Key, this.Value.ElementAt(0).Key).Value);
                }

                [Fact]
                public void IncrementDouble()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
                    Assert.Equal(0.0, Cache.Shared.Hashes.GetDouble(this.Key, this.Value.ElementAt(0).Key).Value);

                    Cache.Shared.Hashes.Increment(this.Key, this.Value.ElementAt(0).Key, 2.84d).Wait();
                    Assert.Equal(2.84, Cache.Shared.Hashes.GetDouble(this.Key, this.Value.ElementAt(0).Key).Value);
                }

                [Fact]
                public void DecrementLong()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
                    Assert.Equal(0, Cache.Shared.Hashes.GetInt64(this.Key, this.Value.ElementAt(0).Key).Value);

                    Cache.Shared.Hashes.Decrement(this.Key, this.Value.ElementAt(0).Key, 2).Wait();
                    Assert.Equal(-2, Cache.Shared.Hashes.GetInt64(this.Key, this.Value.ElementAt(0).Key).Value);
                }

                [Fact]
                public void DecrementDouble()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
                    Assert.Equal(0.0, Cache.Shared.Hashes.GetDouble(this.Key, this.Value.ElementAt(0).Key).Value);

                    Cache.Shared.Hashes.Decrement(this.Key, this.Value.ElementAt(0).Key, 2.84d).Wait();
                    Assert.Equal(-2.84, Cache.Shared.Hashes.GetDouble(this.Key, this.Value.ElementAt(0).Key).Value);
                }

                [Fact]
                public void GetKeys()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
                    string[] actual = Cache.Shared.Hashes.GetKeys(this.Key).Result;

                    Assert.Equal(this.Value.Keys, actual);
                }

                [Fact]
                public void GetValues()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.KVPsByteArrays.First().Value).Wait();
                    byte[][] actual = Cache.Shared.Hashes.GetValues(this.Key).Result;

                    Assert.Equal(this.KVPsByteArrays.First().Value.Values, actual);
                }

                [Fact]
                public void GetLength()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();
                    Assert.Equal(this.Value.Count, Cache.Shared.Hashes.GetLength(this.Key).Result);
                }

                [Fact]
                public void SetString()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, this.Value.ElementAt(0).Value).Wait();
                    string value = Cache.Shared.Hashes.GetString(this.Key, this.Value.ElementAt(0).Key).Value;

                    Assert.Equal(this.Value.ElementAt(0).Value, value);
                }

                [Fact]
                public void SetByteArray()
                {
                    byte[] expected = Encoding.UTF8.GetBytes(this.Value.ElementAt(0).Value);
                    Cache.Shared.Hashes.Set(this.Key, this.Value.ElementAt(0).Key, expected).Wait();
                    byte[] value = Cache.Shared.Hashes.GetByteArray(this.Key, this.Value.ElementAt(0).Key).Value;

                    Assert.Equal(expected, value);
                }

                [Fact]
                public void SetMultipleStrings()
                {
                    Cache.Shared.Hashes.Set(this.Key, this.Value).Wait();

                    string[] actuals = Cache.Shared.Hashes.GetString(this.Key, this.Value.Keys.ToArray()).Value;

                    int i = 0;
                    foreach (string key in this.Value.Keys)
                        Assert.Equal(this.Value[key], actuals[i++]);
                }

                [Fact]
                public void SetMultipleByteArray()
                {
                    var value = this.KVPsByteArrays.First().Value;

                    Cache.Shared.Hashes.Set(this.Key, value).Wait();

                    byte[][] actuals = Cache.Shared.Hashes.GetByteArray(this.Key, value.Keys.ToArray()).Value;

                    int i = 0;
                    foreach (string key in value.Keys)
                        Assert.Equal(value[key], actuals[i++]);
                }

                [Fact]
                public void SetIfNotExistsString()
                {
                    var kvp = this.Value.ElementAt(0);
                    const string bg = "bubblegum";

                    Assert.True(Cache.Shared.Hashes.SetIfNotExists(this.Key, kvp.Key, kvp.Value).Result);

                    Assert.Equal(kvp.Value, Cache.Shared.Hashes.GetString(this.Key, kvp.Key).Value);

                    Assert.False(Cache.Shared.Hashes.SetIfNotExists(this.Key, kvp.Key, bg).Result);
                }

                [Fact]
                public void SetIfNotExistsByteArray()
                {
                    var kvp = this.Value.ElementAt(0);
                    byte[] bytes = Encoding.UTF8.GetBytes(kvp.Value);
                    byte[] bg = Encoding.UTF8.GetBytes("bubblegum");

                    Assert.True(Cache.Shared.Hashes.SetIfNotExists(this.Key, kvp.Key, bytes).Result);

                    Assert.Equal(kvp.Value, Cache.Shared.Hashes.GetString(this.Key, kvp.Key).Value);

                    Assert.False(Cache.Shared.Hashes.SetIfNotExists(this.Key, kvp.Key, bg).Result);
                }
            }
        }
    }
}