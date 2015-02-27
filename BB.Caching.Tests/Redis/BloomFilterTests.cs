using System;
using System.Collections.Generic;
using System.Linq;
using BB.Caching.Redis;
using Xunit;

namespace BB.Caching.Tests.Redis
{
    public class BloomFilterTests : IUseFixture<DefaultTestFixture>, IUseFixture<BloomFilterTests.BloomFilterFixture>
    {
        private const string KEY = "BloomFilterTests.Key";

        private const float FALSE_POSITIVE_PERCENTAGE = 0.001f;

        public class BloomFilterFixture : IDisposable
        {
            public BloomFilterFixture()
            {
                Cache.Shared.Keys.DeleteAsync(KEY).Wait();
            }

            public void Dispose()
            {
                Cache.Shared.Keys.DeleteAsync(KEY).Wait();
            }
        }

        [Fact]
        public void Add()
        {
            var bloomFilter = new BloomFilter();
            bloomFilter.AddAsync(KEY, "test").Wait();

            var b = bloomFilter.IsSetAsync(KEY, "test").Result;
            Assert.True(b);

            b = bloomFilter.IsSetAsync(KEY, "again").Result;
            Assert.False(b);
        }

        [Fact]
        public void FalsePositiveTest()
        {
            const int stringLength = 3;
            int half = (int)Math.Pow(26, stringLength) / 2;
            // ReSharper disable RedundantArgumentDefaultValue
            var bloomFilter = new BloomFilter(half, FALSE_POSITIVE_PERCENTAGE);
            // ReSharper restore RedundantArgumentDefaultValue
            float fpPercentage = BloomTest(bloomFilter, 3, KEY);

            Console.WriteLine("Target false positive percentage: {0:#0.#%}", FALSE_POSITIVE_PERCENTAGE);
            Console.WriteLine("Actual: {0:#0.###%}, or 1 in {1:#,###.#}", fpPercentage, 1 / fpPercentage);
            Assert.True(FALSE_POSITIVE_PERCENTAGE * 2 >= fpPercentage);
        }

        private static float BloomTest(BloomFilter bloomFilter, int stringLength, string key)
        {
            var values = new List<string>();
            long dbl = bloomFilter.Options.NumberOfItems * 2;
            for (int i = 0; i < dbl; i++)
            {
                int z = i;
                string s = "";
                for (int j = 1; j < stringLength + 1; ++j)
                {
                    s = (char)(z % 26 + 65) + s;
                    z = z / 26;
                }
// ReSharper disable once UnusedVariable
                bloomFilter.AddAsync(key, s).Wait();

                ++i;
                z = i;

                s = "";
                for (int j = 1; j < stringLength + 1; ++j)
                {
                    s = (char)(z % 26 + 65) + s;
                    z = z / 26;
                }
                values.Add(s);
            }

            int fpCount = values.Sum(value => bloomFilter.IsSetAsync(key, value).Result ? 1 : 0);

            return ((float)fpCount) / bloomFilter.Options.NumberOfItems;
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }

        public void SetFixture(BloomFilterFixture data)
        {
        }
    }
}