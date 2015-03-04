// ReSharper disable once CheckNamespace
namespace BB.Caching
{
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
            /// Commands that apply to key/sub-key/value tuples, i.e. where the item is a dictionary of inner values. This
            /// can be useful for modeling members of an entity, for example.
            /// </summary>
            /// <remarks>http://redis.io/commands#hash</remarks>
            public static class Hashes
            {
                /// <summary>
                /// Removes the specified fields from the hash stored at key. Non-existing fields are ignored. Non-existing keys
                /// are treated as empty hashes and this command returns 0.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/hdel
                /// </remarks>
                /// <returns>
                /// The number of fields that were removed.
                /// </returns>
                public static bool Delete(RedisKey key, RedisValue field)
                {
                    bool result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashDelete(key, field);

                    return result;
                }
                
                /// <summary>
                /// Removes the specified fields from the hash stored at key. Non-existing fields are ignored. Non-existing keys
                /// are treated as empty hashes and this command returns 0.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/hdel
                /// </remarks>
                /// <returns>
                /// The number of fields that were removed.
                /// </returns>
                public static Task<bool> DeleteAsync(RedisKey key, RedisValue field)
                {
                    Task<bool> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashDeleteAsync(key, field);

                    return result;
                }

                /// <summary>
                /// Removes the specified fields from the hash stored at key. Non-existing fields are ignored. Non-existing keys
                /// are treated as empty hashes and this command returns 0.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="fields">
                /// The fields.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/hdel
                /// </remarks>
                /// <returns>
                /// The number of fields that were removed.
                /// </returns>
                public static long Delete(RedisKey key, RedisValue[] fields)
                {
                    long result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashDelete(key, fields);

                    return result;
                }

                /// <summary>
                /// Removes the specified fields from the hash stored at key. Non-existing fields are ignored. Non-existing keys
                /// are treated as empty hashes and this command returns 0.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="fields">
                /// The fields.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/hdel
                /// </remarks>
                /// <returns>
                /// The number of fields that were removed.
                /// </returns>
                public static Task<long> DeleteAsync(RedisKey key, RedisValue[] fields)
                {
                    Task<long> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashDeleteAsync(key, fields);

                    return result;
                }

                /// <summary>
                /// Returns if field is an existing field in the hash stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <returns>
                /// 1 if the hash contains field. 0 if the hash does not contain field, or key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hexists
                /// </remarks>
                public static bool Exists(RedisKey key, RedisValue field)
                {
                    bool result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashExists(key, field);

                    return result;
                }

                /// <summary>
                /// Returns if field is an existing field in the hash stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <returns>
                /// 1 if the hash contains field. 0 if the hash does not contain field, or key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hexists
                /// </remarks>
                public static Task<bool> ExistsAsync(RedisKey key, RedisValue field)
                {
                    Task<bool> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashExistsAsync(key, field);

                    return result;
                }

                /// <summary>
                /// Returns the value associated with field in the hash stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <returns>
                /// the value associated with field, or nil when field is not present in the hash or key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hget
                /// </remarks>
                public static RedisValue Get(RedisKey key, RedisValue field)
                {
                    RedisValue result = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashGet(key, field);

                    return result;
                }

                /// <summary>
                /// Returns the value associated with field in the hash stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <returns>
                /// the value associated with field, or nil when field is not present in the hash or key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hget
                /// </remarks>
                public static Task<RedisValue> GetAsync(RedisKey key, RedisValue field)
                {
                    Task<RedisValue> result = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashGetAsync(key, field);

                    return result;
                }

                /// <summary>
                /// Returns the values associated with the specified fields in the hash stored at key. For every field that does
                /// not exist in the hash, a nil value is returned.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="fields">
                /// The fields.
                /// </param>
                /// <returns>
                /// list of values associated with the given fields, in the same order as they are requested.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hmget
                /// </remarks>
                public static RedisValue[] Get(RedisKey key, RedisValue[] fields)
                {
                    RedisValue[] result = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashGet(key, fields);

                    return result;
                }

                /// <summary>
                /// Returns the values associated with the specified fields in the hash stored at key. For every field that does
                /// not exist in the hash, a nil value is returned.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="fields">
                /// The fields.
                /// </param>
                /// <returns>
                /// list of values associated with the given fields, in the same order as they are requested.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hmget
                /// </remarks>
                public static Task<RedisValue[]> GetAsync(RedisKey key, RedisValue[] fields)
                {
                    Task<RedisValue[]> result = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashGetAsync(key, fields);

                    return result;
                }

                /// <summary>
                /// Returns all fields and values of the hash stored at key. 
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// list of fields and their values stored in the hash, or an empty list when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hgetall
                /// </remarks>
                public static HashEntry[] GetAll(RedisKey key)
                {
                    HashEntry[] result = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashGetAll(key);

                    return result;
                }

