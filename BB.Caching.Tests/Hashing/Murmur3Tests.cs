using System;
using System.Diagnostics;
using BB.Caching.Hashing;
using Xunit;

namespace BB.Caching.Tests.Hashing
{
    public class Murmur3Tests
    {
        // TODO figure out how to test the implementations correctness

        [Fact]
        public void Murmur3VsMd5Performance()
        {
            const int iterations = 300000;
            const string hashMe = "Hash me please!";
            var murmur3 = Murmur3.Instance;
            var md5 = new Md5();

            ulong lastValue = 0;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                ulong value = murmur3.ComputeInt(hashMe);
                if (0 != i)
                    Assert.Equal(lastValue, value);
                lastValue = value;
            }
            long murmur3Ms = sw.ElapsedMilliseconds;

            lastValue = 0;
            sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                ulong value = md5.ComputeInt(hashMe);
                if (0 != i)
                    Assert.Equal(lastValue, value);
                lastValue = value;
            }
            long md5Ms = sw.ElapsedMilliseconds;

            Console.WriteLine("Murmur3 Hashes:");
            Console.WriteLine("\t{0:#,##0.0#} ops/ms", (float) iterations/murmur3Ms);
            Console.WriteLine("\t{0:#,##0.0#} ops/s", (float) iterations*1000/murmur3Ms);

            Console.WriteLine();
            Console.WriteLine("murmur3 vs md5: {0:#,##0.0#}%", ((float) murmur3Ms/(md5Ms))*100);
            Console.WriteLine("murmur3: {0:#,##0}ms", murmur3Ms);
            Console.WriteLine("md5: {0:#,##0}ms", md5Ms);
        }
    }
}