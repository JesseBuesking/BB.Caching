// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    using System;
    using System.Threading;

    using Murmur;

    /// <summary>
    /// Methods and classes for hashing data.
    /// </summary>
    public static partial class Hashing
    {
        /// <summary>
        /// Core methods for hashing.
        /// </summary>
        public class Core
        {
            /// <summary>
            /// Lazily loads the instance.
            /// </summary>
            private static readonly Lazy<Core> _Lazy = new Lazy<Core>(
                () => new Core(), LazyThreadSafetyMode.ExecutionAndPublication);

            /// <summary>
            /// Gets the instance.
            /// </summary>
            public static Core Instance
            {
                get { return _Lazy.Value; }
            }

            /// <summary>
            /// The murmur hash.
            /// </summary>
            internal Murmur32 MurmurHash { get; private set; }

            /// <summary>
            /// Prevents a default instance of the <see cref="Core"/> class from being created.
            /// </summary>
            private Core()
            {
                this.Initialize();
            }

            /// <summary>
            /// Initializes the hashing core.
            /// </summary>
            /// <param name="seed">
            /// The seed value to use for the murmurhash.
            /// </param>
            /// <param name="managed">
            /// Whether murmurhash should be in managed space or unmanaged.
            /// </param>
            public void Initialize(uint seed = 0, bool managed = true)
            {
                this.MurmurHash = Murmur.MurmurHash.Create32(seed, managed);
            }
        }
    }
}
