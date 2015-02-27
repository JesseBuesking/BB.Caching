using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BB.Caching.Redis;
using SimpleSpeedTester.Core;
using SimpleSpeedTester.Core.OutcomeFilters;
using SimpleSpeedTester.Interfaces;
using StackExchange.Redis;
using Xunit;

namespace BB.Caching.Tests
{
    public class PerformanceTests : IUseFixture<DefaultTestFixture>
    {
        private class CustomResultSummary : ITestResultSummary
        {
            internal CustomResultSummary(TestResult testResult, ITestOutcomeFilter outcomeFilter)
            {
                TestResult = testResult;
                Successes = TestResult.Outcomes.Count(o => o.Exception == null);
                Failures = TestResult.Outcomes.Count() - Successes;

                var eligibleOutcomes = outcomeFilter.Filter(TestResult.Outcomes);

                if (eligibleOutcomes.Any())
                {
                    AverageExecutionTime = eligibleOutcomes.Average(o => o.Elapsed.TotalMilliseconds);
                    ExecutionsPerSecond = eligibleOutcomes.Count / eligibleOutcomes.Sum(o => o.Elapsed.TotalSeconds);

                    double sum = eligibleOutcomes.Sum(
                        o => Math.Pow(o.Elapsed.TotalMilliseconds - AverageExecutionTime, 2)
                    );
                    StandardDeviation = Math.Sqrt(sum/(eligibleOutcomes.Count - 1)) * ExecutionsPerSecond;
                }            
            }        

            /// <summary>
            /// The number of test runs that finished without exception
            /// </summary>
            public int Successes { get; private set; }

            /// <summary>
            /// The number of test runs that excepted
            /// </summary>
            public int Failures { get; private set; }

            /// <summary>
            /// THe average execution time in milliseconds
            /// </summary>
            public double AverageExecutionTime { get; private set; }

            /// <summary>
            /// The number of executions that can be performed per second
            /// </summary>
            private double ExecutionsPerSecond { get; set; }

            /// <summary>
            /// Standard deviation for each.
            /// </summary>
            private double StandardDeviation { get; set; }

            /// <summary>
            /// The test result this summary corresponds to
            /// </summary>
            public TestResult TestResult { get; private set; }