                /// <summary>
                /// Returns all fields and values of the hash stored at key. 
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// list of fields and their values stored in the hash, or an empty list when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hgetall
                /// </remarks>
                public static Task<HashEntry[]> GetAllAsync(RedisKey key)
                {
                    Task<HashEntry[]> result = SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashGetAllAsync(key);

                    return result;
                }

                /// <summary>
                /// Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new
                /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
                /// integer, the value is set to 0 before the operation is performed.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The amount to increment by.
                /// </param>
                /// <remarks>
                /// The range of values supported by HINCRBY is limited to 64 bit signed integers.
                /// </remarks>
                /// <returns>
                /// the value at field after the increment operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hincrby
                /// </remarks>
                public static long Increment(RedisKey key, RedisValue field, int value = 1)
                {
                    long result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashIncrement(key, field, value);

                    return result;
                }

                /// <summary>
                /// Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new
                /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
                /// integer, the value is set to 0 before the operation is performed.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The amount to increment by.
                /// </param>
                /// <remarks>
                /// The range of values supported by HINCRBY is limited to 64 bit signed integers.
                /// </remarks>
                /// <returns>
                /// the value at field after the increment operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hincrby
                /// </remarks>
                public static Task<long> IncrementAsync(RedisKey key, RedisValue field, int value = 1)
                {
                    Task<long> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashIncrementAsync(key, field, value);

                    return result;
                }

                /// <summary>
                /// Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new
                /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
                /// integer, the value is set to 0 before the operation is performed.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The amount to increment by.
                /// </param>
                /// <remarks>
                /// The range of values supported by HINCRBY is limited to 64 bit signed integers.
                /// </remarks>
                /// <returns>
                /// the value at field after the increment operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hincrby
                /// </remarks>
                public static double Increment(RedisKey key, RedisValue field, double value)
                {
                    double result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashIncrement(key, field, value);

                    return result;
                }

                /// <summary>
                /// Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new
                /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
                /// integer, the value is set to 0 before the operation is performed.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The amount to increment by.
                /// </param>
                /// <remarks>
                /// The range of values supported by HINCRBY is limited to 64 bit signed integers.
                /// </remarks>
                /// <returns>
                /// the value at field after the increment operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hincrby
                /// </remarks>
                public static Task<double> IncrementAsync(RedisKey key, RedisValue field, double value)
                {
                    Task<double> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashIncrementAsync(key, field, value);

                    return result;
                }

                /// <summary>
                /// Decrements the number stored at field in the hash stored at key by Decrement. If key does not exist, a new
                /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
                /// integer, the value is set to 0 before the operation is performed.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The amount to decrement by.
                /// </param>
                /// <remarks>
                /// The range of values supported by HINCRBY is limited to 64 bit signed integers.
                /// </remarks>
                /// <returns>
                /// the value at field after the decrement operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hincrby
                /// </remarks>
                public static long Decrement(RedisKey key, RedisValue field, int value = 1)
                {
                    long result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashDecrement(key, field, value);

                    return result;
                }

                /// <summary>
                /// Decrements the number stored at field in the hash stored at key by Decrement. If key does not exist, a new
                /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
                /// integer, the value is set to 0 before the operation is performed.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The amount to decrement by.
                /// </param>
                /// <remarks>
                /// The range of values supported by HINCRBY is limited to 64 bit signed integers.
                /// </remarks>
                /// <returns>
                /// the value at field after the decrement operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hincrby
                /// </remarks>
                public static Task<long> DecrementAsync(RedisKey key, RedisValue field, int value = 1)
                {
                    Task<long> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashDecrementAsync(key, field, value);

                    return result;
                }

                /// <summary>
                /// Decrements the number stored at field in the hash stored at key by Decrement. If key does not exist, a new
                /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
                /// integer, the value is set to 0 before the operation is performed.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The amount to decrement by.
                /// </param>
                /// <remarks>
                /// The range of values supported by HINCRBY is limited to 64 bit signed integers.
                /// </remarks>
                /// <returns>
                /// the value at field after the decrement operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hincrby
                /// </remarks>
                public static double Decrement(RedisKey key, RedisValue field, double value)
                {
                    double result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashDecrement(key, field, value);

                    return result;
                }

                /// <summary>
                /// Decrements the number stored at field in the hash stored at key by Decrement. If key does not exist, a new
                /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
                /// integer, the value is set to 0 before the operation is performed.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The amount to decrement by.
                /// </param>
                /// <remarks>
                /// The range of values supported by HINCRBY is limited to 64 bit signed integers.
                /// </remarks>
                /// <returns>
                /// the value at field after the decrement operation.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hincrby
                /// </remarks>
                public static Task<double> DecrementAsync(RedisKey key, RedisValue field, double value)
                {
                    Task<double> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashDecrementAsync(key, field, value);

                    return result;
                }

