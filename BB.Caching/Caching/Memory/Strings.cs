using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using BB.Caching.Caching;
using BB.Caching.Compression;

// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    public static partial class Cache
    {
        public static partial class Memory
        {
            public static class Strings
            {
                // TODO need to lock when performing a getset (might be fine without it?)

                /// <summary>
                /// Sets the <paramref name="value"/> at <paramref name="key"/>.
                /// </summary>
                /// <param name="key"></param>
                /// <param name="value"></param>
                public static byte[] Set(string key, object value)
                {
                    byte[] result = Compress.Compression.Compress(value);
                    Cache.Memory.Core.Instance.CacheStore.Set(key, result, null);
                    return result;
                }

                /// <summary>
                /// Sets the <paramref name="value"/> at <paramref name="key"/> asynchronously.
                /// </summary>
                /// <param name="key"></param>
                /// <param name="value"></param>
                public static async Task<byte[]> SetAsync(string key, object value)
                {
                    byte[] compressed = await Compress.Compression.CompressAsync(value);
                    Cache.Memory.Core.Instance.CacheStore.Set(key, compressed, null);
                    return compressed;
                }

                /// <summary>
                /// Sets the <paramref name="value"/> at <paramref name="key"/>, expiring after
                /// <paramref name="absoluteExpiration"/>.
                /// </summary>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="absoluteExpiration"></param>
                public static byte[] Set(string key, object value, TimeSpan absoluteExpiration)
                {
                    byte[] result = Compress.Compression.Compress(value);
                    Cache.Memory.Core.Instance.CacheStore.Set(key, result, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.UtcNow.Add(absoluteExpiration)
                    });
                    return result;
                }

                /// <summary>
                /// Sets the <paramref name="value"/> at <paramref name="key"/> asynchronously, expiring after
                /// <paramref name="absoluteExpiration"/>.
                /// </summary>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="absoluteExpiration"></param>
                public static async Task<byte[]> SetAsync(string key, object value, TimeSpan absoluteExpiration)
                {
                    byte[] compressed = await Compress.Compression.CompressAsync(value);
                    Cache.Memory.Core.Instance.CacheStore.Set(key, compressed, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.UtcNow.Add(absoluteExpiration)
                    });
                    return compressed;
                }

                /// <summary>
                /// Sets the <paramref name="value"/> at <paramref name="key"/>, expiring after
                /// <paramref name="slidingExpiration"/>.
                /// <remarks>
                /// Sliding expirations only work when they are a minimum of 1 second long.
                /// http://social.msdn.microsoft.com/Forums/vstudio/en-US/b5f56edd-ce71-40e2-9d9a-ba32df74489e/cant-make-sliding-expiration-on-memorycache-work-bug#e4cc5d7e-3e8a-49ee-99a0-5515bdb2f1c7
                /// </remarks>
                /// </summary>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="slidingExpiration"></param>
                public static byte[] SetSliding(string key, object value, TimeSpan slidingExpiration)
                {
                    byte[] result = Compress.Compression.Compress(value);
                    Cache.Memory.Core.Instance.CacheStore.Set(key, result, new CacheItemPolicy
                    {
                        SlidingExpiration = slidingExpiration
                    });
                    return result;
                }

                /// <summary>
                /// Sets the <paramref name="value"/> at <paramref name="key"/> asynchronously, expiring after
                /// <paramref name="slidingExpiration"/>.
                /// <remarks>
                /// Sliding expirations only work when they are a minimum of 1 second long.
                /// http://social.msdn.microsoft.com/Forums/vstudio/en-US/b5f56edd-ce71-40e2-9d9a-ba32df74489e/cant-make-sliding-expiration-on-memorycache-work-bug#e4cc5d7e-3e8a-49ee-99a0-5515bdb2f1c7
                /// </remarks>
                /// </summary>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="slidingExpiration"></param>
                public static async Task<byte[]> SetSlidingAsync(string key, object value, TimeSpan slidingExpiration)
                {
                    byte[] result = await Compress.Compression.CompressAsync(value);
                    Cache.Memory.Core.Instance.CacheStore.Set(key, result, new CacheItemPolicy
                    {
                        SlidingExpiration = slidingExpiration
                    });
                    return result;
                }

                /// <summary>
                /// Get the <see cref="TObject"/> stored at <paramref name="key"/>.
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <returns></returns>
                public static MemoryValue<TObject> Get<TObject>(string key)
                {
                    byte[] b = Cache.Memory.Core.Instance.CacheStore.Get(key) as byte[];

                    if (null == b)
                    {
                        return MemoryValue<TObject>.Null;
                    }

                    TObject value = Compress.Compression.Decompress<TObject>(b);
                    return new MemoryValue<TObject>(value, true);
                }

                /// <summary>
                /// Get the <see cref="TObject"/> stored at <paramref name="key"/> asynchronously.
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <returns></returns>
                public static async Task<MemoryValue<TObject>> GetAsync<TObject>(string key)
                {
                    byte[] b = Cache.Memory.Core.Instance.CacheStore.Get(key) as byte[];
                    if (null == b)
                    {
                        return MemoryValue<TObject>.Null;
                    }

                    TObject value = await Compress.Compression.DecompressAsync<TObject>(b);
                    return new MemoryValue<TObject>(value, true);
                }

                /// <summary>
                /// Deletes any data stored at <paramref name="key"/>, if the key exists.
                /// </summary>
                /// <param name="key"></param>
                public static void Delete(string key)
                {
                    Cache.Memory.Core.Instance.CacheStore.Remove(key);
                }

                /// <summary>
                /// Checks to see if the <paramref name="key"/> exists.
                /// </summary>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool Exists(string key)
                {
                    return Cache.Memory.Core.Instance.CacheStore.Contains(key);
                }

                /// <summary>
                /// Gets the number of items currently in the cache.
                /// </summary>
                /// <returns></returns>
                public static long GetCount()
                {
                    return Cache.Memory.Core.Instance.CacheStore.GetCount();
                }

                /// <summary>
                /// Clears the cache.
                /// </summary>
                public static void Clear()
                {
                    foreach (var kvp in Cache.Memory.Core.Instance.CacheStore)
                    {
                        Cache.Memory.Core.Instance.CacheStore.Remove(kvp.Key);
                    }
                }

                /// <summary>
                /// Gets the data stored at <paramref name="key"/>, and updates the data to <paramref name="value"/>.
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <returns></returns>
                public static MemoryValue<TObject> GetSet<TObject>(string key, object value)
                {
                    byte[] b = Cache.Memory.Core.Instance.CacheStore.Get(key) as byte[];
                    Cache.Memory.Core.Instance.CacheStore.Set(
                        key, Compress.Compression.Compress(value), null);

                    if (null == b)
                    {
                        return MemoryValue<TObject>.Null;
                    }

                    TObject result = Compress.Compression.Decompress<TObject>(b);
                    return new MemoryValue<TObject>(result, true);
                }

                /// <summary>
                /// Gets the data stored at <paramref name="key"/> asynchronously, and updates the data to
                /// <paramref name="value"/>.
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <returns></returns>
                public static async Task<MemoryValue<TObject>> GetSetAsync<TObject>(string key, object value)
                {
                    byte[] b = Cache.Memory.Core.Instance.CacheStore.Get(key) as byte[];
                    Cache.Memory.Core.Instance.CacheStore.Set(
                        key, Compress.Compression.Compress(value), null);

                    if (null == b)
                    {
                        return MemoryValue<TObject>.Null;
                    }

                    TObject result = await Compress.Compression.DecompressAsync<TObject>(b);
                    return new MemoryValue<TObject>(result, true);
                }

                /// <summary>
                /// Gets the data stored at <paramref name="key"/> and updates the data to <paramref name="value"/>, expiring
                /// after <paramref name="absoluteExpiration"/>.
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="absoluteExpiration"></param>
                /// <returns></returns>
                public static MemoryValue<TObject> GetSet<TObject>(string key, object value, TimeSpan absoluteExpiration)
                {
                    byte[] b = Cache.Memory.Core.Instance.CacheStore.Get(key) as byte[];
                    Cache.Memory.Core.Instance.CacheStore.Set(
                        key, Compress.Compression.Compress(value), new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.UtcNow.Add(absoluteExpiration)
                    });

                    if (null == b)
                    {
                        return MemoryValue<TObject>.Null;
                    }

                    TObject result = Compress.Compression.Decompress<TObject>(b);
                    return new MemoryValue<TObject>(result, true);
                }

                /// <summary>
                /// Gets the data stored at <paramref name="key"/> and updates the data to <paramref name="value"/>
                /// asynchronously, expiring after <paramref name="absoluteExpiration"/>.
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="absoluteExpiration"></param>
                /// <returns></returns>
                public static async Task<MemoryValue<TObject>> GetSetAsync<TObject>(
                    string key, object value, TimeSpan absoluteExpiration)
                {
                    byte[] b = Cache.Memory.Core.Instance.CacheStore.Get(key) as byte[];
                    Cache.Memory.Core.Instance.CacheStore.Set(
                        key, Compress.Compression.Compress(value), new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.UtcNow.Add(absoluteExpiration)
                    });

                    if (null == b)
                    {
                        return MemoryValue<TObject>.Null;
                    }

                    TObject result = await Compress.Compression.DecompressAsync<TObject>(b);
                    return new MemoryValue<TObject>(result, true);
                }

                /// <summary>
                /// Gets the data stored at <paramref name="key"/> and updates the data to <paramref name="value"/>,
                /// expiring after <paramref name="slidingExpiration"/>.
                /// <remarks>
                /// Sliding expirations only work when they are a minimum of 1 second long.
                /// http://social.msdn.microsoft.com/Forums/vstudio/en-US/b5f56edd-ce71-40e2-9d9a-ba32df74489e/cant-make-sliding-expiration-on-memorycache-work-bug#e4cc5d7e-3e8a-49ee-99a0-5515bdb2f1c7
                /// </remarks>
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="slidingExpiration"></param>
                /// <returns></returns>
                public static MemoryValue<TObject> GetSetSliding<TObject>(string key, object value, TimeSpan slidingExpiration)
                {
                    byte[] b = Cache.Memory.Core.Instance.CacheStore.Get(key) as byte[];
                    Cache.Memory.Core.Instance.CacheStore.Set(
                        key, Compress.Compression.Compress(value), new CacheItemPolicy
                    {
                        SlidingExpiration = slidingExpiration
                    });

                    if (null == b)
                    {
                        return MemoryValue<TObject>.Null;
                    }

                    TObject result = Compress.Compression.Decompress<TObject>(b);
                    return new MemoryValue<TObject>(result, true);
                }

                /// <summary>
                /// Gets the data stored at <paramref name="key"/> and updates the data to <paramref name="value"/>
                /// asynchronously, expiring after <paramref name="slidingExpiration"/>.
                /// <remarks>
                /// Sliding expirations only work when they are a minimum of 1 second long.
                /// http://social.msdn.microsoft.com/Forums/vstudio/en-US/b5f56edd-ce71-40e2-9d9a-ba32df74489e/cant-make-sliding-expiration-on-memorycache-work-bug#e4cc5d7e-3e8a-49ee-99a0-5515bdb2f1c7
                /// </remarks>
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="slidingExpiration"></param>
                /// <returns></returns>
                public static async Task<MemoryValue<TObject>> GetSetSlidingAsync<TObject>(
                    string key, object value, TimeSpan slidingExpiration)
                {
                    byte[] b = Cache.Memory.Core.Instance.CacheStore.Get(key) as byte[];
                    Cache.Memory.Core.Instance.CacheStore.Set(
                        key, Compress.Compression.Compress(value), new CacheItemPolicy
                    {
                        SlidingExpiration = slidingExpiration
                    });

                    if (null == b)
                    {
                        return MemoryValue<TObject>.Null;
                    }

                    TObject result = await Compress.Compression.DecompressAsync<TObject>(b);
                    return new MemoryValue<TObject>(result, true);
                }
            }
        }
    }
}
