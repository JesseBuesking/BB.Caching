using System;
using System.Threading.Tasks;
using BB.Caching.Caching;
using BB.Caching.Compression;
using StackExchange.Redis;

// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    public static partial class Cache
    {
        /// <summary>
        /// Where the data should be stored.
        /// </summary>
        public enum Store
        {
            Memory = 0,
            Redis = 1,
            MemoryAndRedis = 2
        }

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
                    Cache.Shared.Strings.SetAsync(key, result);
                    break;
                }
                case Store.MemoryAndRedis:
                {
                    byte[] result = Cache.Memory.Strings.Set(key, value);
                    Cache.Shared.Strings.SetAsync(key, result);
                    break;
                }
            }
        }

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
                    Cache.Shared.Strings.SetAsync(key, result, absoluteExpiration);
                    break;
                }
                case Store.MemoryAndRedis:
                {
                    byte[] result = Cache.Memory.Strings.Set(key, value, absoluteExpiration);
                    Cache.Shared.Strings.SetAsync(key, result, absoluteExpiration);
                    break;
                }
            }
        }

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
                    // TODO sliding logic handled by getsliding methods
                    Cache.Shared.Strings.SetAsync(key, result, slidingExpiration);
                    break;
                }
                case Store.MemoryAndRedis:
                {
                    byte[] result = Cache.Memory.Strings.SetSliding(key, value, slidingExpiration);
                    // TODO sliding logic handled by getsliding methods
                    Cache.Shared.Strings.SetAsync(key, result, slidingExpiration);
                    break;
                }
            }
        }

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
                    // TODO sliding logic handled by getsliding methods
                    await Cache.Shared.Strings.SetAsync(key, result, slidingExpiration);
                    break;
                }
                case Store.MemoryAndRedis:
                {
                    byte[] result = Cache.Memory.Strings.SetSliding(key, value, slidingExpiration);
                    // TODO sliding logic handled by getsliding methods
                    await Cache.Shared.Strings.SetAsync(key, result, slidingExpiration);
                    break;
                }
            }
        }

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
                    RedisValue rv = Cache.Shared.Strings.GetAsync(key).Result;
                    if (rv.IsNull)
                    {
                        return MemoryValue<TObject>.Null;
                    }
                    else
                    {
                        TObject value = Compress.Compression.Decompress<TObject>(rv);
                        return new MemoryValue<TObject>(value, true);
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
                        // TODO use sync method
                        RedisValue rv = Cache.Shared.Strings.GetAsync(key).Result;
                        if (rv.IsNull)
                        {
                            // not there? return null object
                            return MemoryValue<TObject>.Null;
                        }
                        else
                        {
                            // found it in redis? store it in memory for the next time, then return the value
                            TObject value = Compress.Compression.Decompress<TObject>(rv);
                            Cache.Memory.Strings.Set(key, value);
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

        public static bool Exists(string key, Store store = Store.Redis)
        {
            throw new NotImplementedException();
        }

        public static Task<bool> ExistsAsync(string key, Store store = Store.Redis)
        {
            throw new NotImplementedException();
        }

        public static MemoryValue<TObject> GetSet<TObject>(string key, object value, Store store = Store.Redis)
        {
            throw new NotImplementedException();
        }

        public static Task<MemoryValue<TObject>> GetSetAsync<TObject>(string key, object value, Store store = Store.Redis)
        {
            throw new NotImplementedException();
        }

        public static MemoryValue<TObject> GetSet<TObject>(string key, object value, TimeSpan absoluteExpiration, Store store = Store.Redis)
        {
            throw new NotImplementedException();
        }

        public static Task<MemoryValue<TObject>> GetSetAsync<TObject>(string key, object value, TimeSpan absoluteExpiration, Store store = Store.Redis)
        {
            throw new NotImplementedException();
        }

        public static MemoryValue<TObject> GetSetSliding<TObject>(string key, object value, TimeSpan slidingExpiration, Store store = Store.Redis)
        {
            throw new NotImplementedException();
        }

        public static Task<MemoryValue<TObject>> GetSetSlidingAsync<TObject>(string key, object value, TimeSpan slidingExpiration, Store store = Store.Redis)
        {
            throw new NotImplementedException();
        }
    }
}
