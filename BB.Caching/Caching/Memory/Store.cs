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
            public class Store
            {
                private static readonly Lazy<Store> _lazy = new Lazy<Store>(
                    () => new Store(), LazyThreadSafetyMode.ExecutionAndPublication);

                public static Store Instance
                {
                    get { return Store._lazy.Value; }
                }

                /// <summary>
                /// The <see cref="ObjectCache"/> used to store our in-memory data.
                /// </summary>
                internal ObjectCache CacheStore
                {
                    get;
                    set;
                }

                private Store()
                {
                    this.CacheStore = new MemoryCache("l1-cache");
                }
            }
        }
    }
}
