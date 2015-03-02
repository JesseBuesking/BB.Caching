// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    using System;
    using System.Threading.Tasks;

    using BB.Caching.Caching;
    using BB.Caching.Compression;
    using BB.Caching.Redis;

    using StackExchange.Redis;

    /// <summary>
    /// Contains the core methods for caching data in memory, redis, or both.
    /// </summary>
    public static partial class Cache
    {
        /// <summary>
        /// The channel used to publish and subscribe to configuration removal notifications.
        /// </summary>
        private const string CACHE_DELETE_CHANNEL = "cache/delete";

        /// <summary>
        /// Where the data should be stored.
        /// </summary>
        public enum Store
        {
            /// <summary>
            /// Option to store the data in local memory only.
            /// </summary>
            Memory = 0,

            /// <summary>
            /// Option to store the data in redis only.
            /// </summary>
            Redis = 1,

            /// <summary>
            /// Option to store the data in both local memory and redis.
            /// </summary>
            MemoryAndRedis = 2
        }

        /// <summary>
        /// Subscribes a method to the cache/delete channel.
        /// </summary>
        public static void SubscribeCacheDeleteChannel()
        {
            PubSub.SubscribeAsync(Cache.CACHE_DELETE_CHANNEL, Cache.Memory.Strings.Delete);
        }

        /// <summary>
        /// Deletes the value stored at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        public static void Delete(string key, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    PubSub.Publish(Cache.CACHE_DELETE_CHANNEL, key);
                    break;
                }

                case Store.Redis:
                {
                    Cache.Shared.Keys.Delete(key);
                    break;
                }

                case Store.MemoryAndRedis:
                {
                    PubSub.Publish(Cache.CACHE_DELETE_CHANNEL, key);
                    Cache.Shared.Keys.Delete(key);
                    break;
                }
            }
        }

        /// <summary>
        /// Deletes the value stored at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> (void).
        /// </returns>
        public static async Task DeleteAsync(string key, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    await PubSub.PublishAsync(Cache.CACHE_DELETE_CHANNEL, key);
                    break;
                }

                case Store.Redis:
                {
                    await Cache.Shared.Keys.DeleteAsync(key);
                    break;
                }

                case Store.MemoryAndRedis:
                {
                    await PubSub.PublishAsync(Cache.CACHE_DELETE_CHANNEL, key);
                    await Cache.Shared.Keys.DeleteAsync(key);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        public static void Set(string key, object value, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    Cache.Memory.Strings.Set(key, value);
                    break;
                }

                case Store.Redis:
                {
                    byte[] result = Compress.Compression.Compress(value);
                    Cache.Shared.Strings.Set(key, result);
                    break;
                }

                case Store.MemoryAndRedis:
                {
                    byte[] result = Cache.Memory.Strings.Set(key, value);
                    Cache.Shared.Strings.Set(key, result);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> (void).
        /// </returns>
        public static async Task SetAsync(string key, object value, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    await Cache.Memory.Strings.SetAsync(key, value);
                    break;
                }

                case Store.Redis:
                {
                    byte[] result = await Compress.Compression.CompressAsync(value);
                    await Cache.Shared.Strings.SetAsync(key, result);
                    break;
                }

                case Store.MemoryAndRedis:
                {
                    byte[] result = await Cache.Memory.Strings.SetAsync(key, value);
                    await Cache.Shared.Strings.SetAsync(key, result);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="absoluteExpiration">
        /// An expiration lifetime.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        public static void Set(string key, object value, TimeSpan absoluteExpiration, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    Cache.Memory.Strings.Set(key, value, absoluteExpiration);
                    break;
                }

                case Store.Redis:
                {
                    byte[] result = Compress.Compression.Compress(value);
                    Cache.Shared.Strings.Set(key, result, absoluteExpiration);
                    break;
                }

                case Store.MemoryAndRedis:
                {
                    byte[] result = Cache.Memory.Strings.Set(key, value, absoluteExpiration);
                    Cache.Shared.Strings.Set(key, result, absoluteExpiration);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="absoluteExpiration">
        /// An expiration lifetime.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> (void).
        /// </returns>
        public static async Task SetAsync(string key, object value, TimeSpan absoluteExpiration, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    await Cache.Memory.Strings.SetAsync(key, value, absoluteExpiration);
                    break;
                }

                case Store.Redis:
                {
                    byte[] result = await Compress.Compression.CompressAsync(value);
                    await Cache.Shared.Strings.SetAsync(key, result, absoluteExpiration);
                    break;
                }

                case Store.MemoryAndRedis:
                {
                    byte[] result = await Cache.Memory.Strings.SetAsync(key, value, absoluteExpiration);
                    await Cache.Shared.Strings.SetAsync(key, result, absoluteExpiration);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="slidingExpiration">
        /// An expiration lifetime that extends whenever the key is accessed or updated.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        public static void SetSliding(string key, object value, TimeSpan slidingExpiration, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    Cache.Memory.Strings.SetSliding(key, value, slidingExpiration);
                    break;
                }

                case Store.Redis:
                {
                    byte[] result = Compress.Compression.Compress(value);
                    Cache.Shared.Strings.Set(key, result, slidingExpiration);
                    break;
                }

                case Store.MemoryAndRedis:
                {
                    byte[] result = Cache.Memory.Strings.SetSliding(key, value, slidingExpiration);
                    Cache.Shared.Strings.Set(key, result, slidingExpiration);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="slidingExpiration">
        /// An expiration lifetime that extends whenever the key is accessed or updated.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> (void).
        /// </returns>
        public static async Task SetSlidingAsync(string key, object value, TimeSpan slidingExpiration, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    await Cache.Memory.Strings.SetSlidingAsync(key, value, slidingExpiration);
                    break;
                }

                case Store.Redis:
                {
                    byte[] result = await Compress.Compression.CompressAsync(value);
                    await Cache.Shared.Strings.SetAsync(key, result, slidingExpiration);
                    break;
                }

                case Store.MemoryAndRedis:
                {
                    byte[] result = Cache.Memory.Strings.SetSliding(key, value, slidingExpiration);
                    await Cache.Shared.Strings.SetAsync(key, result, slidingExpiration);
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        public static MemoryValue<TObject> Get<TObject>(string key, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return Cache.Memory.Strings.Get<TObject>(key);
                }

                case Store.Redis:
                {
                    RedisValue rv = Cache.Shared.Strings.Get(key);
                    if (rv.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject decompress = Compress.Compression.Decompress<TObject>(rv);
                        return new MemoryValue<TObject>(decompress, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> result = Cache.Memory.Strings.Get<TObject>(key);
                    if (result.Exists)
                    {
                        // found it? return the value
                        return result;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue rv = Cache.Shared.Strings.Get(key);
                        if (rv.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? store it in memory for the next time, then return the value
                            TObject decompress = Compress.Compression.Decompress<TObject>(rv);
                            Cache.Memory.Strings.Set(key, decompress);
                            return new MemoryValue<TObject>(decompress, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        public static async Task<MemoryValue<TObject>> GetAsync<TObject>(string key, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return await Cache.Memory.Strings.GetAsync<TObject>(key);
                }

                case Store.Redis:
                {
                    RedisValue rv = await Cache.Shared.Strings.GetAsync(key);
                    if (rv.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject value = await Compress.Compression.DecompressAsync<TObject>(rv);
                        return new MemoryValue<TObject>(value, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> result = await Cache.Memory.Strings.GetAsync<TObject>(key);
                    if (result.Exists)
                    {
                        // found it? return the value
                        return result;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue rv = await Cache.Shared.Strings.GetAsync(key);
                        if (rv.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? store it in memory for the next time, then return the value
                            TObject value = await Compress.Compression.DecompressAsync<TObject>(rv);

                            // set the value in memory, but don't wait for it to complete before returning our result
#pragma warning disable 4014
                            Cache.Memory.Strings.SetAsync(key, value);
#pragma warning restore 4014

                            return new MemoryValue<TObject>(value, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="expire">
        /// An expiration lifetime.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        public static MemoryValue<TObject> Get<TObject>(string key, TimeSpan expire, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return Cache.Memory.Strings.Get<TObject>(key, expire);
                }

                case Store.Redis:
                {
                    RedisValue rv = Cache.Shared.Strings.Get(key, expire);
                    if (rv.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject decompress = Compress.Compression.Decompress<TObject>(rv);
                        return new MemoryValue<TObject>(decompress, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> result = Cache.Memory.Strings.Get<TObject>(key, expire);
                    if (result.Exists)
                    {
                        // found it? return the value
                        Cache.Shared.Keys.Expire(key, expire);
                        return result;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue rv = Cache.Shared.Strings.Get(key, expire);
                        if (rv.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? store it in memory for the next time, then return the value
                            TObject decompress = Compress.Compression.Decompress<TObject>(rv);
                            Cache.Memory.Strings.Set(key, decompress);
                            return new MemoryValue<TObject>(decompress, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="expire">
        /// An expiration lifetime.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        public static async Task<MemoryValue<TObject>> GetAsync<TObject>(string key, TimeSpan expire, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return await Cache.Memory.Strings.GetAsync<TObject>(key, expire);
                }

                case Store.Redis:
                {
                    RedisValue rv = await Cache.Shared.Strings.GetAsync(key, expire);
                    if (rv.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject value = await Compress.Compression.DecompressAsync<TObject>(rv);
                        return new MemoryValue<TObject>(value, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> result = await Cache.Memory.Strings.GetAsync<TObject>(key, expire);
                    if (result.Exists)
                    {
                        // found it? return the value
#pragma warning disable 4014
                        Cache.Shared.Keys.ExpireAsync(key, expire);
#pragma warning restore 4014
                        return result;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue rv = await Cache.Shared.Strings.GetAsync(key, expire);
                        if (rv.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? store it in memory for the next time, then return the value
                            TObject value = await Compress.Compression.DecompressAsync<TObject>(rv);

                            // set the value in memory, but don't wait for it to complete before returning our result
#pragma warning disable 4014
                            Cache.Memory.Strings.SetAsync(key, value);
#pragma warning restore 4014

                            return new MemoryValue<TObject>(value, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="slidingExpiration">
        /// An expiration lifetime that extends whenever the key is accessed or updated.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        public static MemoryValue<TObject> GetSliding<TObject>(
            string key,
            TimeSpan slidingExpiration,
            Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    // TODO remove this when it's no longer necessary
                    Cache.Memory.Strings.Expire(key, slidingExpiration);
                    return Cache.Memory.Strings.Get<TObject>(key);
                }

                case Store.Redis:
                {
                    RedisValue rv = Cache.Shared.Strings.Get(key, slidingExpiration);
                    if (rv.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject decompress = Compress.Compression.Decompress<TObject>(rv);
                        return new MemoryValue<TObject>(decompress, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> result = Cache.Memory.Strings.Get<TObject>(key);
                    if (result.Exists)
                    {
                        // found it? return the value
                        return result;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue rv = Cache.Shared.Strings.Get(key, slidingExpiration);
                        if (rv.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? store it in memory for the next time, then return the value
                            TObject decompress = Compress.Compression.Decompress<TObject>(rv);
                            Cache.Memory.Strings.Set(key, decompress);
                            return new MemoryValue<TObject>(decompress, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="slidingExpiration">
        /// An expiration lifetime that extends whenever the key is accessed or updated.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        public static async Task<MemoryValue<TObject>> GetSlidingAsync<TObject>(
            string key,
            TimeSpan slidingExpiration,
            Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    // TODO remove this when it's no longer necessary
                    Cache.Memory.Strings.Expire(key, slidingExpiration);
                    return await Cache.Memory.Strings.GetAsync<TObject>(key);
                }

                case Store.Redis:
                {
                    RedisValue rv = await Cache.Shared.Strings.GetAsync(key, slidingExpiration);
                    if (rv.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject decompress = await Compress.Compression.DecompressAsync<TObject>(rv);
                        return new MemoryValue<TObject>(decompress, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> result = await Cache.Memory.Strings.GetAsync<TObject>(key);
                    if (result.Exists)
                    {
                        // found it? return the value
                        return result;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue rv = await Cache.Shared.Strings.GetAsync(key, slidingExpiration);
                        if (rv.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? store it in memory for the next time, then return the value
                            TObject decompress = await Compress.Compression.DecompressAsync<TObject>(rv);
#pragma warning disable 4014
                            Cache.Memory.Strings.SetAsync(key, decompress);
#pragma warning restore 4014
                            return new MemoryValue<TObject>(decompress, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Checks whether the key exists.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <returns>
        /// True if the key exists.
        /// </returns>
        /// <exception cref="Exception">
        /// This should never occur, but the method can throw if there's an unhandled case statement.
        /// </exception>
        public static bool Exists(string key, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return Cache.Memory.Strings.Exists(key);
                }

                case Store.Redis:
                {
                    return Cache.Shared.Keys.Exists(key);
                }

                case Store.MemoryAndRedis:
                {
                    return Cache.Memory.Strings.Exists(key) || Cache.Shared.Keys.Exists(key);
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Checks whether the key exists.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <returns>
        /// True if the key exists.
        /// </returns>
        /// <exception cref="Exception">
        /// This should never occur, but the method can throw if there's an unhandled case statement.
        /// </exception>
        public static async Task<bool> ExistsAsync(string key, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return Cache.Memory.Strings.Exists(key);
                }

                case Store.Redis:
                {
                    return await Cache.Shared.Keys.ExistsAsync(key);
                }

                case Store.MemoryAndRedis:
                {
                    return Cache.Memory.Strings.Exists(key) || await Cache.Shared.Keys.ExistsAsync(key);
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Sets an expiration lifetime on the <paramref name="key"/> supplied.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="expire">
        /// The expiration lifetime.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <exception cref="Exception">
        /// This should never occur, but the method can throw if there's an unhandled case statement.
        /// </exception>
        public static void Expire(string key, TimeSpan expire, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    Cache.Memory.Strings.Expire(key, expire);
                    break;
                }

                case Store.Redis:
                {
                    Cache.Shared.Keys.Expire(key, expire);
                    break;
                }

                case Store.MemoryAndRedis:
                {
                    Cache.Memory.Strings.Expire(key, expire);
                    Cache.Shared.Keys.Expire(key, expire);
                    break;
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value stored at the <paramref name="key"/> and updates its contents with <paramref name="value"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// This should never occur, but the method can throw if there's an unhandled case statement.
        /// </exception>
        public static MemoryValue<TObject> GetSet<TObject>(string key, TObject value, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return Cache.Memory.Strings.GetSet<TObject>(key, value);
                }

                case Store.Redis:
                {
                    byte[] compress = Compress.Compression.Compress(value);
                    RedisValue redisValue = Cache.Shared.Strings.GetSet(key, compress);
                    if (redisValue.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject decompress = Compress.Compression.Decompress<TObject>(redisValue);
                        return new MemoryValue<TObject>(decompress, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> memoryValue = Cache.Memory.Strings.GetSet<TObject>(key, value);
                    byte[] compress = Compress.Compression.Compress(value);
                    if (memoryValue.Exists)
                    {
                        // found it? return the value
                        Cache.Shared.Strings.Set(key, compress);
                        return memoryValue;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue redisValue = Cache.Shared.Strings.GetSet(key, compress);
                        if (redisValue.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis?
                            TObject decompress = Compress.Compression.Decompress<TObject>(redisValue);
                            return new MemoryValue<TObject>(decompress, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value stored at the <paramref name="key"/> and updates its contents with <paramref name="value"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// This should never occur, but the method can throw if there's an unhandled case statement.
        /// </exception>
        public static async Task<MemoryValue<TObject>> GetSetAsync<TObject>(string key, TObject value, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return await Cache.Memory.Strings.GetSetAsync<TObject>(key, value);
                }

                case Store.Redis:
                {
                    byte[] bytes = await Compress.Compression.CompressAsync(value);
                    RedisValue redisValue = await Cache.Shared.Strings.GetSetAsync(key, bytes);
                    if (redisValue.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject decompress = await Compress.Compression.DecompressAsync<TObject>(redisValue);
                        return new MemoryValue<TObject>(decompress, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> memoryValue = await Cache.Memory.Strings.GetSetAsync<TObject>(key, value);
                    byte[] compress = await Compress.Compression.CompressAsync(value);
                    if (memoryValue.Exists)
                    {
                        // found it? return the value
#pragma warning disable 4014
                        Cache.Shared.Strings.SetAsync(key, compress);
#pragma warning restore 4014
                        return memoryValue;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue redisValue = await Cache.Shared.Strings.GetSetAsync(key, compress);
                        if (redisValue.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis?
                            TObject decompress = await Compress.Compression.DecompressAsync<TObject>(redisValue);
                            return new MemoryValue<TObject>(decompress, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value stored at the <paramref name="key"/> and updates its contents with <paramref name="value"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="absoluteExpiration">
        /// An expiration lifetime.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// This should never occur, but the method can throw if there's an unhandled case statement.
        /// </exception>
        public static MemoryValue<TObject> GetSet<TObject>(
            string key, TObject value, TimeSpan absoluteExpiration, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return Cache.Memory.Strings.GetSet<TObject>(key, value, absoluteExpiration);
                }

                case Store.Redis:
                {
                    byte[] compress = Compress.Compression.Compress(value);
                    RedisValue redisValue = Cache.Shared.Strings.GetSet(key, compress, absoluteExpiration);
                    if (redisValue.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject decompress = Compress.Compression.Decompress<TObject>(redisValue);
                        return new MemoryValue<TObject>(decompress, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> memoryValue = Cache.Memory.Strings.GetSet<TObject>(key, value, absoluteExpiration);
                    byte[] compress = Compress.Compression.Compress(value);
                    if (memoryValue.Exists)
                    {
                        // found it? return the value
                        Cache.Shared.Strings.Set(key, compress, absoluteExpiration);
                        return memoryValue;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue redisValue = Cache.Shared.Strings.GetSet(key, compress, absoluteExpiration);
                        if (redisValue.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? just return (don't set, that's done in the GetSet above)
                            TObject decompress = Compress.Compression.Decompress<TObject>(redisValue);
                            return new MemoryValue<TObject>(decompress, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value stored at the <paramref name="key"/> and updates its contents with <paramref name="value"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="absoluteExpiration">
        /// An expiration lifetime.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// This should never occur, but the method can throw if there's an unhandled case statement.
        /// </exception>
        public static async Task<MemoryValue<TObject>> GetSetAsync<TObject>(
            string key, TObject value, TimeSpan absoluteExpiration, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return await Cache.Memory.Strings.GetSetAsync<TObject>(key, value, absoluteExpiration);
                }

                case Store.Redis:
                {
                    byte[] compress = await Compress.Compression.CompressAsync(value);
                    RedisValue redisValue = await Cache.Shared.Strings.GetSetAsync(key, compress, absoluteExpiration);
                    if (redisValue.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject decompress = await Compress.Compression.DecompressAsync<TObject>(redisValue);
                        return new MemoryValue<TObject>(decompress, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> memoryValue = await Cache.Memory.Strings.GetSetAsync<TObject>(
                        key, value, absoluteExpiration);
                    byte[] compress = await Compress.Compression.CompressAsync(value);
                    if (memoryValue.Exists)
                    {
                        // found it? return the value
#pragma warning disable 4014
                        Cache.Shared.Strings.SetAsync(key, compress, absoluteExpiration);
#pragma warning restore 4014
                        return memoryValue;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue redisValue = await Cache.Shared.Strings.GetSetAsync(key, compress, absoluteExpiration);
                        if (redisValue.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? just return (don't set, that's done in the GetSet above)
                            TObject decompress = await Compress.Compression.DecompressAsync<TObject>(redisValue);
                            return new MemoryValue<TObject>(decompress, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value stored at the <paramref name="key"/> and updates its contents with <paramref name="value"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="slidingExpiration">
        /// An expiration lifetime that extends whenever the key is accessed or updated.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// This should never occur, but the method can throw if there's an unhandled case statement.
        /// </exception>
        public static MemoryValue<TObject> GetSetSliding<TObject>(
            string key, TObject value, TimeSpan slidingExpiration, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return Cache.Memory.Strings.GetSetSliding<TObject>(key, value, slidingExpiration);
                }

                case Store.Redis:
                {
                    byte[] result = Compress.Compression.Compress(value);
                    RedisValue rv = Cache.Shared.Strings.GetSet(key, result, slidingExpiration);
                    if (rv.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject decompress = Compress.Compression.Decompress<TObject>(rv);
                        return new MemoryValue<TObject>(decompress, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> sliding = Cache.Memory.Strings.GetSetSliding<TObject>(
                        key, value, slidingExpiration);
                    byte[] compress = Compress.Compression.Compress(value);
                    if (sliding.Exists)
                    {
                        // found it? return the value
                        Cache.Shared.Strings.Set(key, compress);
                        return sliding;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue redisValue = Cache.Shared.Strings.GetSet(key, compress, slidingExpiration);
                        if (redisValue.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? just return (don't set, that's done in the GetSet above)
                            TObject decompress = Compress.Compression.Decompress<TObject>(redisValue);
                            return new MemoryValue<TObject>(decompress, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }

        /// <summary>
        /// Gets the value stored at the <paramref name="key"/> and updates its contents with <paramref name="value"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="slidingExpiration">
        /// An expiration lifetime that extends whenever the key is accessed or updated.
        /// </param>
        /// <param name="store">
        /// The method of storage.
        /// </param>
        /// <typeparam name="TObject">
        /// The type of the value stored at <paramref name="key"/>.
        /// </typeparam>
        /// <returns>
        /// The <see><cref>MemoryValue</cref></see> stored at <paramref name="key"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// This should never occur, but the method can throw if there's an unhandled case statement.
        /// </exception>
        public static async Task<MemoryValue<TObject>> GetSetSlidingAsync<TObject>(
            string key, TObject value, TimeSpan slidingExpiration, Store store = Store.Redis)
        {
            switch (store)
            {
                case Store.Memory:
                {
                    return await Cache.Memory.Strings.GetSetSlidingAsync<TObject>(key, value, slidingExpiration);
                }

                case Store.Redis:
                {
                    byte[] result = await Compress.Compression.CompressAsync(value);
                    RedisValue rv = await Cache.Shared.Strings.GetSetAsync(key, result, slidingExpiration);
                    if (rv.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject decompress = await Compress.Compression.DecompressAsync<TObject>(rv);
                        return new MemoryValue<TObject>(decompress, true);
                    }
                }

                case Store.MemoryAndRedis:
                {
                    // try pulling from memory
                    MemoryValue<TObject> sliding = await Cache.Memory.Strings.GetSetSlidingAsync<TObject>(
                        key, value, slidingExpiration);
                    byte[] compress = await Compress.Compression.CompressAsync(value);
                    if (sliding.Exists)
                    {
                        // found it? return the value
                        Cache.Shared.Strings.Set(key, compress);
                        return sliding;
                    }
                    else
                    {
                        // not found? try getting from redis
                        RedisValue redisValue = await Cache.Shared.Strings.GetSetAsync(key, compress, slidingExpiration);
                        if (redisValue.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? just return (don't set, that's done in the GetSet above)
                            TObject decompress = await Compress.Compression.DecompressAsync<TObject>(redisValue);
                            return new MemoryValue<TObject>(decompress, true);
                        }
                    }
                }

                default:
                {
                    throw new Exception("Invalid case statement reached.");
                }
            }
        }
    }
}
