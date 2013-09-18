using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using BB.Caching.Connection;
using BB.Caching.Serialization;
using ProtoBuf;
using Xunit;

namespace BB.Caching.Tests.Serialization
{
    public class ProtoBufSerializerTests
    {
        #region test classes

// ReSharper disable MemberCanBePrivate.Global
        public enum TestEnum
// ReSharper restore MemberCanBePrivate.Global
        {
// ReSharper disable UnusedMember.Global
            One = 1,
// ReSharper restore UnusedMember.Global

            Two = 2,

// ReSharper disable UnusedMember.Global
            Three = 3
// ReSharper restore UnusedMember.Global
        }

// ReSharper disable MemberCanBePrivate.Global
        public class TestHasDateTime
// ReSharper restore MemberCanBePrivate.Global
        {
            public DateTime DateTime
            {
                get;
                set;
            }
        }

        public interface IInterface
        {
// ReSharper disable UnusedMember.Global
            byte[] Copy();

// ReSharper restore UnusedMember.Global
        }

// ReSharper disable MemberCanBePrivate.Global
        public class TestSubInt : IInterface
// ReSharper restore MemberCanBePrivate.Global
        {
            public string SubInt
            {
                get;
                set;
            }

            public byte[] Copy()
            {
                return null;
            }
        }

// ReSharper disable MemberCanBePrivate.Global
        public class TestSuperInt
// ReSharper restore MemberCanBePrivate.Global
        {
            public int Super
            {
                get;
                set;
            }

            public IInterface SubInt
            {
                get;
                set;
            }
        }

        public class TestSub
        {
            public int Sub
            {
                get;
                set;
            }
        }

// ReSharper disable MemberCanBePrivate.Global
        public class TestSuper
// ReSharper restore MemberCanBePrivate.Global
        {
            public string Super
            {
                get;
                set;
            }

            public TestSub Sub
            {
                get;
                set;
            }
        }

        [Serializable] // Needed for binary serialization
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

// ReSharper disable UnusedMember.Global
            public string FullName
            {
                get { return this.FirstName + " " + this.LastName; }
            }

// ReSharper restore UnusedMember.Global

            public readonly int MyInt;

            public TestClass(int myInt)
            {
                this.MyInt = myInt;
            }
        }

        [ProtoContract]
        [Serializable] // Needed for binary serialization
// ReSharper disable MemberCanBePrivate.Global
        public class ProtoClass
// ReSharper restore MemberCanBePrivate.Global
        {
            [ProtoMember(1)]
            public string FirstName
            {
                private get;
                set;
            }

            [ProtoMember(2)]
            public string LastName
            {
                private get;
                set;
            }

// ReSharper disable UnusedMember.Global
            public string FullName
            {
                get { return this.FirstName + " " + this.LastName; }
            }

// ReSharper restore UnusedMember.Global

// ReSharper disable NotAccessedField.Global
            [ProtoMember(3)] public int MyInt;

// ReSharper restore NotAccessedField.Global
        }

        #endregion // test classes

        public ProtoBufSerializerTests()
        {
            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-0", new SafeRedisConnection("192.168.2.27", 6379)));

            Cache.Shared.AddRedisConnectionGroup(
                new RedisConnectionGroup("node-1", new SafeRedisConnection("192.168.2.27", 6380)));

            Cache.Shared.SetPubSubRedisConnection(new SafeRedisConnection("192.168.2.27", 6379));

            Cache.Prepare();
        }

        [Fact]
        public void TestSerializePrimitives()
        {
// ReSharper disable JoinDeclarationAndInitializer
            byte[] v;
// ReSharper restore JoinDeclarationAndInitializer

            v = ProtoBufSerializer.Instance.Serialize(1);
            Assert.Equal(1, ProtoBufSerializer.Instance.Deserialize<short>(v));

            v = ProtoBufSerializer.Instance.Serialize(12);
            Assert.Equal(12, ProtoBufSerializer.Instance.Deserialize<int>(v));

            v = ProtoBufSerializer.Instance.Serialize(1123L);
            Assert.Equal(1123L, ProtoBufSerializer.Instance.Deserialize<long>(v));

            v = ProtoBufSerializer.Instance.Serialize(1.234f);
            Assert.Equal(1.234f, ProtoBufSerializer.Instance.Deserialize<float>(v));

            v = ProtoBufSerializer.Instance.Serialize("hello there");
            Assert.Equal("hello there", ProtoBufSerializer.Instance.Deserialize<string>(v));

            v = ProtoBufSerializer.Instance.Serialize(TestEnum.Two);
            Assert.Equal(TestEnum.Two, ProtoBufSerializer.Instance.Deserialize<TestEnum>(v));
        }

