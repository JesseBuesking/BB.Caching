﻿// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
            /// Commands that apply to key/value pairs, where the value can be a string, a BLOB, or interpreted as a number
            /// </summary>
            /// <remarks>http://redis.io/commands#string</remarks>
            public static class Strings
            {
                /// <summary>
                /// If key already exists and is a string, this command appends the value at the end of the string. If key does
                /// not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// the length of the string after the append operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/append
                /// </remarks>
                public static long Append(RedisKey key, RedisValue value)
                {
                    long result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringAppend(key, value);

                    return result;
                }

                /// <summary>
                /// If key already exists and is a string, this command appends the value at the end of the string. If key does
                /// not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// the length of the string after the append operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/append
                /// </remarks>
                public static Task<long> AppendAsync(RedisKey key, RedisValue value)
                {
                    Task<long> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringAppendAsync(key, value);

                    return result;
                }

                /// <summary>
                /// If key already exists and is a string, this command appends the value at the end of the string. If key does
                /// not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// the length of the string after the append operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/append
                /// </remarks>
                public static long Append(RedisKey key, byte[] value)
                {
                    long result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringAppend(key, value);

                    return result;
                }

                /// <summary>
                /// If key already exists and is a string, this command appends the value at the end of the string. If key does
                /// not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// the length of the string after the append operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/append
                /// </remarks>
                public static Task<long> AppendAsync(RedisKey key, byte[] value)
                {
                    Task<long> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringAppendAsync(key, value);

                    return result;
                }

                /// <summary>
                /// Decrements the number stored at key by decrement. If the key does not exist, it is set to 0 before
                /// performing the operation. An error is returned if the key contains a value of the wrong type or contains a
                /// string that is not representable as integer. This operation is limited to 64 bit signed integers.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// the value of key after the increment
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/decrby
                /// </remarks>
                /// <remarks>
                /// http://redis.io/commands/decr
                /// </remarks>
                public static long Decrement(RedisKey key, long value = 1)
                {
                    long result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringDecrement(key, value);

                    return result;
                }

                /// <summary>
                /// Decrements the number stored at key by decrement. If the key does not exist, it is set to 0 before
                /// performing the operation. An error is returned if the key contains a value of the wrong type or contains a
                /// string that is not representable as integer. This operation is limited to 64 bit signed integers.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// the value of key after the increment
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/decrby
                /// </remarks>
                /// <remarks>
                /// http://redis.io/commands/decr
                /// </remarks>
                public static Task<long> DecrementAsync(RedisKey key, long value = 1)
                {
                    Task<long> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringDecrementAsync(key, value);

                    return result;
                }

                /// <summary>
                /// Increments the number stored at key by increment. If the key does not exist, it is set to 0 before
                /// performing the operation. An error is returned if the key contains a value of the wrong type or contains a
                /// string that is not representable as integer. This operation is limited to 64 bit signed integers.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// the value of key after the increment
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/incrby
                /// </remarks>
                /// <remarks>
                /// http://redis.io/commands/incr
                /// </remarks>
                public static long Increment(RedisKey key, long value = 1)
                {
                    long result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringIncrement(key, value);

                    return result;
                }

                /// <summary>
                /// Increments the number stored at key by increment. If the key does not exist, it is set to 0 before
                /// performing the operation. An error is returned if the key contains a value of the wrong type or contains a
                /// string that is not representable as integer. This operation is limited to 64 bit signed integers.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// the value of key after the increment
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/incrby
                /// </remarks>
                /// <remarks>
                /// http://redis.io/commands/incr
                /// </remarks>
                public static Task<long> IncrementAsync(RedisKey key, long value = 1)
                {
                    Task<long> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringIncrementAsync(key, value);

                    return result;
                }

                /// <summary>
                /// Get the value of key. If the key does not exist the special value nil is returned. An error is returned if
                /// the value stored at key is not a string, because GET only handles string values.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// the value of key, or nil when key does not exist.
                /// </returns>
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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// the value of key, or nil when key does not exist.
                /// </returns>
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
                /// Get the value of key. If the key does not exist the special value nil is returned. An error is returned if
                /// the value stored at key is not a string, because GET only handles string values.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="expire">
                /// The expire.
                /// </param>
                /// <returns>
                /// the value of key, or nil when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/get
                /// </remarks>
                public static RedisValue Get(RedisKey key, TimeSpan expire)
                {
                    var tran = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .CreateTransaction();

                    Task<RedisValue> result = tran.StringGetAsync(key);
                    tran.KeyExpireAsync(key, expire);

                    tran.Execute();
                    return result.Result;
                }

                // TODO why is this slower than calling each separately?

                /// <summary>
                /// Get the value of key. If the key does not exist the special value nil is returned. An error is returned if
                /// the value stored at key is not a string, because GET only handles string values.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="expire">
                /// The expire.
                /// </param>
                /// <returns>
                /// the value of key, or nil when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/get
                /// </remarks>
                public static async Task<RedisValue> GetAsync(RedisKey key, TimeSpan expire)
                {
                    var tran = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .CreateTransaction();

                    Task<RedisValue> result = tran.StringGetAsync(key);
#pragma warning disable 4014
                    tran.KeyExpireAsync(key, expire);
                    tran.ExecuteAsync();
#pragma warning restore 4014
                    return await result;
                }

                /// <summary>
                /// Returns the substring of the string value stored at key, determined by the offsets start and end (both are
                /// inclusive).
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="start">
                /// The start.
                /// </param>
                /// <param name="end">
                /// The end.
                /// </param>
                /// <remarks>
                /// Negative offsets can be used in order to provide an offset starting from the end of the string. So -1 means
                /// the last character, -2 the penultimate and so forth. The function handles out of range requests by limiting
                /// the resulting range to the actual length of the string.
                /// </remarks>
                /// <returns>
                /// the value of key, or nil when key does not exist.
                /// </returns>
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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="start">
                /// The start.
                /// </param>
                /// <param name="end">
                /// The end.
                /// </param>
                /// <remarks>
                /// Negative offsets can be used in order to provide an offset starting from the end of the string. So -1 means
                /// the last character, -2 the penultimate and so forth. The function handles out of range requests by limiting
                /// the resulting range to the actual length of the string.
                /// </remarks>
                /// <returns>
                /// the value of key, or nil when key does not exist.
                /// </returns>
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
                /// <param name="keys">
                /// The keys.
                /// </param>
                /// <returns>
                /// list of values at the specified keys.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/mget
                /// </remarks>
                public static RedisValue[] Get(RedisKey[] keys)
                {
                    var dictionary = SharedCache.Instance.GetWriteConnection(keys);
                    var values = new RedisValue[dictionary.Count][];
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        values[i] = dictionary.ElementAt(i).Key
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringGet(dictionary.ElementAt(i).Value);
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
                            {
                                break;
                            }
                        }

                        // Cache the results.
                        if (null == results[i])
                        {
                            results[i] = values[i];
                        }

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
                /// <param name="keys">
                /// The keys.
                /// </param>
                /// <returns>
                /// list of values at the specified keys.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/mget
                /// </remarks>
                public static Task<RedisValue[]> GetAsync(RedisKey[] keys)
                {
                    var dictionary = SharedCache.Instance.GetWriteConnection(keys);
                    var tasks = new Task<RedisValue[]>[dictionary.Count];
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        tasks[i] = dictionary.ElementAt(i).Key
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringGetAsync(dictionary.ElementAt(i).Value);
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
                                {
                                    break;
                                }
                            }

                            // Cache the results.
                            if (null == results[i])
                            {
                                results[i] = await tasks[i];
                            }

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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// the old value stored at key, or nil when key did not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/getset
                /// </remarks>
                public static RedisValue GetSet(RedisKey key, RedisValue value)
                {
                    RedisValue result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringGetSet(key, value);

                    return result;
                }

                /// <summary>
                /// Atomically sets key to value and returns the old value stored at key. Returns an error when key exists but
                /// does not hold a string value.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// the old value stored at key, or nil when key did not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/getset
                /// </remarks>
                public static Task<RedisValue> GetSetAsync(RedisKey key, RedisValue value)
                {
                    Task<RedisValue> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringGetSetAsync(key, value);

                    return result;
                }

                /// <summary>
                /// Atomically sets key to value and returns the old value stored at key. Returns an error when key exists but
                /// does not hold a string value.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <param name="expire">
                /// An expiration lifetime.
                /// </param>
                /// <returns>
                /// the old value stored at key, or nil when key did not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/getset
                /// </remarks>
                public static RedisValue GetSet(RedisKey key, RedisValue value, TimeSpan expire)
                {
                    var tran = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .CreateTransaction();

                    Task<RedisValue> result = tran.StringGetSetAsync(key, value);
                    tran.KeyExpireAsync(key, expire);

                    tran.Execute();

                    return null == result ? RedisValue.Null : result.Result;
                }

                /// <summary>
                /// Atomically sets key to value and returns the old value stored at key. Returns an error when key exists but
                /// does not hold a string value.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <param name="expire">
                /// An expiration lifetime.
                /// </param>
                /// <returns>
                /// the old value stored at key, or nil when key did not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/getset
                /// </remarks>
                public static async Task<RedisValue> GetSetAsync(RedisKey key, RedisValue value, TimeSpan expire)
                {
                    var tran = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .CreateTransaction();

                    Task<RedisValue> result = tran.StringGetSetAsync(key, value);

#pragma warning disable 4014
                    tran.KeyExpireAsync(key, expire);
                    tran.ExecuteAsync();
#pragma warning restore 4014

                    return null == result ? RedisValue.Null : await result;
                }

                /// <summary>
                /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/set
                /// </remarks>
                public static void Set(RedisKey key, RedisValue value)
                {
                    SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringSet(key, value);
                }

                /// <summary>
                /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/set
                /// </remarks>
                /// <returns>
                /// The <see cref="Task"/> (void).
                /// </returns>
                public static Task SetAsync(RedisKey key, RedisValue value)
                {
                    Task result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringSetAsync(key, value);

                    return result;
                }

                /// <summary>
                /// Set key to hold the string value and set key to timeout after a given number of seconds.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <param name="expiry">
                /// An expiration lifetime.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/setex
                /// </remarks>
                public static void Set(RedisKey key, RedisValue value, TimeSpan expiry)
                {
                    SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringSet(key, value, expiry);
                }

                /// <summary>
                /// Set key to hold the string value and set key to timeout after a given number of seconds.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <param name="expiry">
                /// An expiration lifetime.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/setex
                /// </remarks>
                /// <returns>
                /// The <see cref="Task"/> (void).
                /// </returns>
                public static Task SetAsync(RedisKey key, RedisValue value, TimeSpan expiry)
                {
                    Task result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringSetAsync(key, value, expiry);

                    return result;
                }

                /// <summary>
                /// Overwrites part of the string stored at key, starting at the specified offset, for the entire length of
                /// value. If the offset is larger than the current length of the string at key, the string is padded with
                /// zero-bytes to make offset fit. Non-existing keys are considered as empty strings, so this command will make
                /// sure it holds a string large enough to be able to set value at offset.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="offset">
                /// The offset.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
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
                /// <remarks>
                /// http://redis.io/commands/setrange
                /// </remarks>
                /// <returns>
                /// the length of the string after it was modified by the command.
                /// </returns>
                public static RedisValue Set(RedisKey key, long offset, RedisValue value)
                {
                    RedisValue result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringSetRange(key, offset, value);

                    return result;
                }

                /// <summary>
                /// Overwrites part of the string stored at key, starting at the specified offset, for the entire length of
                /// value. If the offset is larger than the current length of the string at key, the string is padded with
                /// zero-bytes to make offset fit. Non-existing keys are considered as empty strings, so this command will make
                /// sure it holds a string large enough to be able to set value at offset.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="offset">
                /// The offset.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
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
                /// <remarks>
                /// http://redis.io/commands/setrange
                /// </remarks>
                /// <returns>
                /// the length of the string after it was modified by the command.
                /// </returns>
                public static Task<RedisValue> SetAsync(RedisKey key, long offset, RedisValue value)
                {
                    Task<RedisValue> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringSetRangeAsync(key, offset, value);

                    return result;
                }

                /// <summary>
                /// Sets the given keys to their respective values. MSET replaces existing values with new values, just as
                /// regular SET. See MSETNX if you don't want to overwrite existing values.
                /// </summary>
                /// <param name="values">
                /// A collection of key-value pairs to be set.
                /// </param>
                /// <remarks>
                /// MSET is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
                /// keys were updated while others are unchanged.
                /// </remarks>
                /// <remarks>
                /// http://redis.io/commands/mset
                /// </remarks>
                public static void Set(Dictionary<RedisKey, RedisValue> values)
                {
                    var connections = SharedCache.Instance.GetWriteConnection(values.Keys.ToArray());
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var dictionary = values
                            .Where(k => connections.ElementAt(i).Value.Contains(k.Key))
                            .ToDictionary(d => d.Key, d => d.Value)
                            .ToArray();

                        connections.ElementAt(i).Key
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSet(dictionary);
                    }
                }

                /// <summary>
                /// Sets the given keys to their respective values. MSET replaces existing values with new values, just as
                /// regular SET. See MSETNX if you don't want to overwrite existing values.
                /// </summary>
                /// <param name="values">
                /// A collection of key-value pairs to be set.
                /// </param>
                /// <remarks>
                /// MSET is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
                /// keys were updated while others are unchanged.
                /// </remarks>
                /// <remarks>
                /// http://redis.io/commands/mset
                /// </remarks>
                /// <returns>
                /// The <see cref="Task"/> (void).
                /// </returns>
                public static Task SetAsync(Dictionary<RedisKey, RedisValue> values)
                {
                    var connections = SharedCache.Instance.GetWriteConnection(values.Keys.ToArray());
                    var results = new Task[connections.Count];
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var dictionary = values
                            .Where(k => connections.ElementAt(i).Value.Contains(k.Key))
                            .ToDictionary(d => d.Key, d => d.Value)
                            .ToArray();

                        results[i] = connections.ElementAt(i).Key
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSetAsync(dictionary);
                    }

                    return Task.WhenAll(results);
                }

                /// <summary>
                /// Sets the given keys to their respective values. MSETNX will not perform any operation at all even if just a
                /// single key already exists.
                /// </summary>
                /// <param name="values">
                /// A collection of key-value pairs to be set.
                /// </param>
                /// <remarks>
                /// Because of this semantic MSETNX can be used in order to set different keys representing different fields of
                /// an unique logic object in a way that ensures that either all the fields or none at all are set.
                /// <para>
                /// MSETNX is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
                /// keys were updated while others are unchanged.
                /// </para>
                /// </remarks>
                /// <returns>
                /// 1 if all the keys were set, 0 if no key was set (at least one key already existed).
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/msetnx
                /// </remarks>
                public static bool SetIfNotExists(Dictionary<RedisKey, RedisValue> values)
                {
                    var connections = SharedCache.Instance.GetWriteConnection(values.Keys.ToArray());
                    var results = new bool[connections.Count];
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var dictionary = values
                            .Where(k => connections.ElementAt(i).Value.Contains(k.Key))
                            .ToDictionary(d => d.Key, d => d.Value)
                            .ToArray();

                        results[i] = connections.ElementAt(i).Key
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSet(dictionary, When.NotExists);
                    }

                    bool res = results.All(b => b);
                    if (res)
                    {
                        return true;
                    }

                    res = results.All(b => !b);
                    if (res)
                    {
                        return false;
                    }

                    throw new Exception("inconsistent results");
                }

                /// <summary>
                /// Sets the given keys to their respective values. MSETNX will not perform any operation at all even if just a
                /// single key already exists.
                /// </summary>
                /// <param name="values">
                /// A collection of key-value pairs to be set.
                /// </param>
                /// <remarks>
                /// Because of this semantic MSETNX can be used in order to set different keys representing different fields of
                /// an unique logic object in a way that ensures that either all the fields or none at all are set.
                /// <para>
                /// MSETNX is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
                /// keys were updated while others are unchanged.
                /// </para>
                /// </remarks>
                /// <returns>
                /// 1 if all the keys were set, 0 if no key was set (at least one key already existed).
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/msetnx
                /// </remarks>
                public static Task<bool> SetIfNotExistsAsync(Dictionary<RedisKey, RedisValue> values)
                {
                    var connections = SharedCache.Instance.GetWriteConnection(values.Keys.ToArray());
                    var results = new Task<bool>[connections.Count];
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var dictionary = values
                            .Where(k => connections.ElementAt(i).Value.Contains(k.Key))
                            .ToDictionary(d => d.Key, d => d.Value)
                            .ToArray();

                        results[i] = connections.ElementAt(i).Key
                            .GetDatabase(SharedCache.Instance.Db)
                            .StringSetAsync(dictionary, When.NotExists);
                    }

                    var result = Task.Run(async () =>
                    {
                        bool[] blah = await Task.WhenAll(results);
                        bool res = blah.All(b => b);
                        if (res)
                        {
                            return true;
                        }

                        res = blah.All(b => !b);
                        if (res)
                        {
                            return false;
                        }

                        throw new Exception("inconsistent results");
                    });
                    return result;
                }

                /// <summary>
                /// Set key to hold string value if key does not exist. In that case, it is equal to SET. When key already holds
                /// a value, no operation is performed.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// 1 if the key was set, 0 if the key was not set
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/setnx
                /// </remarks>
                public static bool SetIfNotExists(RedisKey key, RedisValue value)
                {
                    bool result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringSet(key, value, when: When.NotExists);

                    return result;
                }

                /// <summary>
                /// Set key to hold string value if key does not exist. In that case, it is equal to SET. When key already holds
                /// a value, no operation is performed.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// 1 if the key was set, 0 if the key was not set
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/setnx
                /// </remarks>
                public static Task<bool> SetIfNotExistsAsync(RedisKey key, RedisValue value)
                {
                    Task<bool> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringSetAsync(key, value, when: When.NotExists);

                    return result;
                }

                /// <summary>
                /// Returns the bit value at offset in the string value stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="offset">
                /// The offset.
                /// </param>
                /// <remarks>
                /// When offset is beyond the string length, the string is assumed to be a contiguous space with 0 bits. When
                /// key does not exist it is assumed to be an empty string, so offset is always out of range and the value is
                /// also assumed to be a contiguous space with 0 bits.
                /// </remarks>
                /// <returns>
                /// the bit value stored at offset.
                /// </returns>
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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="offset">
                /// The offset.
                /// </param>
                /// <remarks>
                /// When offset is beyond the string length, the string is assumed to be a contiguous space with 0 bits. When
                /// key does not exist it is assumed to be an empty string, so offset is always out of range and the value is
                /// also assumed to be a contiguous space with 0 bits.
                /// </remarks>
                /// <returns>
                /// the bit value stored at offset.
                /// </returns>
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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// the length of the string at key, or 0 when key does not exist.
                /// </returns>
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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// the length of the string at key, or 0 when key does not exist.
                /// </returns>
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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="offset">
                /// The offset.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
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
                /// <returns>
                /// the original bit value stored at offset.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/setbit
                /// </remarks>
                public static bool SetBit(RedisKey key, long offset, bool value)
                {
                    bool result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringSetBit(key, offset, value);

                    return result;
                }

                /// <summary>
                /// Sets or clears the bit at offset in the string value stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="offset">
                /// The offset.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
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
                /// <returns>
                /// the original bit value stored at offset.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/setbit
                /// </remarks>
                public static Task<bool> SetBitAsync(RedisKey key, long offset, bool value)
                {
                    Task<bool> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringSetBitAsync(key, offset, value);

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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="start">
                /// The start.
                /// </param>
                /// <param name="end">
                /// The end.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/bitcount
                /// </remarks>
                /// <returns>
                /// The number of set bits.
                /// </returns>
                public static long CountSetBits(RedisKey key, long start = 0, long end = -1)
                {
                    long result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringBitCount(key, start, end);

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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="start">
                /// The start.
                /// </param>
                /// <param name="end">
                /// The end.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/bitcount
                /// </remarks>
                /// <returns>
                /// The number of set bits.
                /// </returns>
                public static Task<long> CountSetBitsAsync(RedisKey key, long start = 0, long end = -1)
                {
                    Task<long> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .StringBitCountAsync(key, start, end);

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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <param name="expiry">
                /// An expiration lifetime.
                /// </param>
                /// <returns>
                /// <c>null</c> if the lock was successfully taken; the competing value otherwise
                /// </returns>
                /// <remarks>
                /// It transpires that robust locking in redis is actually remarkably hard, and most implementations are broken
                /// in one way or another (most commonly: thread-race, or extending the lock duration when failing to take the
                /// lock).
                /// </remarks>
                public static bool TakeLock(RedisKey key, RedisValue value, TimeSpan expiry)
                {
                    bool result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .LockTake(key, value, expiry);

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
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <param name="expiry">
                /// An expiration lifetime.
                /// </param>
                /// <returns>
                /// <c>null</c> if the lock was successfully taken; the competing value otherwise
                /// </returns>
                /// <remarks>
                /// It transpires that robust locking in redis is actually remarkably hard, and most implementations are broken
                /// in one way or another (most commonly: thread-race, or extending the lock duration when failing to take the
                /// lock).
                /// </remarks>
                public static Task<bool> TakeLockAsync(RedisKey key, RedisValue value, TimeSpan expiry)
                {
                    Task<bool> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .LockTakeAsync(key, value, expiry);

                    return result;
                }

                /// <summary>
                /// Releases a lock that was taken successfully via TakeLock. You should not release a lock that you did not
                /// take, as this will cause problems.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                public static void ReleaseLock(RedisKey key, RedisValue value)
                {
                    SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .LockRelease(key, value);
                }

                /// <summary>
                /// Releases a lock that was taken successfully via TakeLock. You should not release a lock that you did not
                /// take, as this will cause problems.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// The <see cref="Task"/> (void).
                /// </returns>
                public static Task ReleaseLockAsync(RedisKey key, RedisValue value)
                {
                    Task result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .LockReleaseAsync(key, value);

                    return result;
                }
            }
        }
    }
}