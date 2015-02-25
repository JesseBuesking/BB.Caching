using System;
using System.Runtime.Caching;
using BB.Caching.Compression;

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
                /// Tries to get the <see cref="TObject"/> stored at <paramref name="key"/>.
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <returns></returns>
                public static bool TryGet<TObject>(string key, out TObject value)
                {
                    byte[] b = Cache.Memory.Store.Instance.CacheStore.Get(key) as byte[];

                    if (null == b)
                    {
                        value = default(TObject);
                        return false;
                    }

                    value = Compress.Compression.Decompress<TObject>(b);
                    return true;
                }

                /// <summary>
                /// Sets <paramref name="value"/> at <paramref name="key"/>.
                /// </summary>
                /// <param name="key"></param>
                /// <param name="value"></param>
                public static byte[] Set(string key, object value)
                {
                    byte[] result = Compress.Compression.Compress(value);
                    Cache.Memory.Store.Instance.CacheStore.Set(key, result, null);
                    return result;
                }

                /// <summary>
                /// Sets <paramref name="value"/> at <paramref name="key"/>, expiring after
                /// <paramref name="absoluteExpiration"/>.
                /// </summary>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="absoluteExpiration"></param>
                public static byte[] Set(string key, object value, TimeSpan absoluteExpiration)
                {
                    byte[] result = Compress.Compression.Compress(value);
                    Cache.Memory.Store.Instance.CacheStore.Set(key, result, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.UtcNow.Add(absoluteExpiration)
                    });
                    return result;
                }

                /// <summary>
                /// Sets <paramref name="value"/> at <paramref name="key"/>, expiring after
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
                    Cache.Memory.Store.Instance.CacheStore.Set(key, result, new CacheItemPolicy
                    {
                        SlidingExpiration = slidingExpiration
                    });
                    return result;
                }

                /// <summary>
                /// Deletes any data stored at <paramref name="key"/>, if the key exists.
                /// </summary>
                /// <param name="key"></param>
                public static void Remove(string key)
                {
                    Cache.Memory.Store.Instance.CacheStore.Remove(key);
                }

                /// <summary>
                /// Checks to see if the <paramref name="key"/> exists.
                /// </summary>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool Exists(string key)
                {
                    return Cache.Memory.Store.Instance.CacheStore.Contains(key);
                }

                /// <summary>
                /// Gets the number of items currently in the cache.
                /// </summary>
                /// <returns></returns>
                public static long GetCount()
                {
                    return Cache.Memory.Store.Instance.CacheStore.GetCount();
                }

                /// <summary>
                /// Clears the cache.
                /// </summary>
                public static void Clear()
                {
                    foreach (var kvp in Cache.Memory.Store.Instance.CacheStore)
                    {
                        Cache.Memory.Store.Instance.CacheStore.Remove(kvp.Key);
                    }
                }

                /// <summary>
                /// Gets the data stored at <paramref name="key"/>, and updates the data to <paramref name="value"/>.
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="store"></param>
                /// <returns></returns>
                public static bool TryGetSet<TObject>(string key, object value, out TObject store)
                {
                    byte[] b = Cache.Memory.Store.Instance.CacheStore.Get(key) as byte[];
                    Cache.Memory.Store.Instance.CacheStore.Set(
                        key, Compress.Compression.Compress(value), null);

                    if (null == b)
                    {
                        store = default(TObject);
                        return false;
                    }

                    store = Compress.Compression.Decompress<TObject>(b);
                    return true;
                }

                /// <summary>
                /// Gets the data stored at <paramref name="key"/> and updates the data to <paramref name="value"/>, expiring
                /// after <paramref name="absoluteExpiration"/>.
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="absoluteExpiration"></param>
                /// <param name="store"></param>
                /// <returns></returns>
                public static bool TryGetSet<TObject>(string key, object value, TimeSpan absoluteExpiration,
                    out TObject store)
                {
                    byte[] b = Cache.Memory.Store.Instance.CacheStore.Get(key) as byte[];
                    Cache.Memory.Store.Instance.CacheStore.Set(
                        key, Compress.Compression.Compress(value), new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.UtcNow.Add(absoluteExpiration)
                    });

                    if (null == b)
                    {
                        store = default(TObject);
                        return false;
                    }

                    store = Compress.Compression.Decompress<TObject>(b);
                    return true;
                }

                /// <summary>
                /// Gets the data stored at <paramref name="key"/> and updates the data to <paramref name="value"/>, expiring
                /// after <paramref name="slidingExpiration"/>.
                /// <remarks>
                /// Sliding expirations only work when they are a minimum of 1 second long.
                /// http://social.msdn.microsoft.com/Forums/vstudio/en-US/b5f56edd-ce71-40e2-9d9a-ba32df74489e/cant-make-sliding-expiration-on-memorycache-work-bug#e4cc5d7e-3e8a-49ee-99a0-5515bdb2f1c7
                /// </remarks>
                /// </summary>
                /// <typeparam name="TObject"></typeparam>
                /// <param name="key"></param>
                /// <param name="value"></param>
                /// <param name="slidingExpiration"></param>
                /// <param name="store"></param>
                /// <returns></returns>
                public static bool TryGetSetSliding<TObject>(string key, object value, TimeSpan slidingExpiration,
                    out TObject store)
                {
                    byte[] b = Cache.Memory.Store.Instance.CacheStore.Get(key) as byte[];
                    Cache.Memory.Store.Instance.CacheStore.Set(
                        key, Compress.Compression.Compress(value), new CacheItemPolicy
                    {
                        SlidingExpiration = slidingExpiration
                    });

                    if (null == b)
                    {
                        store = default(TObject);
                        return false;
                    }

                    store = Compress.Compression.Decompress<TObject>(b);
                    return true;
                }
            }
        }
    }
}
