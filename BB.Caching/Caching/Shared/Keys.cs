// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BB.Caching.Redis;

    using StackExchange.Redis;

    /// <summary>
    /// Contains the core methods for caching data in memory, redis, or both.
    /// </summary>
    public static partial class Cache
    {
        /// <summary>
        /// The shared / L2 / redis cache.
        /// </summary>
        public static partial class Shared
        {
            /// <summary>
            /// Generic commands that apply to all/most data structures
            /// </summary>
            /// <remarks>
            /// http://redis.io/commands#generic
            /// </remarks>
            public static class Keys
            {
                /// <summary>
                /// Removes the specified key. A key is ignored if it does not exist.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// True if the key was removed.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/del
                /// </remarks>
                public static bool Delete(RedisKey key)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    bool result = false;
                    foreach (var connection in connections)
                    {
                        result = result || connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .KeyDelete(key);
                    }

                    return result;
                }

                /// <summary>
                /// Removes the specified key. A key is ignored if it does not exist.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// True if the key was removed.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/del
                /// </remarks>
                public static Task<bool> DeleteAsync(RedisKey key)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<bool> result = null;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .KeyDeleteAsync(key);
                    }

                    return result;
                }

                /// <summary>
                /// Removes the specified keys. A key is ignored if it does not exist.
                /// </summary>
                /// <param name="keys">
                /// The keys.
                /// </param>
                /// <returns>
                /// The number of keys that were removed.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/del
                /// </remarks>
                public static long Delete(RedisKey[] keys)
                {
                    var dictionary = SharedCache.Instance.GetWriteConnections(keys);
                    long removed = 0;
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        foreach (var connection in dictionary.ElementAt(i).Key)
                        {
                            removed += connection
                                .GetDatabase(SharedCache.Instance.Db)
                                .KeyDelete(dictionary.ElementAt(i).Value);
                        }
                    }

                    return removed;
                }

                /// <summary>
                /// Removes the specified keys. A key is ignored if it does not exist.
                /// </summary>
                /// <param name="keys">
                /// The keys.
                /// </param>
                /// <returns>
                /// The number of keys that were removed.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/del
                /// </remarks>
                public static async Task<long> DeleteAsync(RedisKey[] keys)
                {
                    var dictionary = SharedCache.Instance.GetWriteConnections(keys);
                    var tasks = new Task<long>[dictionary.Count];
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        foreach (var connection in dictionary.ElementAt(i).Key)
                        {
                            tasks[i] = connection
                                .GetDatabase(SharedCache.Instance.Db)
                                .KeyDeleteAsync(dictionary.ElementAt(i).Value);
                        }
                    }

                    var results = await Task.WhenAll(tasks);
                    return results.Sum();
                }

                /// <summary>
                /// Returns if key exists.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// 1 if the key exists. 0 if the key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/exists
                /// </remarks>
                public static bool Exists(string key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .KeyExists(key);
                }

                /// <summary>
                /// Returns if key exists.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// 1 if the key exists. 0 if the key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/exists
                /// </remarks>
                public static Task<bool> ExistsAsync(string key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .KeyExistsAsync(key);
                }

                /// <summary>
                /// Set a timeout on key. After the timeout has expired, the key will automatically be deleted. A key with an
                /// associated timeout is said to be volatile in Redis terminology.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="expiry">
                /// The expiry.
                /// </param>
                /// <remarks>
                /// If key is updated before the timeout has expired, then the timeout is removed as if the PERSIST
                /// command was invoked on key.
                /// For Redis versions &lt; 2.1.3, existing timeouts cannot be overwritten. So, if key already has an associated
                /// timeout, it will do nothing and return 0. Since Redis 2.1.3, you can update the timeout of a key. It is also
                /// possible to remove the timeout using the PERSIST command. See the page on key expiry for more information.
                /// </remarks>
                /// <returns>
                /// 1 if the timeout was set. 0 if key does not exist or the timeout could not be set.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/expire
                /// </remarks>
                public static bool Expire(string key, TimeSpan expiry)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    bool result = false;
                    foreach (var connection in connections)
                    {
                        result = result || connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .KeyExpire(key, expiry);
                    }

                    return result;
                }

                /// <summary>
                /// Set a timeout on key. After the timeout has expired, the key will automatically be deleted. A key with an
                /// associated timeout is said to be volatile in Redis terminology.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="expiry">
                /// The expiry.
                /// </param>
                /// <remarks>
                /// If key is updated before the timeout has expired, then the timeout is removed as if the PERSIST
                /// command was invoked on key.
                /// For Redis versions &lt; 2.1.3, existing timeouts cannot be overwritten. So, if key already has an associated
                /// timeout, it will do nothing and return 0. Since Redis 2.1.3, you can update the timeout of a key. It is also
                /// possible to remove the timeout using the PERSIST command. See the page on key expiry for more information.
                /// </remarks>
                /// <returns>
                /// 1 if the timeout was set. 0 if key does not exist or the timeout could not be set.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/expire
                /// </remarks>
                public static Task<bool> ExpireAsync(string key, TimeSpan expiry)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<bool> result = null;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .KeyExpireAsync(key, expiry);
                    }

                    return result;
                }

                /// <summary>
                /// Remove the existing timeout on key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// 1 if the timeout was removed. 0 if key does not exist or does not have an associated timeout.
                /// </returns>
                /// <remarks>
                /// Available with 2.1.2 and above only
                /// </remarks>
                /// <remarks>
                /// http://redis.io/commands/persist
                /// </remarks>
                public static bool Persist(string key)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    bool result = false;
                    foreach (var connection in connections)
                    {
                        result = result || connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .KeyPersist(key);
                    }

                    return result;
                }

                /// <summary>
                /// Remove the existing timeout on key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// 1 if the timeout was removed. 0 if key does not exist or does not have an associated timeout.
                /// </returns>
                /// <remarks>
                /// Available with 2.1.2 and above only
                /// </remarks>
                /// <remarks>
                /// http://redis.io/commands/persist
                /// </remarks>
                public static Task<bool> PersistAsync(string key)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<bool> result = null;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .KeyPersistAsync(key);
                    }

                    return result;
                }

                /// <summary>
                /// Return a random key from the currently selected database.
                /// </summary>
                /// <returns>
                /// the random key, or nil when the database is empty.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/randomkey
                /// </remarks>
                public static RedisKey Random()
                {
                    return SharedCache.Instance.GetRandomReadConnection()
                        .GetDatabase(SharedCache.Instance.Db)
                        .KeyRandom();
                }

                /// <summary>
                /// Return a random key from the currently selected database.
                /// </summary>
                /// <returns>
                /// the random key, or nil when the database is empty.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/randomkey
                /// </remarks>
                public static Task<RedisKey> RandomAsync()
                {
                    return SharedCache.Instance.GetRandomReadConnection()
                        .GetDatabase(SharedCache.Instance.Db)
                        .KeyRandomAsync();
                }

                /// <summary>
                /// Returns the remaining time to live of a key that has a timeout. This introspection capability allows a
                /// Redis client to check how many seconds a given key will continue to be part of the dataset.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// TTL in seconds or -1 when key does not exist or does not have a timeout.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/ttl
                /// </remarks>
                public static TimeSpan? TimeToLive(string key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .KeyTimeToLive(key);
                }

                /// <summary>
                /// Returns the remaining time to live of a key that has a timeout. This introspection capability allows a
                /// Redis client to check how many seconds a given key will continue to be part of the dataset.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// TTL in seconds or -1 when key does not exist or does not have a timeout.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/ttl
                /// </remarks>
                public static Task<TimeSpan?> TimeToLiveAsync(string key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .KeyTimeToLiveAsync(key);
                }

                /// <summary>
                /// Returns the string representation of the type of the value stored at key. The different types that can be
                /// returned are: string, list, set, zset and hash.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// type of key, or none when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/type
                /// </remarks>
                public static RedisType Type(string key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .KeyType(key);
                }

                /// <summary>
                /// Returns the string representation of the type of the value stored at key. The different types that can be
                /// returned are: string, list, set, zset and hash.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// type of key, or none when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/type
                /// </remarks>
                public static Task<RedisType> TypeAsync(string key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .KeyTypeAsync(key);
                }

                /// <summary>
                /// Return the number of keys in the currently selected database.
                /// </summary>
                /// <remarks>
                /// http://redis.io/commands/dbsize
                /// </remarks>
                /// <returns>
                /// The <see cref="long"/>.
                /// </returns>
                public static long GetLength()
                {
                    var connections = SharedCache.Instance.GetAllReadConnections();
                    var results = new string[connections.Length];
                    for (int i = 0; i < connections.Length; i++)
                    {
                        results[i] = connections[i].GetStatus();
                    }

                    return results.Select(long.Parse).Sum();
                }

                /// <summary>
                /// Returns the raw DEBUG OBJECT output for a key; this command is not fully documented and should be avoided
                /// unless you have good reason, and then avoided anyway.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/debug-object
                /// </remarks>
                /// <returns>
                /// The <see cref="RedisValue"/> containing raw DEBUG OBJECT output.
                /// </returns>
                public static RedisValue DebugObject(string key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .DebugObject(key);
                }

                /// <summary>
                /// Returns the raw DEBUG OBJECT output for a key; this command is not fully documented and should be avoided
                /// unless you have good reason, and then avoided anyway.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/debug-object
                /// </remarks>
                /// <returns>
                /// The <see cref="RedisValue"/> containing raw DEBUG OBJECT output.
                /// </returns>
                public static Task<RedisValue> DebugObjectAsync(string key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .DebugObjectAsync(key);
                }

                /// <summary>
                /// Invalidates the object stored at the key's location in Cache.Memory.Strings.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// The number of clients that received the message.
                /// </returns>
                public static long Invalidate(string key)
                {
                    Cache.Memory.Strings.Delete(key);
                    SharedCache.Instance.AlreadyInvalidated.Add(key);
                    return PubSub.Publish(SharedCache.CACHE_INVALIDATION_CHANNEL, key);
                }

                /// <summary>
                /// Invalidates the object stored at the key's location in Cache.Memory.Strings.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// The number of clients that received the message.
                /// </returns>
                public static Task<long> InvalidateAsync(string key)
                {
                    Cache.Memory.Strings.Delete(key);
                    SharedCache.Instance.AlreadyInvalidated.Add(key);
                    return PubSub.PublishAsync(SharedCache.CACHE_INVALIDATION_CHANNEL, key);
                }

                /// <summary>
                /// Invalidates the objects stored at the keys located in Cache.Memory.Strings.
                /// </summary>
                /// <param name="keys">
                /// The keys.
                /// </param>
                /// <returns>
                /// The number of clients that received the message.
                /// </returns>
                public static long Invalidate(string[] keys)
                {
                    foreach (string key in keys)
                    {
                        Cache.Memory.Strings.Delete(key);
                        SharedCache.Instance.AlreadyInvalidated.Add(key);
                    }

                    string multipleKeys = string.Join(PubSub.MULTIPLE_MESSAGE_SEPARATOR, keys);
                    return PubSub.Publish(SharedCache.CACHE_MULTIPLE_INVALIDATION_CHANNEL, multipleKeys);
                }

                /// <summary>
                /// Invalidates the objects stored at the keys located in Cache.Memory.Strings.
                /// </summary>
                /// <param name="keys">
                /// The keys.
                /// </param>
                /// <returns>
                /// The number of clients that received the message.
                /// </returns>
                public static Task<long> InvalidateAsync(string[] keys)
                {
                    foreach (string key in keys)
                    {
                        Cache.Memory.Strings.Delete(key);
                        SharedCache.Instance.AlreadyInvalidated.Add(key);
                    }

                    string multipleKeys = string.Join(PubSub.MULTIPLE_MESSAGE_SEPARATOR, keys);
                    return PubSub.PublishAsync(SharedCache.CACHE_MULTIPLE_INVALIDATION_CHANNEL, multipleKeys);
                }
            }
        }
    }
}