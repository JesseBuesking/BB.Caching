using System;
using System.Collections.Generic;
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
        public void HashRingWithinAllowedError()
        {
            const long iterations = 100000;
            const int replications = 128;
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

            var counts = new Dictionary<string, long>(
                ring.GetAvailableNodes().ToDictionary(n => n, n => 0L));

            var random = new Random();
            for (int i = 0; i < iterations; i++)
            {
                string s = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(random.Next(10000000, 1000000000).ToString(CultureInfo.InvariantCulture)));
                counts[ring.GetNode(s)] += 1;
            }

            double mean = counts.Values.Average();
            double sdev = counts.Values.Select(count => count - mean).Select(diff => diff*diff).Sum();
            sdev = Math.Sqrt(sdev/iterations);

            var murmur3Stats = new Stats(mean, sdev, counts.Values.Sum());

            Console.WriteLine("ConsistentHashRing Stress Test:");
            Console.WriteLine("iterations: {0:#,##0}", iterations);
            Console.WriteLine("replications: {0:#,##0}", replications);
            Console.WriteLine("murmur3 sdev: {0:#,##0.0#}%", murmur3Stats.SDev);

            Assert.Equal(iterations, murmur3Stats.Count);

            Assert.True(murmur3Stats.SDev < 20.0);
        }

        [Fact]
        public void NodeWeightSameTest()
        {
            const long iterations = 100000;
            const int replications = 128;
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
            const int replications = 128;
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
            double allowedError = singleWeightExpectedValue*0.20;

            foreach (var kvp in counts.OrderBy(k => k.Key))
            {
                Assert.True(nodes[kvp.Key]*singleWeightExpectedValue - allowedError <= kvp.Value);
                Assert.True(nodes[kvp.Key]*singleWeightExpectedValue + allowedError >= kvp.Value);
            }
        }
    }
}