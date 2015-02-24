using Xunit;

namespace BB.Caching.Tests.Hashing
{
    public class Murmur3Tests
    {
        // TODO figure out how to test the implementations correctness

        [Fact]
        public void HashesTheSameValue()
        {
            const int iterations = 2000;
            const string hashMe = "Hash me please!";

            ulong lastValue = 0;
            for (int i = 0; i < iterations; i++)
            {
                ulong value = BB.Caching.Hashing.Murmur3.ComputeInt(hashMe);

                if (0 != i)
                {
                    Assert.Equal(lastValue, value);
                }

                lastValue = value;
            }
        }
    }
}