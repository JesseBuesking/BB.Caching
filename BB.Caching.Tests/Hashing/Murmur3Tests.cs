using System;
using System.Diagnostics;
using Xunit;

namespace BB.Caching.Tests.Hashing
{
    public class Murmur3Tests
    {
        // TODO figure out how to test the implementations correctness

        [Fact]
        public void Performance()
        {
            const int iterations = 300000;
            const string hashMe = "Hash me please!";

            ulong lastValue = 0;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                ulong value = BB.Caching.Hashing.Murmur3.ComputeInt(hashMe);
                if (0 != i)
                    Assert.Equal(lastValue, value);
                lastValue = value;
            }
            long murmur3Ms = sw.ElapsedMilliseconds;


            Console.WriteLine("Murmur3 Hashes:");
            Console.WriteLine("\t{0:#,##0.0#} ops/ms", (float) iterations/murmur3Ms);
            Console.WriteLine("\t{0:#,##0.0#} ops/s", (float) iterations*1000/murmur3Ms);
        }
    }
}