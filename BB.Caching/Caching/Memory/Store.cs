// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    using System;
    using System.Runtime.Caching;
    using System.Threading;

    /// <summary>
    /// Contains the core methods for caching data in memory, redis, or both.
    /// </summary>
    public static partial class Cache
    {
        /// <summary>
        /// An in-memory cache specific to the current machine.
        /// </summary>
        public static partial class Memory
        {
            /// <summary>
            /// The core of the in-memory caching which manages a single instance to be re-used.
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
                    get { return Core._Lazy.Value; }
                }

                /// <summary>
                /// The <see cref="ObjectCache"/> used to store our in-memory data.
                /// </summary>
                internal ObjectCache CacheStore
                {
                    get;
                    set;
                }

                /// <summary>
                /// Prevents a default instance of the <see cref="Core"/> class from being created.
                /// </summary>
                private Core()
                {
                    this.CacheStore = new MemoryCache("l1-cache");
                }
            }
        }
    }
}
