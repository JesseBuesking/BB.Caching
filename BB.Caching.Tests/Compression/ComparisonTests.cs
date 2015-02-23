using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using BB.Caching.Compression;
using BB.Caching.Serialization;
using Xunit;

namespace BB.Caching.Tests.Compression
{
    public class ComparisonTests
    {
        [Serializable] // Needed for binary serialization test.
// ReSharper disable MemberCanBePrivate.Global
        public class TestClass
// ReSharper restore MemberCanBePrivate.Global
        {
            public string FirstName
            {
                get;
                set;
            }

            public string LastName
            {
                get;
                set;
            }

            public long Id
            {
                get;
                set;
            }
        }

        private const string VALUE_BAD_COMPRESSION =
            "I am the string that we want to compress, but it's never smaller! :(";

        private const string VALUE_GOOD_COMPRESSION =
            "I am the string that we want to compress, but it's never smaller! :(" +
                "I am the string that we want to compress, but it's never smaller! :(";

        [Fact]
        public void GZipVsSmartBadCompressionTest()
        {
            byte[] smartCompress = SmartCompressor.Instance.CompressAsync(VALUE_BAD_COMPRESSION).Result;
            byte[] gzipCompress = GZipCompressor.Instance.CompressAsync(VALUE_BAD_COMPRESSION).Result;

            Assert.True(gzipCompress.Length > smartCompress.Length);
        }

        [Fact]
        public void GZipVsSmartGoodCompressionTest()
        {
            byte[] smartCompress = SmartCompressor.Instance.CompressAsync(VALUE_GOOD_COMPRESSION).Result;
            byte[] gzipCompress = GZipCompressor.Instance.CompressAsync(VALUE_GOOD_COMPRESSION).Result;

            Assert.True(gzipCompress.Length + 1 == smartCompress.Length);
        }

        [Fact]
        public void ProtoSmartCompressTest()
        {
            var test = new TestClass
                {
                    FirstName = "Jack",
                    LastName = "Jacob",
                    Id = 1
                };

            byte[] proto = ProtoBufSerializer.Instance.Serialize(test);
            byte[] compressed = SmartCompressor.Instance.Compress(proto);

            byte[] decompressed = SmartCompressor.Instance.Decompress(compressed);
            TestClass actual = ProtoBufSerializer.Instance.Deserialize<TestClass>(decompressed);

            Assert.Equal(test.FirstName, actual.FirstName);
            Assert.Equal(test.LastName, actual.LastName);
            Assert.Equal(test.Id, actual.Id);
        }

        [Fact]
        public void BinaryGZipCompressTest()
        {
            var test = new TestClass
                {
                    FirstName = "Jack",
                    LastName = "Jacob",
                    Id = 1
                };

            BinaryFormatter bf = new BinaryFormatter();
            byte[] binary;
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, test);
                binary = ms.ToArray();
            }
            byte[] gzip = GZipCompressor.Instance.Compress(binary);
            byte[] proto = ProtoBufSerializer.Instance.Serialize(test);
            byte[] smarted = SmartCompressor.Instance.Compress(proto);

            string debugInfo = "";
            debugInfo += "proto+smart vs binary+gzip: " + ((float) smarted.Length/gzip.Length)*100 + "%\n";
            Console.WriteLine(debugInfo);

            Assert.True(smarted.Length < gzip.Length);
        }
    }
}