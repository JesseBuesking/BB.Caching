namespace BB.Caching.Tests.Hashing
{
    using Xunit;

    public class Murmur3Tests
    {
        // TODO figure out how to test the implementations correctness
        [Fact]
        public void HashesTheSameValue()
        {
            const int ITERATIONS = 2000;
            const string HASH_ME = "Hash me please!";

            ulong lastValue = 0;
            for (int i = 0; i < ITERATIONS; i++)
            {
                ulong value = BB.Caching.Hashing.Murmur3.ComputeInt(HASH_ME);

                if (0 != i)
                {
                    Assert.Equal(lastValue, value);
                }

                lastValue = value;
            }
        }
    }
}