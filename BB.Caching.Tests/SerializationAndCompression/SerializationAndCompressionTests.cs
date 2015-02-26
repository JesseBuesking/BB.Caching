using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Caching;
using System.Text;
using BB.Caching.Compression;
using BB.Caching.Serialization;
using ProtoBuf;
using Xunit;

namespace BB.Caching.Tests.SerializationAndCompression
{
    public class SerializationAndCompressionTests
    {
        private static readonly Random _random = new Random(1);

        [ProtoContract]
        private class OneLong
        {
            [ProtoMember(1)]
            public long Id
            {
// ReSharper disable once UnusedAutoPropertyAccessor.Local
                private get;
                set;
            }
        }

        [ProtoContract]
        private class MultipleProperties
        {
            [ProtoMember(1)]
            public long Id
            {
// ReSharper disable once UnusedAutoPropertyAccessor.Local
                private get;
                set;
            }

            [ProtoMember(2)]
            public string Name
            {
// ReSharper disable once UnusedAutoPropertyAccessor.Local
                private get;
                set;
            }
        }

        private static long GenerateId()
        {
            return _random.Next(1, int.MaxValue);
        }

        /// <summary>
        /// Generates a string.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <param name="repeatSize">
        /// The size of repeats.
        /// 
        /// E.g. if repeatSize = 2 and length = 10: "aabbccddee" or "aa2211ssdd"
        /// E.g. if repeatSize = 1 and length = 10: "abcdefghij" or "aacdefghij"
        /// </param>
        /// <returns></returns>
        private static string GenerateString(int length, int repeatSize)
        {
            int iterations = 0 == repeatSize ? length : length/repeatSize;
            repeatSize = 0 == repeatSize ? 1 : repeatSize;

            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < iterations; i++)
                sb.Append((char) _random.Next(32, 127), repeatSize);
            return sb.ToString();
        }

        [Fact]
        public void Performance()
        {
            MemorySpeedPerformance();
            Console.WriteLine("");
            MemorySizePerformance();
        }