            public override string ToString()
            {
                return string.Format(
    @"
  '{0}' results summary:
    Successes         [{1}]
    Failures          [{2}] 
    Average Exec Time [{3:0.###0}] ms
    Per Second        [{4:#,##0}]
    Stdev Per Second  [+/-{5:#,##0}]", 
                            TestResult.Test,
                            Successes,
                            Failures,
                            AverageExecutionTime,
                            ExecutionsPerSecond,
                            StandardDeviation);
            }
        }

        private static string TestToString(TestGroup group, ITestOutcomeFilter filter = null)
        {
            if (null == filter)
            {
                filter = new DefaultTestOutcomeFilter();
            }

            string header = null;
            string results = group.GetPlannedTests()
                .Select(x =>
                {
                    var summary = new CustomResultSummary((TestResult) x.GetResult(), filter);
                    if (null == header)
                    {
                        header = summary.TestResult.Test.TestGroup.Name;
                    }
                    var result = summary.ToString();
                    return result;
                })
                .Aggregate("", (agg, curr) => agg + curr);

            return String.Format("{0}{1}", header, results);
        }

        private const int ITERATIONS = 10000;

        [Fact]
        public void AllPerformanceTests()
        {
            // warmup
            try
            {
                Cache.Prepare();
            }
            catch (PubSub.ChannelAlreadySubscribedException)
            { }

            Cache.Shared.Keys.ExistsAsync("warmup");

            Console.WriteLine(Shared.Keys.All());
            Console.WriteLine(Shared.Strings.All());
            Console.WriteLine(Shared.Hashes.All());

            Console.WriteLine(Redis.BloomFilter.All());
            Console.WriteLine(Redis.RateLimiter.All());
            Console.WriteLine(Redis.Statistics.All());

            Console.WriteLine(Hashing.ConsistentHashRing.All());
            Console.WriteLine(Hashing.Murmur3.All());
        }

        private static class Shared
        {
            private static readonly RedisKey _key = "PerformanceTests.Shared.Key";

            private static readonly RedisValue _field = "PerformanceTests.Shared.Field";

            private static readonly RedisValue _value = "PerformanceTests.Shared.Value";

            public static class Keys
            {
                public static string All()
                {
                    var group = new TestGroup("Shared Keys");

                    group.Plan("ExistsAsync", Shared.Keys.ExistsAsync, ITERATIONS);
                    group.Plan("ExpireAsync", Shared.Keys.ExpireAsync, ITERATIONS);
                    group.Plan("PersistAsync", Shared.Keys.PersistAsync, ITERATIONS);
                    group.Plan("DeleteAsync", Shared.Keys.DeleteAsync, ITERATIONS);

                    return TestToString(group);
                }

                private static void ExistsAsync()
                {
                    Cache.Shared.Keys.ExistsAsync(_key);
                }

                private static void ExpireAsync()
                {
                    Cache.Shared.Keys.ExpireAsync(_key, TimeSpan.FromSeconds(.5));
                }

                private static void PersistAsync()
                {
                    Cache.Shared.Keys.PersistAsync(_key);
                }

                private static void DeleteAsync()
                {
                    Cache.Shared.Keys.DeleteAsync(_key);
                }
            }

            public static class Strings
            {
                public static string All()
                {
                    var group = new TestGroup("Shared Strings");

                    group.Plan("SetAsync", Shared.Strings.SetAsync, ITERATIONS);
                    group.Plan("GetAsync", Shared.Strings.GetAsync, ITERATIONS);

                    return TestToString(group);
                }

                private static void SetAsync()
                {
// ReSharper disable once UnusedVariable
                    Task task = Cache.Shared.Strings.SetAsync(_key, _value);
                }

                private static void GetAsync()
                {
// ReSharper disable once UnusedVariable
                    Task<RedisValue> task = Cache.Shared.Strings.GetAsync(_key);
                }
            }

            public static class Hashes
            {
                public static string All()
                {
                    var group = new TestGroup("Shared Hashes");

                    group.Plan("SetAsync", Shared.Hashes.SetAsync, ITERATIONS);
                    group.Plan("GetAsync", Shared.Hashes.GetAsync, ITERATIONS);

                    return TestToString(group);
                }

                private static void SetAsync()
                {
// ReSharper disable once UnusedVariable
                    Task<bool> task = Cache.Shared.Hashes.SetAsync(_key, _field, _value);
                }

                private static void GetAsync()
                {
// ReSharper disable once UnusedVariable
                    Task<RedisValue> value = Cache.Shared.Hashes.GetAsync(_key, _field);
                }
            }
        }

        private static class Redis
        {
            public static class BloomFilter
            {
                private static readonly RedisKey _key = "PerformanceTests.Redis.BloomFilter.Key";

                private static readonly RedisValue _value = "PerformanceTests.Redis.BloomFilter.Value";

                public static string All()
                {
                    var group = new TestGroup("Bloom Filter");

                    var bloomFilter = new BB.Caching.Redis.BloomFilter(1000, 0.01f);

                    group.Plan("SetAsync", Redis.BloomFilter.SetAsync, bloomFilter, ITERATIONS);
                    group.Plan("GetAsync", Redis.BloomFilter.GetAsync, bloomFilter, ITERATIONS);

                    return TestToString(group);
                }

                private static void SetAsync(BB.Caching.Redis.BloomFilter bloomFilter)
                {
// ReSharper disable once UnusedVariable
                    Task t = bloomFilter.AddAsync(_key, _value);
                }

                private static void GetAsync(BB.Caching.Redis.BloomFilter bloomFilter)
                {
// ReSharper disable once UnusedVariable
                    Task<bool> task = bloomFilter.IsSetAsync(_key, _value);
                }
            }

            public static class RateLimiter
            {
                private static readonly RedisKey _key = "PerformanceTests.Redis.RateLimiter.Key";

                public static string All()
                {
                    var group = new TestGroup("RateLimiter");

                    group.Plan("IncrementAsync", Redis.RateLimiter.IncrementAsync, ITERATIONS);

                    return TestToString(group);
                }

                private static void IncrementAsync()
                {
// ReSharper disable once UnusedVariable
                    Task<RedisResult> result = BB.Caching.Redis.RateLimiter.IncrementAsync(
                        _key, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), 1);
                }
            }

            public static class Statistics
            {
                private static readonly RedisKey _key = "PerformanceTests.Redis.Statistics.Key";

                public static string All()
                {
                    var group = new TestGroup("Statistics");

                    group.Plan("SetAsync", Redis.Statistics.SetAsync, ITERATIONS);
                    group.Plan("GetAsync", Redis.Statistics.GetAsync, ITERATIONS);

                    return TestToString(group);
                }

                private static void SetAsync()
                {
// ReSharper disable once UnusedVariable
                    Task t = BB.Caching.Redis.Statistics.SetStatisticAsync(_key, 1.0);
                }

                private static void GetAsync()
                {
// ReSharper disable once UnusedVariable
                    Task<BB.Caching.Redis.Statistics.Stats> result = BB.Caching.Redis.Statistics.GetStatisticsAsync(_key);
                }
            }
        }

        private static class Hashing
        {
            public static class ConsistentHashRing
            {
                private static readonly Random _random = new Random(0);

                public static string All()
                {
                    var group = new TestGroup("ConsistentHashRing");

                    var nodes = new Dictionary<string, int>
                        {
                            {"node1", 1},
                            {"node2", 1},
                            {"node3", 1},
                            {"node4", 1},
                            {"node5", 1},
                            {"node6", 1},
                            {"node7", 1},
                            {"node8", 1},
                            {"node9", 1},
                            {"node10", 1}
                        };

                    const int replications = 500;

                    var ring = new BB.Caching.Hashing.ConsistentHashRing<string>();
                    ring.Init(nodes, replications);

                    group.Plan("GetNode", Hashing.ConsistentHashRing.GetNode, ring, ITERATIONS);

                    return TestToString(group);
                }

                /// <summary>
                /// Performance of hashing a key to find the node where it'll be stored.
                /// </summary>
                /// <param name="ring"></param>
                private static void GetNode(BB.Caching.Hashing.ConsistentHashRing<string> ring)
                {
                    ring.GetNode(_random.Next(333333).ToString(CultureInfo.InvariantCulture));
                }
            }

            public static class Murmur3
            {
                public static string All()
                {
                    var group = new TestGroup("Murmur3");

                    group.Plan("HashSpeed", Hashing.Murmur3.HashSpeed, ITERATIONS);

                    return TestToString(group);
                }

                private static void HashSpeed()
                {
// ReSharper disable once UnusedVariable
                    ulong value = BB.Caching.Hashing.Murmur3.ComputeInt("Hash me please!");
                }
            }
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }
    }
}