        [Fact]
        public void TestAutoSerialization()
        {
            var test = new TestClass(12)
                {
                    FirstName = "Jesse",
                    LastName = "TestCase"
                };

            byte[] serialized = ProtoBufSerializer.Instance.Serialize(test);

            TestClass actual = ProtoBufSerializer.Instance.Deserialize<TestClass>(serialized);

            Assert.Equal(test.FirstName, actual.FirstName);
            Assert.Equal(test.LastName, actual.LastName);
            Assert.Equal(test.MyInt, actual.MyInt);
        }

        [Fact]
        public void TestAutoSerializationWithDateTime()
        {
            var test = new TestHasDateTime
                {
                    DateTime = new DateTime(2013, 01, 01)
                };
            byte[] serialized = ProtoBufSerializer.Instance.Serialize(test);

            TestHasDateTime actual = ProtoBufSerializer.Instance.Deserialize<TestHasDateTime>(serialized);

            Assert.Equal(test.DateTime, actual.DateTime);
        }

        [Fact]
        public void TestAutoSerializationWithSubClass()
        {
            var test = new TestSuper
                {
                    Sub = new TestSub
                        {
                            Sub = 12
                        },
                    Super = "Helloski!"
                };

            byte[] serialized = ProtoBufSerializer.Instance.Serialize(test);

            TestSuper actual = ProtoBufSerializer.Instance.Deserialize<TestSuper>(serialized);

            Assert.Equal(test.Sub.Sub, actual.Sub.Sub);
            Assert.Equal(test.Super, actual.Super);
        }

        [Fact]
        public void TestAutoSerializationWithSubInterface()
        {
            var test = new TestSuperInt
                {
                    SubInt = new TestSubInt
                        {
                            SubInt = "Test!"
                        },
                    Super = 12
                };

            byte[] serialized = ProtoBufSerializer.Instance.Serialize(test);

            TestSuperInt actual = ProtoBufSerializer.Instance.Deserialize<TestSuperInt>(serialized);

            Assert.Equal(((TestSubInt) test.SubInt).SubInt, ((TestSubInt) actual.SubInt).SubInt);
            Assert.Equal(test.Super, actual.Super);
        }

        [Fact]
        public void PerformanceComparison()
        {
            const int iterations = 1000000;
            var auto = new TestClass(12)
                {
                    FirstName = "Jesse",
                    LastName = "TestCase"
                };

            var proto = new ProtoClass
                {
                    FirstName = "Jesse",
                    LastName = "TestCase",
                    MyInt = 12
                };

            // Cache the serializer.
            ProtoBufSerializer.Instance.Serialize(auto);

            byte[] serializeAuto = new byte[0];
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                serializeAuto = ProtoBufSerializer.Instance.Serialize(auto);
            }
            long autoTime = sw.ElapsedTicks;

            byte[] serializeProto = new byte[0];
            sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(ms, proto);
                    serializeProto = ms.ToArray();
                }
            }
            long protoTime = sw.ElapsedTicks;

            byte[] serializeBinary = new byte[0];
            sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, auto);
                    serializeBinary = ms.ToArray();
                }
            }
            long binaryTime = sw.ElapsedTicks;

            string debugInfo = "";
            debugInfo += "auto vs proto size: " + ((float) serializeAuto.Length/serializeProto.Length)*100 + "%\n";
            debugInfo += "auto vs binary size: " + ((float) serializeAuto.Length/serializeBinary.Length)*100 + "%\n";
            debugInfo += "\n";
            debugInfo += "auto vs proto speed: " + ((float) autoTime/protoTime)*100 + "%\n";
            debugInfo += "auto vs binary speed: " + ((float) autoTime/binaryTime)*100 + "%\n";

            Console.WriteLine(debugInfo);

            Assert.Equal(serializeAuto.Length, serializeProto.Length);
            Assert.True(serializeAuto.Length < serializeBinary.Length);
        }
    }
}