                /// <summary>
                /// Returns all field names in the hash stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// list of fields in the hash, or an empty list when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hkeys
                /// </remarks>
                public static RedisValue[] GetKeys(RedisKey key)
                {
                    RedisValue[] result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashKeys(key);

                    return result;
                }

                /// <summary>
                /// Returns all field names in the hash stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// list of fields in the hash, or an empty list when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hkeys
                /// </remarks>
                public static Task<RedisValue[]> GetKeysAsync(RedisKey key)
                {
                    Task<RedisValue[]> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashKeysAsync(key);

                    return result;
                }

                /// <summary>
                /// Returns all values in the hash stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// list of values in the hash, or an empty list when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hvals
                /// </remarks>
                public static RedisValue[] GetValues(RedisKey key)
                {
                    RedisValue[] result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashValues(key);

                    return result;
                }

                /// <summary>
                /// Returns all values in the hash stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// list of values in the hash, or an empty list when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hvals
                /// </remarks>
                public static Task<RedisValue[]> GetValuesAsync(RedisKey key)
                {
                    Task<RedisValue[]> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashValuesAsync(key);

                    return result;
                }

                /// <summary>
                /// Returns the number of fields contained in the hash stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// number of fields in the hash, or 0 when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hlen
                /// </remarks>
                public static long GetLength(RedisKey key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashLength(key);
                }

                /// <summary>
                /// Returns the number of fields contained in the hash stored at key.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <returns>
                /// number of fields in the hash, or 0 when key does not exist.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hlen
                /// </remarks>
                public static Task<long> GetLengthAsync(RedisKey key)
                {
                    return SharedCache.Instance.GetReadConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashLengthAsync(key);
                }

                /// <summary>
                /// Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created.
                /// If field already exists in the hash, it is overwritten.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and the value
                /// was updated.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hset
                /// </remarks>
                public static bool Set(RedisKey key, RedisValue field, RedisValue value)
                {
                    bool result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashSet(key, field, value);

                    return result;
                }

                /// <summary>
                /// Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created.
                /// If field already exists in the hash, it is overwritten.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and the value
                /// was updated.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hset
                /// </remarks>
                public static Task<bool> SetAsync(RedisKey key, RedisValue field, RedisValue value)
                {
                    Task<bool> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashSetAsync(key, field, value);

                    return result;
                }

                /// <summary>
                /// Sets the specified fields to their respective values in the hash stored at key. This command overwrites any
                /// existing fields in the hash. If key does not exist, a new key holding a hash is created.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="values">
                /// The values.
                /// </param>
                /// <remarks>
                /// http://redis.io/commands/hmset
                /// </remarks>
                public static void Set(RedisKey key, HashEntry[] values)
                {
                    SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashSet(key, values);
                }

                /// <summary>
                /// Sets the specified fields to their respective values in the hash stored at key. This command overwrites any
                /// existing fields in the hash. If key does not exist, a new key holding a hash is created.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="values">
                /// The values.
                /// </param>
                /// <returns>
                /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and the value
                /// was updated.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hmset
                /// </remarks>
                public static Task SetAsync(RedisKey key, HashEntry[] values)
                {
                    Task result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashSetAsync(key, values);

                    return result;
                }

                /// <summary>
                /// Sets field in the hash stored at key to value, only if field does not yet exist. If key does not exist, a
                /// new key holding a hash is created. If field already exists, this operation has no effect.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and no
                /// operation was performed.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hsetnx
                /// </remarks>
                public static bool SetIfNotExists(RedisKey key, RedisValue field, RedisValue value)
                {
                    bool result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashSet(key, field, value, When.NotExists);

                    return result;
                }

                /// <summary>
                /// Sets field in the hash stored at key to value, only if field does not yet exist. If key does not exist, a
                /// new key holding a hash is created. If field already exists, this operation has no effect.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="field">
                /// The field.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <returns>
                /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and no
                /// operation was performed.
                /// </returns>
                /// <remarks>
                /// http://redis.io/commands/hsetnx
                /// </remarks>
                public static Task<bool> SetIfNotExistsAsync(RedisKey key, RedisValue field, RedisValue value)
                {
                    Task<bool> result = SharedCache.Instance.GetWriteConnection(key)
                        .GetDatabase(SharedCache.Instance.Db)
                        .HashSetAsync(key, field, value, When.NotExists);

                    return result;
                }
            }
        }
    }
}