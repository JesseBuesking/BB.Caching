using System;
using System.Runtime.Caching;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    public static partial class Cache
    {
        /// <summary>
        /// An in-memory cache specific to the current machine.
        /// </summary>
        public static partial class Memory
        {
            public class Core
            {
                private static readonly Lazy<Core> _lazy = new Lazy<Core>(
                    () => new Core(), LazyThreadSafetyMode.ExecutionAndPublication);

                public static Core Instance
                {
                    get { return Core._lazy.Value; }
                }

                /// <summary>
                /// The <see cref="ObjectCache"/> used to store our in-memory data.
                /// </summary>
                internal ObjectCache CacheStore
                {
                    get;
                    set;
                }

                private Core()
                {
                    this.CacheStore = new MemoryCache("l1-cache");
                }
            }
        }
    }
}
