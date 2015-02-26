using System;
using System.Threading;
using Murmur;

// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    public static partial class Hashing
    {
        public class Core
        {
            private static readonly Lazy<Core> _lazy = new Lazy<Core>(
                () => new Core(), LazyThreadSafetyMode.ExecutionAndPublication);

            public static Core Instance
            {
                get { return _lazy.Value; }
            }

            internal Murmur32 MurmurHash;

            private Core()
            {
                this.Config();
            }

            public void Config(uint seed = 0, bool managed = true)
            {
                this.MurmurHash = Murmur.MurmurHash.Create32(seed, managed);
            }
        }
    }
}
