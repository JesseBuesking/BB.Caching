using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;

namespace BB.Caching.Tests.Hashing
{
    public class ConsistentHashRingTests
    {
        private class Stats
        {
            private double Mean
            {
// ReSharper disable UnusedAutoPropertyAccessor.Local
                get;
// ReSharper restore UnusedAutoPropertyAccessor.Local
                set;
            }

            public double SDev
            {
                get;
                private set;
            }

            public long Count
            {
                get;
                private set;
            }

            public Stats(double mean, double sdev, long count)
            {
                this.Mean = mean;
                this.SDev = sdev;
                this.Count = count;
            }
        }

        [Fact]
        public void PerformanceTests()
        {
            const long iterations = 300000;
            const int replications = 120;
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

            var ring = new BB.Caching.Hashing.ConsistentHashRing<string>();
            ring.Init(nodes, replications);

            Stopwatch sw = Stopwatch.StartNew();
            var murmur3Stats = this.RunTest(iterations, ring);
            long murmurMs = sw.ElapsedMilliseconds;


            Console.WriteLine("ConsistentHashRing Stress Test:");
            Console.WriteLine("iterations: {0:#,##0}", iterations);
            Console.WriteLine("replications: {0:#,##0}", replications);
            Console.WriteLine("murmur3 sdev: {0:#,##0.0#}%", murmur3Stats.SDev);
            Console.WriteLine("murmur3 time: {0:#,##0}ms", murmurMs);

            Assert.Equal(iterations, murmur3Stats.Count);

            Assert.True(murmur3Stats.SDev < 20.0);
        }

        [Fact]
        public void HashRingPerformanceTest()
        {
            const long iterations = 3000000;
            const int replications = 500;
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

            var ring = new BB.Caching.Hashing.ConsistentHashRing<string>();
            ring.Init(nodes, replications);

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
                ring.GetNode(i.ToString(CultureInfo.InvariantCulture));
            long murmurMs = sw.ElapsedMilliseconds;

            Console.WriteLine("ConsistentHashRing Performance:");
            Console.WriteLine("\t{0:#,##0.0#} ops/ms", (float) iterations/murmurMs);
            Console.WriteLine("\t{0:#,##0.0#} ops/s", (float) iterations*1000/murmurMs);
        }

        private Stats RunTest<TNode>(long iterations, BB.Caching.Hashing.ConsistentHashRing<TNode> ring)
        {
            var counts = new Dictionary<TNode, long>(
                ring.GetAvailableNodes().ToDictionary(n => n, n => 0L));

            var random = new Random();
            for (int i = 0; i < iterations; i++)
            {
                string s = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(random.Next(10000000, 1000000000).ToString(CultureInfo.InvariantCulture)));
                counts[ring.GetNode(s)] += 1;
            }

            double mean = counts.Values.Average();
            double sdev = 0;
// ReSharper disable LoopCanBeConvertedToQuery
            foreach (long count in counts.Values)
// ReSharper restore LoopCanBeConvertedToQuery
            {
                double diff = count - mean;
                sdev += diff*diff;
            }
            sdev = Math.Sqrt(sdev/iterations);

            return new Stats(mean, sdev, counts.Values.Sum());
        }

        [Fact]
        public void NodeWeightSameTest()
        {
            const long iterations = 100000;
            const int replications = 512;
            var nodes = new Dictionary<string, int>
                {
                    {"node1", 1},
                    {"node2", 1},
                    {"node3", 1}
                };

            var ring = new BB.Caching.Hashing.ConsistentHashRing<string>();
            ring.Init(nodes, replications);

            var counts = new Dictionary<string, long>(
                ring.GetAvailableNodes().ToDictionary(n => n, n => 0L));

            var random = new Random();
            for (int i = 0; i < iterations; i++)
            {
                string s = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(random.Next(10000000, 1000000000).ToString(CultureInfo.InvariantCulture)));
                counts[ring.GetNode(s)] += 1;
            }

            float perUnit = (float) iterations/nodes.Sum(x => x.Value);

            foreach (var kvp in counts.OrderBy(k => k.Key))
                Console.WriteLine("{0}: {1:#,##0} (target {2:#,##0})", kvp.Key, kvp.Value, nodes[kvp.Key] * perUnit);

            double singleWeightExpectedValue = (double) iterations/nodes.Values.Sum();
            double allowedError = singleWeightExpectedValue*0.15;

            foreach (var kvp in counts.OrderBy(k => k.Key))
            {
                Assert.True(nodes[kvp.Key]*singleWeightExpectedValue - allowedError <= kvp.Value);
                Assert.True(nodes[kvp.Key]*singleWeightExpectedValue + allowedError >= kvp.Value);
            }
        }

        [Fact]
        public void NodeWeightVariesTest()
        {
            const long iterations = 100000;
            const int replications = 512;
            var nodes = new Dictionary<string, int>
                {
                    {"node1", 1},
                    {"node2", 2},
                    {"node3", 1},
                    {"node4", 4},
                    {"node5", 2}
                };

            var ring = new BB.Caching.Hashing.ConsistentHashRing<string>();
            ring.Init(nodes, replications);

            var counts = new Dictionary<string, long>(
                ring.GetAvailableNodes().ToDictionary(n => n, n => 0L));

            var random = new Random();
            for (int i = 0; i < iterations; i++)
            {
                string s = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(random.Next(10000000, 1000000000).ToString(CultureInfo.InvariantCulture)));
                counts[ring.GetNode(s)] += 1;
            }

            float perUnit = (float) iterations/nodes.Sum(x => x.Value);

            foreach (var kvp in counts.OrderBy(k => k.Key))
                Console.WriteLine("{0}: {1:#,##0} (target {2:#,##0})", kvp.Key, kvp.Value, nodes[kvp.Key] * perUnit);

            double singleWeightExpectedValue = (double) iterations/nodes.Values.Sum();
            double allowedError = singleWeightExpectedValue*0.15;

            foreach (var kvp in counts.OrderBy(k => k.Key))
            {
                Assert.True(nodes[kvp.Key]*singleWeightExpectedValue - allowedError <= kvp.Value);
                Assert.True(nodes[kvp.Key]*singleWeightExpectedValue + allowedError >= kvp.Value);
            }
        }
    }
}