        private static void MemorySpeedPerformance()
        {
            /*
             * Some numbers:
             *     1. keep in mind, this is done with blocking => ...value).Result
             *     2. all tests were ran in debug mode
             * Remarks:
             *     Running the async methods synchronously adds a *lot* of overhead, so the values are probably
             *     exaggerated quite a bit (since they were ran using the async versions).
             *
             *     Based on the figures, it looks like you can store 1m raw objects with 10k strings in < 1s. However
             *     at ~70s, you can compress and store those same objects but use ~1/50th the space. Neato!
             * ---------------------------------------------------------------------------------------------------------
             * 1m iterations @ OneLong
             * ---------------------------------------------------------------------------------------------------------
             *                                    Blocking Async    No Async
             * Serialized Set:         6,921,999         1,962ms     2,262ms
             * Serialized Get:         3,529,009           929ms     1,055ms
             * Compression Set:      177,613,772        54,904ms    22,462ms
             * Decompression Get:      4,610,846         1,248ms     1,096ms
             * Raw Set:                3,317,033         1,046ms     1,063ms
             * Raw Get:                  472,352           139ms       141ms
             *
             *                                          No Async
             * Serialized v Raw Get:    7.47114x         5.7688x
             * Serialized v Raw Set:    2.08680x         1.9814x
             * Compression v Raw Get:   9.76146x         6.2608x
             * Compression v Raw Set:  53.54598x        20.6647x
             *
             * ---------------------------------------------------------------------------------------------------------
             * 1m iterations @ MultipleProperties w/ GenerateString(1, 1) (Ran using the synchronous methods)
             * (From this we can see that in the simplest case we pay the cost for compressing the data, but when it
             *  doesn't improve the result (non-compressed size is better than compressed), compressed data retrieval is
             *  nearly the same cost as serialized-only retrieval (because the smart bit is 0)).
             * ---------------------------------------------------------------------------------------------------------
             * Serialized Set:         8,010,415
             * Serialized Get:         3,784,092
             * Compression Set:       50,185,964
             * Decompression Get:      4,051,573
             * Raw Set:                3,437,408
             * Raw Get:                  465,960
             * 
             *                                          No Async
             * Serialized v Raw Get:                     8.1210x
             * Serialized v Raw Set:                     2.3303x
             * Compression v Raw Get:                    8.6951x
             * Compression v Raw Set:                   14.5999x
             *
             * ---------------------------------------------------------------------------------------------------------
             * 1m iterations @ MultipleProperties w/ GenerateString(128, 128)
             * ---------------------------------------------------------------------------------------------------------
             * Serialized Set:         8,138,454
             * Serialized Get:         3,425,754
             * Compression Set:      316,601,410
             * Decompression Get:     76,957,352
             * Raw Set:                3,389,203
             * Raw Get:                  463,274
             *
             *                                          No Async
             * Serialized v Raw Get:     7.3946x         5.5158x
             * Serialized v Raw Set:     2.4012x         2.5692x
             * Compression v Raw Get:  166.1163x        36.6555x
             * Compression v Raw Set:   93.4147x        31.4675x
             *
             * ---------------------------------------------------------------------------------------------------------
             * 1m iterations @ MultipleProperties w/ GenerateString(10000, 10000)
             * ---------------------------------------------------------------------------------------------------------
             * Serialized Set:        53,034,383
             * Serialized Get:         3,893,312
             * Compression Set:      570,493,276
             * Decompression Get:    483,579,832
             * Raw Set:                3,247,694
             * Raw Get:                  462,159
             *
             *                                          No Async
             * Serialized v Raw Get:      8.424x         8.2344x
             * Serialized v Raw Set:     16.329x        15.6698x
             * Compression v Raw Get: 1,046.349x       448.3221x
             * Compression v Raw Set:   175.661x        99.6618x
             */

            const int iterations = 30000;
            const string key = "impt-key";

            var value = new OneLong
                {
                    Id = GenerateId()
                };

            MemoryCache cM = new MemoryCache("cM");
            Stopwatch sw = Stopwatch.StartNew();

            // warmup
            byte[] serialize = ProtoBufSerializer.Serialize(value);
            byte[] compress = SmartCompressor.Instance.Compress(serialize);
            cM.Set(key, compress, null);

            for (int i = 0; i < iterations; i++)
            {
                serialize = ProtoBufSerializer.Serialize(value);
                compress = SmartCompressor.Instance.Compress(serialize);
                cM.Set(key, compress, null);
            }
            long compressSet = sw.ElapsedMilliseconds;

            // warmup
            byte[] compressed = (byte[]) cM.Get(key);
            byte[] decompressed = SmartCompressor.Instance.Decompress(compressed);
            ProtoBufSerializer.Deserialize<OneLong>(decompressed);

            sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                compressed = (byte[]) cM.Get(key);
                decompressed = SmartCompressor.Instance.Decompress(compressed);
                ProtoBufSerializer.Deserialize<OneLong>(decompressed);
            }
            long compressGet = sw.ElapsedMilliseconds;

            MemoryCache sM = new MemoryCache("sM");

            // warmup
            serialize = ProtoBufSerializer.Serialize(value);
            sM.Set(key, serialize, null);

            sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                serialize = ProtoBufSerializer.Serialize(value);
                sM.Set(key, serialize, null);
            }
            long serializeSet = sw.ElapsedMilliseconds;


            // warmup
            compressed = (byte[]) sM.Get(key);
            ProtoBufSerializer.Deserialize<OneLong>(compressed);

            sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                compressed = (byte[]) sM.Get(key);
                ProtoBufSerializer.Deserialize<OneLong>(compressed);
            }
            long serializeGet = sw.ElapsedMilliseconds;

            MemoryCache rM = new MemoryCache("rM");

            // warmup
            rM.Set(key, value, null);

            sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                rM.Set(key, value, null);
            }
            long rawSet = sw.ElapsedMilliseconds;

            // warmup
            rM.Get(key);

            sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                rM.Get(key);
            }
            long rawGet = sw.ElapsedMilliseconds;

            Console.WriteLine("Memory Speed: (operations per second)");
            Console.WriteLine("  Set:");
            Console.WriteLine(
                "    Raw: {0:#,##0.0#}",
                (float) iterations/rawSet * 1000.0
            );
            Console.WriteLine(
                "    Serialized: {0:#,##0.0#} ({1:0.00})%",
                (float) iterations/serializeSet * 1000.0,
                (float) rawSet/serializeSet * 100.0
            );
            Console.WriteLine(
                "    Serialized + Compressed: {0:#,##0.0#} ({1:0.00})%",
                (float) iterations/compressSet * 1000.0,
                (float) rawSet/compressSet * 100.0
            );
            Console.WriteLine("  Get:");
            Console.WriteLine(
                "    Raw: {0:#,##0.0#}",
                (float) iterations/rawGet * 1000.0
            );
            Console.WriteLine(
                "    Serialized: {0:#,##0.0#} ({1:0.00})%",
                (float) iterations/serializeGet * 1000.0,
                (float) rawGet/serializeGet * 100.0
            );
            Console.WriteLine(
                "    Serialized + Compressed: {0:#,##0.0#} ({1:0.00})%",
                (float) iterations/compressGet * 1000.0,
                (float) rawGet/compressGet * 100.0
            );
        }

        private static void MemorySizePerformance()
        {
            /*
             * Some numbers:
             * ---------------------------------------------------------------------------------------------------------
             * 10k objects
             * ---------------------------------------------------------------------------------------------------------
             * maxStringSize            repeatStringSize            serialized vs Raw            compression vs raw
             * 1                        1                            92.4%                        93.8%
             * 17                       1                            88.7%                        90.0%
             * 128                      1                            73.9%                        74.7%
             * 2                        2                            92.7%                        93.8%
             * 17                       2                            86.7%                        89.7%
             * 128                      2                            73.9%                        74.7%
             * 4                        4                            90.5%                        91.6%
             * 17                       4                            86.7%                        89.7%
             * 128                      4                            73.9%                        68.8%
             * 128                      128                          73.9%                        56.8%
             * 
             * 10,000                   10,000                       50.7%                         1.8%
             * 100,000                  100,000                      50.1%                         0.2%
             */

            const int maxStringSize = 10000;
            const int repeatStringSize = 10;
            const int iterations = 1000;
            const string key = "impt-key";

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            long mCs = GC.GetTotalMemory(true);
            MemoryCache cM = new MemoryCache("cM");
            for (int i = 0; i < iterations; i++)
            {
                var m = new MultipleProperties
                    {
                        Id = GenerateId(),
                        Name = GenerateString(maxStringSize, repeatStringSize)
                    };

                byte[] s = ProtoBufSerializer.Serialize(m);
                byte[] c = SmartCompressor.Instance.CompressAsync(s).Result;
                cM.Set(key + i.ToString(CultureInfo.InvariantCulture), c, null);
            }
            long mCe = GC.GetTotalMemory(true);
            long compressMemory = mCe - mCs;

            cM.Trim(100);
            cM.Dispose();
// ReSharper disable RedundantAssignment
            cM = null;
// ReSharper restore RedundantAssignment

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            long mSs = GC.GetTotalMemory(true);
            MemoryCache sM = new MemoryCache("sM");
            for (int i = 0; i < iterations; i++)
            {
                var m = new MultipleProperties
                    {
                        Id = GenerateId(),
                        Name = GenerateString(maxStringSize, repeatStringSize)
                    };

                byte[] s = ProtoBufSerializer.Serialize(m);
                sM.Set(key + i.ToString(CultureInfo.InvariantCulture), s, null);
            }
            long mSe = GC.GetTotalMemory(true);
            long serializeMemory = mSe - mSs;

            sM.Trim(100);
            sM.Dispose();
// ReSharper disable RedundantAssignment
            sM = null;
// ReSharper restore RedundantAssignment

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            long mRs = GC.GetTotalMemory(true);
            MemoryCache rM = new MemoryCache("rM");
            for (int i = 0; i < iterations; i++)
            {
                var m = new MultipleProperties
                    {
                        Id = GenerateId(),
                        Name = GenerateString(maxStringSize, repeatStringSize)
                    };
                rM.Set(key + i.ToString(CultureInfo.InvariantCulture), m, null);
            }
            long mRe = GC.GetTotalMemory(true);
            long rawMemory = mRe - mRs;

            rM.Trim(100);
            rM.Dispose();
// ReSharper disable RedundantAssignment
            rM = null;
// ReSharper restore RedundantAssignment

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("Memory Size:");
            Console.WriteLine("  Raw: {0:#,##0.#}KB", rawMemory/1024.0);
            Console.WriteLine(
                "  Serialized: {0:#,##0.#}KB ({1:0.00}%)",
                serializeMemory/1024.0,
                ((float) serializeMemory/rawMemory)*100
            );
            Console.WriteLine(
                "  Serialized + Compressed: {0:#,##0.#}KB ({1:0.00}%)",
                compressMemory/1024.0,
                ((float) compressMemory/rawMemory)*100
            );
        }
    }
}