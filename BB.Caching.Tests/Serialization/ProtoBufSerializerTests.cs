namespace BB.Caching.Tests.Serialization
{
    using System;

    using BB.Caching.Serialization;

    using ProtoBuf;

    using Xunit;

    public class ProtoBufSerializerTests : IUseFixture<DefaultTestFixture>
    {
        public enum TestEnum
        {
            One = 1,
            Two = 2,
            Three = 3
        }

        public interface IInterface
        {
            byte[] Copy();
        }

        [Fact]
        public void SerializingAString()
        {
            const string S = "I'm a string";
            ProtoBufSerializer.Serialize(S);
        }

        [Fact]
        public void TestSerializePrimitives()
        {
            // ReSharper disable once JoinDeclarationAndInitializer
            byte[] v;

            v = ProtoBufSerializer.Serialize(1);
            Assert.Equal(1, ProtoBufSerializer.Deserialize<short>(v));

            v = ProtoBufSerializer.Serialize(12);
            Assert.Equal(12, ProtoBufSerializer.Deserialize<int>(v));

            v = ProtoBufSerializer.Serialize(1123L);
            Assert.Equal(1123L, ProtoBufSerializer.Deserialize<long>(v));

            v = ProtoBufSerializer.Serialize(1.234f);
            Assert.Equal(1.234f, ProtoBufSerializer.Deserialize<float>(v));

            v = ProtoBufSerializer.Serialize("hello there");
            Assert.Equal("hello there", ProtoBufSerializer.Deserialize<string>(v));

            v = ProtoBufSerializer.Serialize(TestEnum.Two);
            Assert.Equal(TestEnum.Two, ProtoBufSerializer.Deserialize<TestEnum>(v));
        }

        public void SetFixture(DefaultTestFixture data)
        {
        }

        #region test classes

        public class TestHasDateTime
        {
            public DateTime DateTime
            {
                get;
                set;
            }
        }

        public class TestSubInt : IInterface
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

        public class TestSuperInt
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

        public class TestSuper
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
        public class TestClass
        {
            public readonly int Int;

            public TestClass(int @int)
            {
                this.Int = @int;
            }

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

            public string FullName
            {
                get { return this.FirstName + " " + this.LastName; }
            }
        }

        [ProtoContract]
        [Serializable] // Needed for binary serialization
        public class ProtoClass
        {
            [ProtoMember(3)]
#pragma warning disable 169
            private int _int;
#pragma warning restore 169

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

            public string FullName
            {
                get { return this.FirstName + " " + this.LastName; }
            }
        }

        #endregion // test classes
    }
}