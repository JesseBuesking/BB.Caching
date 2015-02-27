using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    public static partial class Cache
    {
        public static partial class Shared
        {
            /// <summary>
            /// Commands that apply to key/value pairs, where the value can be a string, a BLOB, or interpreted as a number
            /// </summary>
            /// <remarks>http://redis.io/commands#string</remarks>
            public static class Strings
            {
                /// <summary>
                /// If key already exists and is a string, this command appends the value at the end of the string. If key does
                /// not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
                /// </summary>
                /// 
                /// <returns>
                /// the length of the string after the append operation.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/append
                /// </remarks>
                public static long Append(RedisKey key, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    long result = 0;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringAppend(key, value);
                    }
                    return result;
                }

                /// <summary>
                /// If key already exists and is a string, this command appends the value at the end of the string. If key does
                /// not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
                /// </summary>
                /// 
                /// <returns>
                /// the length of the string after the append operation.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/append
                /// </remarks>
                public static Task<long> AppendAsync(RedisKey key, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<long> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringAppendAsync(key, value);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// If key already exists and is a string, this command appends the value at the end of the string. If key does
                /// not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
                /// </summary>
                /// 
                /// <returns>
                /// the length of the string after the append operation.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/append
                /// </remarks>
                public static long Append(RedisKey key, byte[] value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    long result = 0;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringAppend(key, value);
                    }
                    return result;
                }

                /// <summary>
                /// If key already exists and is a string, this command appends the value at the end of the string. If key does
                /// not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
                /// </summary>
                /// 
                /// <returns>
                /// the length of the string after the append operation.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/append
                /// </remarks>
                public static Task<long> AppendAsync(RedisKey key, byte[] value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<long> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringAppendAsync(key, value);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Decrements the number stored at key by decrement. If the key does not exist, it is set to 0 before
                /// performing the operation. An error is returned if the key contains a value of the wrong type or contains a
                /// string that is not representable as integer. This operation is limited to 64 bit signed integers.
                /// </summary>
                /// 
                /// <returns>
                /// the value of key after the increment
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/decrby
                /// </remarks>
                /// 
                /// <remarks>
                /// http://redis.io/commands/decr
                /// </remarks>
                public static long Decrement(RedisKey key, long value = 1)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    long result = 0;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringDecrement(key, value);
                    }
                    return result;
                }

                /// <summary>
                /// Decrements the number stored at key by decrement. If the key does not exist, it is set to 0 before
                /// performing the operation. An error is returned if the key contains a value of the wrong type or contains a
                /// string that is not representable as integer. This operation is limited to 64 bit signed integers.
                /// </summary>
                /// 
                /// <returns>
                /// the value of key after the increment
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/decrby
                /// </remarks>
                /// 
                /// <remarks>
                /// http://redis.io/commands/decr
                /// </remarks>
                public static Task<long> DecrementAsync(RedisKey key, long value = 1)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<long> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringDecrementAsync(key, value);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Increments the number stored at key by increment. If the key does not exist, it is set to 0 before
                /// performing the operation. An error is returned if the key contains a value of the wrong type or contains a
                /// string that is not representable as integer. This operation is limited to 64 bit signed integers.
                /// </summary>
                /// 
                /// <returns>
                /// the value of key after the increment
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/incrby
                /// </remarks>
                /// 
                /// <remarks>
                /// http://redis.io/commands/incr
                /// </remarks>
                public static long Increment(RedisKey key, long value = 1)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    long result = 0;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringIncrement(key, value);
                    }
                    return result;
                }

                /// <summary>
                /// Increments the number stored at key by increment. If the key does not exist, it is set to 0 before
                /// performing the operation. An error is returned if the key contains a value of the wrong type or contains a
                /// string that is not representable as integer. This operation is limited to 64 bit signed integers.
                /// </summary>
                /// 
                /// <returns>
                /// the value of key after the increment
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/incrby
                /// </remarks>
                /// 
                /// <remarks>
                /// http://redis.io/commands/incr
                /// </remarks>
                public static Task<long> IncrementAsync(RedisKey key, long value = 1)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<long> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringIncrementAsync(key, value);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Get the value of key. If the key does not exist the special value nil is returned. An error is returned if
                /// the value stored at key is not a string, because GET only handles string values.
                /// </summary>
                /// 
                /// <returns>
                /// the value of key, or nil when key does not exist.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/get
                /// </remarks>
                public static RedisValue Get(RedisKey key)
                {
                    var result = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringGet(key);

                    return result;
                }

                /// <summary>
                /// Get the value of key. If the key does not exist the special value nil is returned. An error is returned if
                /// the value stored at key is not a string, because GET only handles string values.
                /// </summary>
                /// 
                /// <returns>
                /// the value of key, or nil when key does not exist.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/get
                /// </remarks>
                public static Task<RedisValue> GetAsync(RedisKey key)
                {
                    var result = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringGetAsync(key);

                    return result;
                }

                /// <summary>
                /// Returns the substring of the string value stored at key, determined by the offsets start and end (both are
                /// inclusive).
                /// </summary>
                /// 
                /// <remarks>
                /// Negative offsets can be used in order to provide an offset starting from the end of the string. So -1 means
                /// the last character, -2 the penultimate and so forth. The function handles out of range requests by limiting
                /// the resulting range to the actual length of the string.
                /// </remarks>
                /// 
                /// <returns>
                /// the value of key, or nil when key does not exist.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/getrange
                /// </remarks>
                public static RedisValue Get(RedisKey key, int start, int end)
                {
                    var result = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringGetRange(key, start, end);

                    return result;
                }

                /// <summary>
                /// Returns the substring of the string value stored at key, determined by the offsets start and end (both are
                /// inclusive).
                /// </summary>
                /// 
                /// <remarks>
                /// Negative offsets can be used in order to provide an offset starting from the end of the string. So -1 means
                /// the last character, -2 the penultimate and so forth. The function handles out of range requests by limiting
                /// the resulting range to the actual length of the string.
                /// </remarks>
                /// 
                /// <returns>
                /// the value of key, or nil when key does not exist.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/getrange
                /// </remarks>
                public static Task<RedisValue> GetAsync(RedisKey key, int start, int end)
                {
                    var result = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringGetRangeAsync(key, start, end);

                    return result;
                }

                /// <summary>
                /// Returns the values of all specified keys. For every key that does not hold a string value or does
                /// not exist, the special value nil is returned. Because of this, the operation never fails.
                /// </summary>
                /// 
                /// <returns>
                /// list of values at the specified keys.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/mget
                /// </remarks>
                public static RedisValue[] Get(RedisKey[] keys)
                {
                    var dictionary = SharedCache.Instance.GetWriteConnections(keys);
                    var values = new RedisValue[dictionary.Count][];
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        foreach (var connection in dictionary.ElementAt(i).Key)
                        {
                            var task = connection
                                .GetDatabase(SharedCache.Instance.Db)
                                .StringGet(dictionary.ElementAt(i).Value);

                            if (null == values[i])
                                values[i] = task;
                        }
                    }

                    var ret = new RedisValue[keys.Length];
                    int counter = 0;
                    int[] indexes = new int[values.Length];

                    // Caching results after we've awaited them.
                    RedisValue[][] results = new RedisValue[values.Length][];

                    foreach (RedisKey key in keys)
                    {
                        // Which task index contains the data for the next key?
                        int i = 0;
                        for (; i < dictionary.Count; i++)
                        {
                            if (dictionary.ElementAt(i).Value.Contains(key))
                                break;
                        }

                        // Cache the results.
                        if (null == results[i])
                            results[i] = values[i];

                        // Store a local copy of the data (so our inner tasks return the correct value).
                        RedisValue val = results[i][indexes[i]];
                        ret[counter] = val;
                        ++indexes[i];
                        ++counter;
                    }
                    return ret;
                }

                /// <summary>
                /// Returns the values of all specified keys. For every key that does not hold a string value or does
                /// not exist, the special value nil is returned. Because of this, the operation never fails.
                /// </summary>
                /// 
                /// <returns>
                /// list of values at the specified keys.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/mget
                /// </remarks>
                public static Task<RedisValue[]> GetAsync(RedisKey[] keys)
                {
                    var dictionary = SharedCache.Instance.GetWriteConnections(keys);
                    var tasks = new Task<RedisValue[]>[dictionary.Count];
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        foreach (var connection in dictionary.ElementAt(i).Key)
                        {
                            var task = connection
                                .GetDatabase(SharedCache.Instance.Db)
                                .StringGetAsync(dictionary.ElementAt(i).Value);

                            if (null == tasks[i])
                                tasks[i] = task;
                        }
                    }

                    Task<RedisValue[]> result = Task.Run(async () =>
                    {
                        var ret = new RedisValue[keys.Length];
                        int counter = 0;
                        int[] indexes = new int[tasks.Length];

                        // Caching results after we've awaited them.
                        RedisValue[][] results = new RedisValue[tasks.Length][];

                        foreach (RedisKey key in keys)
                        {
                            // Which task index contains the data for the next key?
                            int i = 0;
                            for (; i < dictionary.Count; i++)
                            {
                                if (dictionary.ElementAt(i).Value.Contains(key))
                                    break;
                            }

                            // Cache the results.
                            if (null == results[i])
                                results[i] = await tasks[i];

                            // Store a local copy of the data (so our inner tasks return the correct value).
                            RedisValue val = results[i][indexes[i]];
                            ret[counter] = val;
                            ++indexes[i];
                            ++counter;
                        }
                        return ret;
                    });
                    return result;
                }

                /// <summary>
                /// Atomically sets key to value and returns the old value stored at key. Returns an error when key exists but
                /// does not hold a string value.
                /// </summary>
                /// 
                /// <returns>
                /// the old value stored at key, or nil when key did not exist.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/getset
                /// </remarks>
                public static RedisValue GetSet(RedisKey key, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    RedisValue result = RedisValue.Null;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringGetSet(key, value);
                    }
                    return result;
                }

                /// <summary>
                /// Atomically sets key to value and returns the old value stored at key. Returns an error when key exists but
                /// does not hold a string value.
                /// </summary>
                /// 
                /// <returns>
                /// the old value stored at key, or nil when key did not exist.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/getset
                /// </remarks>
                public static Task<RedisValue> GetSetAsync(RedisKey key, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<RedisValue> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringGetSetAsync(key, value);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Atomically sets key to value and returns the old value stored at key. Returns an error when key exists but
                /// does not hold a string value.
                /// </summary>
                /// 
                /// <returns>
                /// the old value stored at key, or nil when key did not exist.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/getset
                /// </remarks>
                public static RedisValue GetSet(RedisKey key, byte[] value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    RedisValue result = RedisValue.Null;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringGetSet(key, value);
                    }
                    return result;
                }

                /// <summary>
                /// Atomically sets key to value and returns the old value stored at key. Returns an error when key exists but
                /// does not hold a string value.
                /// </summary>
                /// 
                /// <returns>
                /// the old value stored at key, or nil when key did not exist.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/getset
                /// </remarks>
                public static Task<RedisValue> GetSetAsync(RedisKey key, byte[] value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<RedisValue> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringGetSetAsync(key, value);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
                /// </summary>
                /// 
                /// <remarks>
                /// http://redis.io/commands/set
                /// </remarks>
                public static void Set(RedisKey key, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    foreach (var connection in connections)
                    {
                        connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSet(key, value);
                    }
                }

                /// <summary>
                /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
                /// </summary>
                /// 
                /// <remarks>
                /// http://redis.io/commands/set
                /// </remarks>
                public static Task SetAsync(RedisKey key, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSetAsync(key, value);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Set key to hold the string value and set key to timeout after a given number of seconds.
                /// </summary>
                /// 
                /// <remarks>
                /// http://redis.io/commands/setex
                /// </remarks>
                public static void Set(RedisKey key, RedisValue value, TimeSpan expiry)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    foreach (var connection in connections)
                    {
                        connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSet(key, value, expiry);
                    }
                }

                /// <summary>
                /// Set key to hold the string value and set key to timeout after a given number of seconds.
                /// </summary>
                /// 
                /// <remarks>
                /// http://redis.io/commands/setex
                /// </remarks>
                public static Task SetAsync(RedisKey key, RedisValue value, TimeSpan expiry)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSetAsync(key, value, expiry);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Overwrites part of the string stored at key, starting at the specified offset, for the entire length of
                /// value. If the offset is larger than the current length of the string at key, the string is padded with
                /// zero-bytes to make offset fit. Non-existing keys are considered as empty strings, so this command will make
                /// sure it holds a string large enough to be able to set value at offset.
                /// </summary>
                /// 
                /// <remarks>
                /// Note that the maximum offset that you can set is 229 -1 (536870911), as Redis Strings are limited to 512
                /// megabytes. If you need to grow beyond this size, you can use multiple keys.
                /// <para>
                /// Warning: When setting the last possible byte and the string value stored at key does not yet hold a string
                /// value, or holds a small string value, Redis needs to allocate all intermediate memory which can block the
                /// server for some time. On a 2010 MacBook Pro, setting byte number 536870911 (512MB allocation) takes ~300ms,
                /// setting byte number 134217728 (128MB allocation) takes ~80ms, setting bit number 33554432 (32MB allocation)
                /// takes ~30ms and setting bit number 8388608 (8MB allocation) takes ~8ms. Note that once this first allocation
                /// is done, subsequent calls to SETRANGE for the same key will not have the allocation overhead.
                /// </para>
                /// </remarks>
                /// 
                /// <remarks>
                /// http://redis.io/commands/setrange
                /// </remarks>
                /// 
                /// <returns>
                /// the length of the string after it was modified by the command.
                /// </returns>
                public static RedisValue Set(RedisKey key, long offset, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    RedisValue result = RedisValue.Null;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSetRange(key, offset, value);
                    }
                    return result;
                }

                /// <summary>
                /// Overwrites part of the string stored at key, starting at the specified offset, for the entire length of
                /// value. If the offset is larger than the current length of the string at key, the string is padded with
                /// zero-bytes to make offset fit. Non-existing keys are considered as empty strings, so this command will make
                /// sure it holds a string large enough to be able to set value at offset.
                /// </summary>
                /// 
                /// <remarks>
                /// Note that the maximum offset that you can set is 229 -1 (536870911), as Redis Strings are limited to 512
                /// megabytes. If you need to grow beyond this size, you can use multiple keys.
                /// <para>
                /// Warning: When setting the last possible byte and the string value stored at key does not yet hold a string
                /// value, or holds a small string value, Redis needs to allocate all intermediate memory which can block the
                /// server for some time. On a 2010 MacBook Pro, setting byte number 536870911 (512MB allocation) takes ~300ms,
                /// setting byte number 134217728 (128MB allocation) takes ~80ms, setting bit number 33554432 (32MB allocation)
                /// takes ~30ms and setting bit number 8388608 (8MB allocation) takes ~8ms. Note that once this first allocation
                /// is done, subsequent calls to SETRANGE for the same key will not have the allocation overhead.
                /// </para>
                /// </remarks>
                /// 
                /// <remarks>
                /// http://redis.io/commands/setrange
                /// </remarks>
                /// 
                /// <returns>
                /// the length of the string after it was modified by the command.
                /// </returns>
                public static Task<RedisValue> SetAsync(RedisKey key, long offset, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<RedisValue> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSetRangeAsync(key, offset, value);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Sets the given keys to their respective values. MSET replaces existing values with new values, just as
                /// regular SET. See MSETNX if you don't want to overwrite existing values.
                /// </summary>
                /// 
                /// <remarks>
                /// MSET is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
                /// keys were updated while others are unchanged.
                /// </remarks>
                /// 
                /// <remarks>
                /// http://redis.io/commands/mset
                /// </remarks>
                public static void Set(Dictionary<RedisKey, RedisValue> values)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(values.Keys.ToArray());
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var dictionary = values
                            .Where(k => connections.ElementAt(i).Value.Contains(k.Key))
                            .ToDictionary(d => d.Key, d => d.Value)
                            .ToArray();

                        foreach (var connection in connections.ElementAt(i).Key)
                        {
                            connection
                                .GetDatabase(SharedCache.Instance.Db)
                                .StringSet(dictionary);
                        }
                    }
                }

                /// <summary>
                /// Sets the given keys to their respective values. MSET replaces existing values with new values, just as
                /// regular SET. See MSETNX if you don't want to overwrite existing values.
                /// </summary>
                /// 
                /// <remarks>
                /// MSET is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
                /// keys were updated while others are unchanged.
                /// </remarks>
                /// 
                /// <remarks>
                /// http://redis.io/commands/mset
                /// </remarks>
                public static Task SetAsync(Dictionary<RedisKey, RedisValue> values)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(values.Keys.ToArray());
                    var results = new Task[connections.Count];
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var dictionary = values
                            .Where(k => connections.ElementAt(i).Value.Contains(k.Key))
                            .ToDictionary(d => d.Key, d => d.Value)
                            .ToArray();

                        foreach (var connection in connections.ElementAt(i).Key)
                        {
                            var task = connection
                                .GetDatabase(SharedCache.Instance.Db)
                                .StringSetAsync(dictionary);


                            if (null == results[i])
                                results[i] = task;
                        }
                    }
                    return Task.WhenAll(results);
                }

                /// <summary>
                /// Sets the given keys to their respective values. MSETNX will not perform any operation at all even if just a
                /// single key already exists.
                /// </summary>
                /// 
                /// <remarks>
                /// Because of this semantic MSETNX can be used in order to set different keys representing different fields of
                /// an unique logic object in a way that ensures that either all the fields or none at all are set.
                /// <para>
                /// MSETNX is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
                /// keys were updated while others are unchanged.
                /// </para>
                /// </remarks>
                /// 
                /// <returns>
                /// 1 if all the keys were set, 0 if no key was set (at least one key already existed).
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/msetnx
                /// </remarks>
                public static bool SetIfNotExists(Dictionary<RedisKey, RedisValue> values)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(values.Keys.ToArray());
                    var results = new bool[connections.Count];
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var dictionary = values
                            .Where(k => connections.ElementAt(i).Value.Contains(k.Key))
                            .ToDictionary(d => d.Key, d => d.Value)
                            .ToArray();

                        foreach (var connection in connections.ElementAt(i).Key)
                        {
                            results[i] = connection
                                .GetDatabase(SharedCache.Instance.Db)
                                .StringSet(dictionary, When.NotExists);
                        }
                    }

                    bool res = results.All(b => b);
                    if (res)
                        return true;

                    res = results.All(b => !b);
                    if (res)
                        return false;

                    throw new Exception("inconsistent results");
                }

                /// <summary>
                /// Sets the given keys to their respective values. MSETNX will not perform any operation at all even if just a
                /// single key already exists.
                /// </summary>
                /// 
                /// <remarks>
                /// Because of this semantic MSETNX can be used in order to set different keys representing different fields of
                /// an unique logic object in a way that ensures that either all the fields or none at all are set.
                /// <para>
                /// MSETNX is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
                /// keys were updated while others are unchanged.
                /// </para>
                /// </remarks>
                /// 
                /// <returns>
                /// 1 if the all the keys were set, 0 if no key was set (at least one key already existed).
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/msetnx
                /// </remarks>
                public static Task<bool> SetIfNotExistsAsync(Dictionary<RedisKey, RedisValue> values)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(values.Keys.ToArray());
                    var results = new Task<bool>[connections.Count];
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var dictionary = values
                            .Where(k => connections.ElementAt(i).Value.Contains(k.Key))
                            .ToDictionary(d => d.Key, d => d.Value)
                            .ToArray();

                        foreach (var connection in connections.ElementAt(i).Key)
                        {
                            var task = connection
                                .GetDatabase(SharedCache.Instance.Db)
                                .StringSetAsync(dictionary, When.NotExists);

                            if (null == results[i])
                                results[i] = task;
                        }
                    }

                    var result = Task.Run(async () =>
                    {
                        bool[] blah = await Task.WhenAll(results);
                        bool res = blah.All(b => b);
                        if (res)
                            return true;

                        res = blah.All(b => !b);
                        if (res)
                            return false;

                        throw new Exception("inconsistent results");
                    });
                    return result;
                }

                /// <summary>
                /// Set key to hold string value if key does not exist. In that case, it is equal to SET. When key already holds
                /// a value, no operation is performed.
                /// </summary>
                /// 
                /// <returns>
                /// 1 if the key was set, 0 if the key was not set
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/setnx
                /// </remarks>
                public static bool SetIfNotExists(RedisKey key, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    bool result = false;
                    foreach (var connection in connections)
                    {
                        result = result || connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSet(key, value, when: When.NotExists);
                    }
                    return result;
                }

                /// <summary>
                /// Set key to hold string value if key does not exist. In that case, it is equal to SET. When key already holds
                /// a value, no operation is performed.
                /// </summary>
                /// 
                /// <returns>
                /// 1 if the key was set, 0 if the key was not set
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/setnx
                /// </remarks>
                public static Task<bool> SetIfNotExistsAsync(RedisKey key, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<bool> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSetAsync(key, value, when: When.NotExists);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Returns the bit value at offset in the string value stored at key.
                /// </summary>
                /// 
                /// <remarks>
                /// When offset is beyond the string length, the string is assumed to be a contiguous space with 0 bits. When
                /// key does not exist it is assumed to be an empty string, so offset is always out of range and the value is
                /// also assumed to be a contiguous space with 0 bits.
                /// </remarks>
                /// 
                /// <returns>
                /// the bit value stored at offset.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/getbit
                /// </remarks>
                public static bool GetBit(RedisKey key, long offset)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringGetBit(key, offset);
                }

                /// <summary>
                /// Returns the bit value at offset in the string value stored at key.
                /// </summary>
                /// 
                /// <remarks>
                /// When offset is beyond the string length, the string is assumed to be a contiguous space with 0 bits. When
                /// key does not exist it is assumed to be an empty string, so offset is always out of range and the value is
                /// also assumed to be a contiguous space with 0 bits.
                /// </remarks>
                /// 
                /// <returns>
                /// the bit value stored at offset.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/getbit
                /// </remarks>
                public static Task<bool> GetBitAsync(RedisKey key, long offset)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringGetBitAsync(key, offset);
                }

                /// <summary>
                /// Returns the length of the string value stored at key. An error is returned when key holds a non-string
                /// value.
                /// </summary>
                /// 
                /// <returns>
                /// the length of the string at key, or 0 when key does not exist.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/strlen
                /// </remarks>
                public static long GetLength(RedisKey key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringLength(key);
                }

                /// <summary>
                /// Returns the length of the string value stored at key. An error is returned when key holds a non-string
                /// value.
                /// </summary>
                /// 
                /// <returns>
                /// the length of the string at key, or 0 when key does not exist.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/strlen
                /// </remarks>
                public static Task<long> GetLengthAsync(RedisKey key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringLengthAsync(key);
                }

                /// <summary>
                /// Sets or clears the bit at offset in the string value stored at key.
                /// </summary>
                /// 
                /// <remarks>
                /// The bit is either set or cleared depending on value, which can be either 0 or 1. When key does not exist, a
                /// new string value is created. The string is grown to make sure it can hold a bit at offset. The offset
                /// argument is required to be greater than or equal to 0, and smaller than 232 (this limits bitmaps to 512MB).
                /// When the string at key is grown, added bits are set to 0.
                /// <para>
                /// Warning: When setting the last possible bit (offset equal to 232 -1) and the string value stored at key does
                /// not yet hold a string value, or holds a small string value, Redis needs to allocate all intermediate memory
                /// which can block the server for some time. On a 2010 MacBook Pro, setting bit number 232 -1 (512MB
                /// allocation) takes ~300ms, setting bit number 230 -1 (128MB allocation) takes ~80ms, setting bit number
                /// 228 -1 (32MB allocation) takes ~30ms and setting bit number 226 -1 (8MB allocation) takes ~8ms. Note that
                /// once this first allocation is done, subsequent calls to SETBIT for the same key will not have the allocation
                /// overhead.
                /// </para>
                /// </remarks>
                /// 
                /// <returns>
                /// the original bit value stored at offset.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/setbit
                /// </remarks>
                public static bool SetBit(RedisKey key, long offset, bool value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    bool result = false;
                    foreach (var connection in connections)
                    {
                        result = result || connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSetBit(key, offset, value);
                    }
                    return result;
                }

                /// <summary>
                /// Sets or clears the bit at offset in the string value stored at key.
                /// </summary>
                /// 
                /// <remarks>
                /// The bit is either set or cleared depending on value, which can be either 0 or 1. When key does not exist, a
                /// new string value is created. The string is grown to make sure it can hold a bit at offset. The offset
                /// argument is required to be greater than or equal to 0, and smaller than 232 (this limits bitmaps to 512MB).
                /// When the string at key is grown, added bits are set to 0.
                /// <para>
                /// Warning: When setting the last possible bit (offset equal to 232 -1) and the string value stored at key does
                /// not yet hold a string value, or holds a small string value, Redis needs to allocate all intermediate memory
                /// which can block the server for some time. On a 2010 MacBook Pro, setting bit number 232 -1 (512MB
                /// allocation) takes ~300ms, setting bit number 230 -1 (128MB allocation) takes ~80ms, setting bit number
                /// 228 -1 (32MB allocation) takes ~30ms and setting bit number 226 -1 (8MB allocation) takes ~8ms. Note that
                /// once this first allocation is done, subsequent calls to SETBIT for the same key will not have the allocation
                /// overhead.
                /// </para>
                /// </remarks>
                /// 
                /// <returns>
                /// the original bit value stored at offset.
                /// </returns>
                /// 
                /// <remarks>
                /// http://redis.io/commands/setbit
                /// </remarks>
                public static Task<bool> SetBitAsync(RedisKey key, long offset, bool value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<bool> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSetBitAsync(key, offset, value);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Count the number of set bits (population counting) in a string.
                /// <para>
                /// By default all the bytes contained in the string are examined. It is possible to specify the counting
                /// operation only in an interval passing the additional arguments start and end.
                /// </para>
                /// <para>
                /// Like for the GETRANGE command start and end can contain negative values in order to index bytes starting
                /// from the end of the string, where -1 is the last byte, -2 is the penultimate, and so forth.
                /// </para>
                /// </summary>
                /// 
                /// <remarks>
                /// http://redis.io/commands/bitcount
                /// </remarks>
                public static long CountSetBits(RedisKey key, long start = 0, long count = -1)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    long result = 0;
                    foreach (var connection in connections)
                    {
                        result = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringBitCount(key, start, count);
                    }
                    return result;
                }

                /// <summary>
                /// Count the number of set bits (population counting) in a string.
                /// <para>
                /// By default all the bytes contained in the string are examined. It is possible to specify the counting
                /// operation only in an interval passing the additional arguments start and end.
                /// </para>
                /// <para>
                /// Like for the GETRANGE command start and end can contain negative values in order to index bytes starting
                /// from the end of the string, where -1 is the last byte, -2 is the penultimate, and so forth.
                /// </para>
                /// </summary>
                /// 
                /// <remarks>
                /// http://redis.io/commands/bitcount
                /// </remarks>
                public static Task<long> CountSetBitsAsync(RedisKey key, long start = 0, long count = -1)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<long> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringBitCountAsync(key, start, count);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                // TODO bring back
                ///// <summary>
                ///// Perform a bitwise AND operation between multiple keys (containing string values) and store the result in the
                ///// destination key.
                ///// </summary>
                ///// 
                ///// <returns>
                ///// The size of the string stored in the destination key, that is equal to the size of the longest input string.
                ///// </returns>
                ///// 
                ///// <remarks>
                ///// http://redis.io/commands/bitop
                ///// </remarks>
                //Task<long> BitwiseAnd(RedisKey destination, RedisKey[] keys)
                //{
                //    var connections = SharedCache.Instance.GetWriteConnections(keys);
                //    var dest = SharedCache.Instance.GetWriteConnections(destination);
                //    Task<long> result = null;
                //    // All ops performed on same machine, so we can do this in a MUCH faster way.
                //    if (1 == connections.Count
                //        && connections.ElementAt(0).Key[0].Host == dest[0].Host
                //        && connections.ElementAt(0).Key[0].Port == dest[0].Port)
                //    {
                //        foreach (var connection in connections.ElementAt(0).Key)
                //        {
                //            var task = connection.Strings
                //                .BitwiseAnd(SharedCache.Instance.Db, destination, keys, SharedCache.Instance.QueueJump);
                //            if (null == result)
                //                result = task;
                //        }
                //    }
                //    // Ops on multiple machines. Need to aggregate results. Boo.
                //    else
                //    {
                //        // (1) bitwise AND keys on each server into `destination` + "_temp"
                //        // (2) get each `destination` + "_temp" value from each server
                //        // (3) get the longest length (to return)
                //        // (4) bitwise AND all of the values retrieved
                //        // (5) store the final bitwise'd result into `destination`
                //        // (6) delete all `destination` + "_temp" values from each server
                //        // TODO this
                //        throw new NotImplementedException();
                //    }
                //    return result;
                //}

                ///// <summary>
                ///// Perform a bitwise OR operation between multiple keys (containing string values) and store the result in the
                ///// destination key.
                ///// </summary>
                ///// 
                ///// <returns>
                ///// The size of the string stored in the destination key, that is equal to the size of the longest input string.
                ///// </returns>
                ///// 
                ///// <remarks>
                ///// http://redis.io/commands/bitop
                ///// </remarks>
                //Task<long> BitwiseOr(string destination, string[] keys)
                //{
                //    var connections = SharedCache.Instance.GetWriteConnections(keys);
                //    var dest = SharedCache.Instance.GetWriteConnections(destination);
                //    Task<long> result = null;
                //    // All ops performed on same machine, so we can do this in a MUCH faster way.
                //    if (1 == connections.Count
                //        && connections.ElementAt(0).Key[0].Host == dest[0].Host
                //        && connections.ElementAt(0).Key[0].Port == dest[0].Port)
                //    {
                //        foreach (var connection in connections.ElementAt(0).Key)
                //        {
                //            var task = connection.Strings
                //                .BitwiseOr(SharedCache.Instance.Db, destination, keys, SharedCache.Instance.QueueJump);
                //            if (null == result)
                //                result = task;
                //        }
                //    }
                //    // Ops on multiple machines. Need to aggregate results. Boo.
                //    else
                //    {
                //        // (1) bitwise OR keys on each server into `destination` + "_temp"
                //        // (2) get each `destination` + "_temp" value from each server
                //        // (3) get the longest length (to return)
                //        // (4) bitwise OR all of the values retrieved
                //        // (5) store the final bitwise'd result into `destination`
                //        // (6) delete all `destination` + "_temp" values from each server
                //        // TODO this
                //        throw new NotImplementedException();
                //    }
                //    return result;
                //}

                ///// <summary>
                ///// Perform a bitwise XOR operation between multiple keys (containing string values) and store the result in the
                ///// destination key.
                ///// </summary>
                ///// 
                ///// <returns>
                ///// The size of the string stored in the destination key, that is equal to the size of the longest input string.
                ///// </returns>
                ///// 
                ///// <remarks>
                ///// http://redis.io/commands/bitop
                ///// </remarks>
                //Task<long> BitwiseXOr(string destination, string[] keys)
                //{
                //    var connections = SharedCache.Instance.GetWriteConnections(keys);
                //    var dest = SharedCache.Instance.GetWriteConnections(destination);
                //    Task<long> result = null;
                //    // All ops performed on same machine, so we can do this in a MUCH faster way.
                //    if (1 == connections.Count
                //        && connections.ElementAt(0).Key[0].Host == dest[0].Host
                //        && connections.ElementAt(0).Key[0].Port == dest[0].Port)
                //    {
                //        foreach (var connection in connections.ElementAt(0).Key)
                //        {
                //            var task = connection.Strings
                //                .BitwiseXOr(SharedCache.Instance.Db, destination, keys, SharedCache.Instance.QueueJump);
                //            if (null == result)
                //                result = task;
                //        }
                //    }
                //    // Ops on multiple machines. Need to aggregate results. Boo.
                //    else
                //    {
                //        // (1) bitwise XOR keys on each server into `destination` + "_temp"
                //        // (2) get each `destination` + "_temp" value from each server
                //        // (3) get the longest length (to return)
                //        // (4) bitwise XOR all of the values retrieved
                //        // (5) store the final bitwise'd result into `destination`
                //        // (6) delete all `destination` + "_temp" values from each server
                //        // TODO this
                //        throw new NotImplementedException();
                //    }
                //    return result;
                //}

                ///// <summary>
                ///// Perform a bitwise NOT operation on a key (containing a string value) and store the result in the destination
                ///// key.
                ///// </summary>
                ///// 
                ///// <returns>
                ///// The size of the string stored in the destination key, that is equal to the size of the longest input string.
                ///// </returns>
                ///// 
                ///// <remarks>
                ///// http://redis.io/commands/bitop
                ///// </remarks>
                //Task<long> BitwiseNot(string destination, RedisKey key)
                //{
                //    var connections = SharedCache.Instance.GetWriteConnections(key);
                //    var dest = SharedCache.Instance.GetWriteConnections(destination);
                //    // All ops performed on same machine, so we can do this in a MUCH faster way.
                //    if (1 == connections.Length
                //        && connections[0].Host == dest[0].Host
                //        && connections[0].Port == dest[0].Port)
                //    {
                //        var task = connections[0].Strings
                //            .BitwiseNot(SharedCache.Instance.Db, destination, key, SharedCache.Instance.QueueJump);
                //        return task;
                //    }
                //    // Ops on multiple machines. Need to aggregate results. Boo.
                //    else
                //    {
                //        // (1) bitwise NOT keys on each server into `destination` + "_temp"
                //        // (2) get each `destination` + "_temp" value from each server
                //        // (3) get the longest length (to return)
                //        // (4) bitwise NOT all of the values retrieved
                //        // (5) store the final bitwise'd result into `destination`
                //        // (6) delete all `destination` + "_temp" values from each server
                //        // TODO this
                //        throw new NotImplementedException();
                //        //                Task<long> result = null;
                //        //                return result;
                //    }
                //}

                /// <summary>
                /// This is a composite helper command, to help with using redis as a lock provider. This is achieved as a
                /// RedisKey key/value pair with timeout. If the lock does not exist (or has expired), then a new RedisKey key is
                /// created (with the supplied duration), and <c>true</c> is returned to indicate success. If the lock already
                /// exists, then no lock is taken, and <c>false</c> is returned. The value may be fetched separately, but the
                /// meaning is implementation-defined). No change is made if the lock was not successfully taken. In this case,
                /// the client should delay and retry.
                /// <para>
                /// It is expected that a well-behaved client will also release the lock in a timely fashion via
                /// <see cref="ReleaseLockAsync">ReleaseLockAsync</see>.
                /// </para>
                /// </summary>
                /// 
                /// <returns>
                /// <c>null</c> if the lock was successfully taken; the competing value otherwise
                /// </returns>
                /// 
                /// <remarks>
                /// It transpires that robust locking in redis is actually remarkably hard, and most implementations are broken
                /// in one way or another (most commonly: thread-race, or extending the lock duration when failing to take the
                /// lock).
                /// </remarks>
                public static bool TakeLock(RedisKey key, RedisValue value, TimeSpan expiry)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    bool result = false;
                    foreach (var connection in connections)
                    {
                        result = result || connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .LockTake(key, value, expiry);
                    }
                    return result;
                }

                /// <summary>
                /// This is a composite helper command, to help with using redis as a lock provider. This is achieved as a
                /// RedisKey key/value pair with timeout. If the lock does not exist (or has expired), then a new RedisKey key is
                /// created (with the supplied duration), and <c>true</c> is returned to indicate success. If the lock already
                /// exists, then no lock is taken, and <c>false</c> is returned. The value may be fetched separately, but the
                /// meaning is implementation-defined). No change is made if the lock was not successfully taken. In this case,
                /// the client should delay and retry.
                /// <para>
                /// It is expected that a well-behaved client will also release the lock in a timely fashion via
                /// <see cref="ReleaseLockAsync">ReleaseLockAsync</see>.
                /// </para>
                /// </summary>
                /// 
                /// <returns>
                /// <c>null</c> if the lock was successfully taken; the competing value otherwise
                /// </returns>
                /// 
                /// <remarks>
                /// It transpires that robust locking in redis is actually remarkably hard, and most implementations are broken
                /// in one way or another (most commonly: thread-race, or extending the lock duration when failing to take the
                /// lock).
                /// </remarks>
                public static Task<bool> TakeLockAsync(RedisKey key, RedisValue value, TimeSpan expiry)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    Task<bool> result = null;
                    foreach (var connection in connections)
                    {
                        var task = connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .LockTakeAsync(key, value, expiry);

                        if (null == result)
                            result = task;
                    }
                    return result;
                }

                /// <summary>
                /// Releases a lock that was taken successfully via TakeLock. You should not release a lock that you did not
                /// take, as this will cause problems.
                /// </summary>
                public static void ReleaseLock(RedisKey key, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    foreach (var connection in connections)
                    {
                        connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .LockRelease(key, value);
                    }
                }

                /// <summary>
                /// Releases a lock that was taken successfully via TakeLock. You should not release a lock that you did not
                /// take, as this will cause problems.
                /// </summary>
                public static Task ReleaseLockAsync(RedisKey key, RedisValue value)
                {
                    var connections = SharedCache.Instance.GetWriteConnections(key);
                    foreach (var connection in connections)
                    {
                        connection
                            .GetDatabase(SharedCache.Instance.Db)
                            .LockReleaseAsync(key, value);
                    }
                    return Task.FromResult(false);
                }
            }
        }
    }
}