using System.Collections.Generic;
using System.Threading.Tasks;

namespace BB.Caching.Shared
{
    /// <summary>
    /// Commands that apply to key/sub-key/value tuples, i.e. where the item is a dictionary of inner values. This can
    /// be useful for modeling members of an entity, for example.
    /// </summary>
    /// <remarks>http://redis.io/commands#hash</remarks>
    public interface IHashes
    {
        /// <summary>
        /// Removes the specified fields from the hash stored at key. Non-existing fields are ignored. Non-existing keys
        /// are treated as empty hashes and this command returns 0.
        /// </summary>
        /// <remarks>http://redis.io/commands/hdel</remarks>
        /// <returns>The number of fields that were removed.</returns>
        Task<bool> Remove(string key, string field);

        /// <summary>
        /// Removes the specified fields from the hash stored at key. Non-existing fields are ignored. Non-existing keys
        /// are treated as empty hashes and this command returns 0.
        /// </summary>
        /// <remarks>http://redis.io/commands/hdel</remarks>
        /// <returns>The number of fields that were removed.</returns>
        Task<long> Remove(string key, string[] fields);

        /// <summary>
        /// Returns if field is an existing field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// 1 if the hash contains field. 0 if the hash does not contain field, or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hexists</remarks>
        Task<bool> Exists(string key, string field);

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        Wrapper<string, string> GetString(string key, string field);

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        Wrapper<long, long?> GetInt64(string key, string field);

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        Wrapper<double, double?> GetDouble(string key, string field);

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        Wrapper<byte[], byte[]> GetByteArray(string key, string field);

        /// <summary>
        /// Returns the values associated with the specified fields in the hash stored at key. For every field that does
        /// not exist in the hash, a nil value is returned.
        /// </summary>
        /// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
        /// <remarks>http://redis.io/commands/hmget</remarks>
        Wrapper<string[], string[]> GetString(string key, string[] fields);

        /// <summary>
        /// Returns the values associated with the specified fields in the hash stored at key. For every field that does
        /// not exist in the hash, a nil value is returned.
        /// </summary>
        /// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
        /// <remarks>http://redis.io/commands/hmget</remarks>
        Wrapper<byte[][], byte[][]> GetByteArray(string key, string[] fields);

        /// <summary>
        /// Returns all fields and values of the hash stored at key. 
        /// </summary>
        /// <returns>
        /// list of fields and their values stored in the hash, or an empty list when key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hgetall</remarks>
        Task<Dictionary<string, byte[]>> GetAll(string key);

        /// <summary>
        /// Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new
        /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
        /// integer, the value is set to 0 before the operation is performed.
        /// </summary>
        /// <remarks>The range of values supported by HINCRBY is limited to 64 bit signed integers.</remarks>
        /// <returns>the value at field after the increment operation.</returns>
        /// <remarks>http://redis.io/commands/hincrby</remarks>
        Task<long> Increment(string key, string field, int value = 1);

        /// <summary>
        /// Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new
        /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
        /// integer, the value is set to 0 before the operation is performed.
        /// </summary>
        /// <remarks>The range of values supported by HINCRBY is limited to 64 bit signed integers.</remarks>
        /// <returns>the value at field after the increment operation.</returns>
        /// <remarks>http://redis.io/commands/hincrby</remarks>
        Task<double> Increment(string key, string field, double value);

        /// <summary>
        /// Decrements the number stored at field in the hash stored at key by Decrement. If key does not exist, a new
        /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
        /// integer, the value is set to 0 before the operation is performed.
        /// </summary>
        /// <remarks>The range of values supported by HINCRBY is limited to 64 bit signed integers.</remarks>
        /// <returns>the value at field after the decrement operation.</returns>
        /// <remarks>http://redis.io/commands/hincrby</remarks>
        Task<long> Decrement(string key, string field, int value = 1);

        /// <summary>
        /// Decrements the number stored at field in the hash stored at key by Decrement. If key does not exist, a new
        /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
        /// integer, the value is set to 0 before the operation is performed.
        /// </summary>
        /// <remarks>The range of values supported by HINCRBY is limited to 64 bit signed integers.</remarks>
        /// <returns>the value at field after the decrement operation.</returns>
        /// <remarks>http://redis.io/commands/hincrby</remarks>
        Task<double> Decrement(string key, string field, double value);

        /// <summary>
        /// Returns all field names in the hash stored at key.
        /// </summary>
        /// <returns>list of fields in the hash, or an empty list when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hkeys</remarks>
        Task<string[]> GetKeys(string key);

        /// <summary>
        /// Returns all values in the hash stored at key.
        /// </summary>
        /// <returns>list of values in the hash, or an empty list when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hvals</remarks>
        Task<byte[][]> GetValues(string key);

        /// <summary>
        /// Returns the number of fields contained in the hash stored at key.
        /// </summary>
        /// <returns>number of fields in the hash, or 0 when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hlen</remarks>
        Task<long> GetLength(string key);

        /// <summary>
        /// Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created.
        /// If field already exists in the hash, it is overwritten.
        /// </summary>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and the value
        /// was updated.
        /// </returns>
        /// <remarks>http://redis.io/commands/hset</remarks>
        Task<bool> Set(string key, string field, string value);

        /// <summary>
        /// Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created.
        /// If field already exists in the hash, it is overwritten.
        /// </summary>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and the value
        /// was updated.
        /// </returns>
        /// <remarks>http://redis.io/commands/hset</remarks>
        Task<bool> Set(string key, string field, byte[] value);

        /// <summary>
        /// Sets the specified fields to their respective values in the hash stored at key. This command overwrites any
        /// existing fields in the hash. If key does not exist, a new key holding a hash is created.
        /// </summary>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and the value
        /// was updated.
        /// </returns>
        /// <remarks>http://redis.io/commands/hmset</remarks>
        Task Set(string key, Dictionary<string, byte[]> values);

        /// <summary>
        /// Sets the specified fields to their respective values in the hash stored at key. This command overwrites any
        /// existing fields in the hash. If key does not exist, a new key holding a hash is created.
        /// </summary>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and the value
        /// was updated.
        /// </returns>
        /// <remarks>http://redis.io/commands/hmset</remarks>
        Task Set(string key, Dictionary<string, string> values);

        /// <summary>
        /// Sets field in the hash stored at key to value, only if field does not yet exist. If key does not exist, a
        /// new key holding a hash is created. If field already exists, this operation has no effect.
        /// </summary>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and no
        /// operation was performed.
        /// </returns>
        /// <remarks>http://redis.io/commands/hsetnx</remarks>
        Task<bool> SetIfNotExists(string key, string field, string value);

        /// <summary>
        /// Sets field in the hash stored at key to value, only if field does not yet exist. If key does not exist, a
        /// new key holding a hash is created. If field already exists, this operation has no effect.
        /// </summary>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and no
        /// operation was performed.
        /// </returns>
        /// <remarks>http://redis.io/commands/hsetnx</remarks>
        Task<bool> SetIfNotExists(string key, string field, byte[] value);
    }
}