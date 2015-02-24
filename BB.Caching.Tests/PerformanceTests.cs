using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
            public double ExecutionsPerSecond { get; private set; }

            /// <summary>
            /// Standard deviation for each.
            /// </summary>
            public double StandardDeviation { get; private set; }

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

        [Fact]
        public void AllPerformanceTests()
        {
            // warmup
            Cache.Prepare();
            Cache.Shared.Keys.Exists("warmup");

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

                    group.Plan("Exists", Shared.Keys.Exists, 1000);
                    group.Plan("Expire", Shared.Keys.Expire, 1000);
                    group.Plan("Persist", Shared.Keys.Persist, 1000);
                    group.Plan("Remove", Shared.Keys.Remove, 1000);

                    return TestToString(group);
                }

                private static void Exists()
                {
                    Cache.Shared.Keys.Exists(_key);
                }

                private static void Expire()
                {
                    Cache.Shared.Keys.Expire(_key, TimeSpan.FromSeconds(.5));
                }

                private static void Persist()
                {
                    Cache.Shared.Keys.Persist(_key);
                }

                private static void Remove()
                {
                    Cache.Shared.Keys.Remove(_key);
                }
            }

            public static class Strings
            {
                public static string All()
                {
                    var group = new TestGroup("Shared Strings");

                    group.Plan("Set", Shared.Strings.Set, 1000);
                    group.Plan("Get", Shared.Strings.Get, 1000);

                    return TestToString(group);
                }

                private static void Set()
                {
                    Task task = Cache.Shared.Strings.Set(_key, _value);
                }

                private static void Get()
                {
                    Task<RedisValue> task = Cache.Shared.Strings.GetString(_key);
                }
            }

            public static class Hashes
            {
                public static string All()
                {
                    var group = new TestGroup("Shared Hashes");

                    group.Plan("Set", Shared.Hashes.Set, 1000);
                    group.Plan("Get", Shared.Hashes.Get, 1000);

                    return TestToString(group);
                }

                private static void Set()
                {
                    Task<bool> task = Cache.Shared.Hashes.Set(_key, _field, _value);
                }

                private static void Get()
                {
                    Task<RedisValue> value = Cache.Shared.Hashes.GetString(_key, _field);
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

                    group.Plan("Set", Redis.BloomFilter.Set, bloomFilter, 1000);
                    group.Plan("Get", Redis.BloomFilter.Get, bloomFilter, 1000);

                    return TestToString(group);
                }

                private static void Set(BB.Caching.Redis.BloomFilter bloomFilter)
                {
                    Task task = bloomFilter.Add(_key, _value);
                }

                private static void Get(BB.Caching.Redis.BloomFilter bloomFilter)
                {
                    Task<bool> task = bloomFilter.IsSet(_key, _value);
                }
            }

            public static class RateLimiter
            {
                private static readonly RedisKey _key = "PerformanceTests.Redis.RateLimiter.Key";

                public static string All()
                {
                    var group = new TestGroup("RateLimiter");

                    group.Plan("Increment", Redis.RateLimiter.Increment, 1000);

                    return TestToString(group);
                }

                private static void Increment()
                {
                    BB.Caching.Redis.RateLimiter.Increment(
                        _key, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), 1);
                }
            }

            public static class Statistics
            {
                private static readonly RedisKey _key = "PerformanceTests.Redis.Statistics.Key";

                public static string All()
                {
                    var group = new TestGroup("Statistics");

                    group.Plan("Set", Redis.Statistics.Set, 1000);
                    group.Plan("Get", Redis.Statistics.Get, 1000);

                    return TestToString(group);
                }

                private static void Set()
                {
                    BB.Caching.Redis.Statistics.SetStatistic(_key, 1.0);
                }

                private static void Get()
                {
                    BB.Caching.Redis.Statistics.GetStatistics(_key);
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

                    group.Plan("GetNode", Hashing.ConsistentHashRing.GetNode, ring, 1000);

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

                    group.Plan("HashSpeed", Hashing.Murmur3.HashSpeed, 1000);

                    return TestToString(group);
                }

                private static void HashSpeed()
                {
                    ulong value = BB.Caching.Hashing.Murmur3.ComputeInt("Hash me please!");
                }
            }
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }
    }
}
