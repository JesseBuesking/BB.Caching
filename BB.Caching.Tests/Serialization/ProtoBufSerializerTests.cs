using System;
using BB.Caching.Redis;
using BB.Caching.Serialization;
using ProtoBuf;
using Xunit;

namespace BB.Caching.Tests.Serialization
{
    public class ProtoBufSerializerTestsFixture : IDisposable
    {
        public ProtoBufSerializerTestsFixture()
        {
            try
            {
                Cache.Prepare();
            }
            catch (PubSub.ChannelAlreadySubscribedException)
            { }
        }

        public void Dispose()
        {
        }
    }

    public class ProtoBufSerializerTests : IUseFixture<DefaultTestFixture>, IUseFixture<ProtoBufSerializerTestsFixture>
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

        [Fact]
        public void SerializingAString()
        {
            const string s = "I'm a string";
            ProtoBufSerializer.Serialize(s);
        }

        [Fact]
        public void TestSerializePrimitives()
        {
// ReSharper disable JoinDeclarationAndInitializer
            byte[] v;
// ReSharper restore JoinDeclarationAndInitializer

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

        public void SetFixture(ProtoBufSerializerTestsFixture data)
        {
        }
    }
}