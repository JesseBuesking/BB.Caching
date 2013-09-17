using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using BB.Caching.Hashing;
using Xunit;

namespace BB.Caching.Tests.Hashing
{
    public class ConsistentHashRingTests
    {
        public class Stats
        {
            public double Mean
            {
                get;
                set;
            }

            public double SDev
            {
                get;
                set;
            }

            public long Count
            {
                get;
                set;
            }

            public Stats(double mean, double sdev, long count)
            {
                this.Mean = mean;
                this.SDev = sdev;
                this.Count = count;
            }
        }

        [Fact]
        public void Murmur3VsMd5HashRingPerformanceTests()
        {
            const long iterations = 1000000;
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

            var ring = new ConsistentHashRing<string>();
            ring.Init(nodes, Murmur3.Instance, replications);

            Stopwatch sw = Stopwatch.StartNew();
            var murmur3Stats = this.RunTest(iterations, ring);
            long murmurMs = sw.ElapsedMilliseconds;

            ring = new ConsistentHashRing<string>();
            ring.Init(nodes, new Md5(), replications);

            sw = Stopwatch.StartNew();
            var md5Stats = this.RunTest(iterations, ring);
            long md5Ms = sw.ElapsedMilliseconds;

            Console.WriteLine("iterations: {0:#,##0}", iterations);
            Console.WriteLine("replications: {0:#,##0}", replications);
            Console.WriteLine("murmur3 sdev: {0:#,##0.0#}%", murmur3Stats.SDev);
            Console.WriteLine("murmur3 time: {0:#,##0}ms", murmurMs);
            Console.WriteLine("md5 sdev: {0:#,##0.0#}%", md5Stats.SDev);
            Console.WriteLine("md5 time: {0:#,##0}ms", md5Ms);

            Assert.Equal(iterations, murmur3Stats.Count);
            Assert.Equal(iterations, md5Stats.Count);

            Assert.True(murmurMs < md5Ms);
            Assert.True(murmur3Stats.SDev < 20.0);
        }

        [Fact]
        public void Murmur3HashRingPerformanceTest()
        {
            const long iterations = 10000000;
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

            var ring = new ConsistentHashRing<string>();
            ring.Init(nodes, Murmur3.Instance, replications);

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
                ring.GetNode(i.ToString(CultureInfo.InvariantCulture));
            long murmurMs = sw.ElapsedMilliseconds;

            Console.WriteLine("{0:#,##0.0#} lookups per ms", (float) iterations/murmurMs);
            Console.WriteLine();
            Console.WriteLine("total ms: {0:#,##0}ms", murmurMs);
        }

        public Stats RunTest<TNode>(long iterations, ConsistentHashRing<TNode> ring)
        {
            var counts = new Dictionary<TNode, long>(
                ring.GetAvailableNodes().ToDictionary(n => n, n => 0L));

            var random = new Random();
            for (int i = 0; i < iterations; i++)
            {
                string s = Convert.ToBase64String(Encoding.UTF8.GetBytes(random.Next(10000000, 1000000000).ToString()));
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
            const long iterations = 1000000;
            const int replications = 512;
            var nodes = new Dictionary<string, int>
                {
                    {"node1", 1},
                    {"node2", 1},
                    {"node3", 1}
                };

            var ring = new ConsistentHashRing<string>();
            ring.Init(nodes, Murmur3.Instance, replications);

            var counts = new Dictionary<string, long>(
                ring.GetAvailableNodes().ToDictionary(n => n, n => 0L));

            var random = new Random();
            for (int i = 0; i < iterations; i++)
            {
                string s = Convert.ToBase64String(Encoding.UTF8.GetBytes(random.Next(10000000, 1000000000).ToString()));
                counts[ring.GetNode(s)] += 1;
            }

            foreach (var kvp in counts.OrderBy(k => k.Key))
                Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);

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
            const long iterations = 1000000;
            const int replications = 512;
            var nodes = new Dictionary<string, int>
                {
                    {"node1", 1},
                    {"node2", 2},
                    {"node3", 1},
                    {"node4", 4},
                    {"node5", 2}
                };

            var ring = new ConsistentHashRing<string>();
            ring.Init(nodes, Murmur3.Instance, replications);

            var counts = new Dictionary<string, long>(
                ring.GetAvailableNodes().ToDictionary(n => n, n => 0L));

            var random = new Random();
            for (int i = 0; i < iterations; i++)
            {
                string s = Convert.ToBase64String(Encoding.UTF8.GetBytes(random.Next(10000000, 1000000000).ToString()));
                counts[ring.GetNode(s)] += 1;
            }

            foreach (var kvp in counts.OrderBy(k => k.Key))
                Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);

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