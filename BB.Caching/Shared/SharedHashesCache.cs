using System.Threading.Tasks;
using StackExchange.Redis;

namespace BB.Caching.Shared
{
    public partial class SharedCache : IHashes
    {
        /// <summary>
        /// Commands that apply to key/sub-key/value tuples, i.e. where the item is a dictionary of inner values. This
        /// can be useful for modeling members of an entity, for example.
        /// </summary>
        /// <remarks>http://redis.io/commands#hash</remarks>
        public IHashes Hashes
        {
            get { return this; }
        }

        /// <summary>
        /// Removes the specified fields from the hash stored at key. Non-existing fields are ignored. Non-existing keys
        /// are treated as empty hashes and this command returns 0.
        /// </summary>
        /// <remarks>http://redis.io/commands/hdel</remarks>
        /// <returns>The number of fields that were removed.</returns>
        public Task<bool> Remove(RedisKey key, RedisValue field)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.GetDatabase(SharedCache.Instance.Db)
                    .HashDeleteAsync(key, field);
                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Removes the specified fields from the hash stored at key. Non-existing fields are ignored. Non-existing keys
        /// are treated as empty hashes and this command returns 0.
        /// </summary>
        /// <remarks>http://redis.io/commands/hdel</remarks>
        /// <returns>The number of fields that were removed.</returns>
        public Task<long> Remove(RedisKey key, RedisValue[] fields)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<long> result = null;
            foreach (var connection in connections)
            {
                var task = connection.GetDatabase(SharedCache.Instance.Db)
                    .HashDeleteAsync(key, fields);
                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Returns if field is an existing field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// 1 if the hash contains field. 0 if the hash does not contain field, or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hexists</remarks>
        public Task<bool> Exists(RedisKey key, RedisValue field)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.GetDatabase(SharedCache.Instance.Db)
                    .HashExistsAsync(key, field);
                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        public Task<RedisValue> GetString(RedisKey key, RedisValue field)
        {
            Task<RedisValue> result = SharedCache.Instance.GetReadConnection(key)
                .GetDatabase(SharedCache.Instance.Db)
                .HashGetAsync(key, field);

            return result;
        }

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        public Task<RedisValue> GetInt64(RedisKey key, RedisValue field)
        {
            Task<RedisValue> result = SharedCache.Instance.GetReadConnection(key)
                .GetDatabase(SharedCache.Instance.Db)
                .HashGetAsync(key, field);

            return result;
        }

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        public Task<RedisValue> GetDouble(RedisKey key, RedisValue field)
        {
            Task<RedisValue> result = SharedCache.Instance.GetReadConnection(key)
                .GetDatabase(SharedCache.Instance.Db)
                .HashGetAsync(key, field);

            return result;
        }

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        public Task<RedisValue> GetByteArray(RedisKey key, RedisValue field)
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
        /// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
        /// <remarks>http://redis.io/commands/hmget</remarks>
        public Task<RedisValue[]> GetString(RedisKey key, RedisValue[] fields)
        {
            Task<RedisValue[]> result = SharedCache.Instance.GetReadConnection(key)
                .GetDatabase(SharedCache.Instance.Db)
                .HashGetAsync(key, fields);

            return result;
        }

        /// <summary>
        /// Returns the values associated with the specified fields in the hash stored at key. For every field that does
        /// not exist in the hash, a nil value is returned.
        /// </summary>
        /// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
        /// <remarks>http://redis.io/commands/hmget</remarks>
        public Task<RedisValue[]> GetByteArray(RedisKey key, RedisValue[] fields)
        {
            Task<RedisValue[]> result = SharedCache.Instance.GetReadConnection(key)
                .GetDatabase(SharedCache.Instance.Db)
                .HashGetAsync(key, fields);

            return result;
        }

        /// <summary>
        /// Returns all fields and values of the hash stored at key. 
        /// </summary>
        /// <returns>
        /// list of fields and their values stored in the hash, or an empty list when key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hgetall</remarks>
        public Task<HashEntry[]> GetAll(RedisKey key)
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
        /// <remarks>The range of values supported by HINCRBY is limited to 64 bit signed integers.</remarks>
        /// <returns>the value at field after the increment operation.</returns>
        /// <remarks>http://redis.io/commands/hincrby</remarks>
        public Task<long> Increment(RedisKey key, RedisValue field, int value = 1)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<long> result = null;
            foreach (var connection in connections)
            {
                Task<long> task = connection
                    .GetDatabase(SharedCache.Instance.Db)
                    .HashIncrementAsync(key, field, value);


                if (null == result)
                    result = task;
            }

            return result;
        }

        /// <summary>
        /// Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new
        /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
        /// integer, the value is set to 0 before the operation is performed.
        /// </summary>
        /// <remarks>The range of values supported by HINCRBY is limited to 64 bit signed integers.</remarks>
        /// <returns>the value at field after the increment operation.</returns>
        /// <remarks>http://redis.io/commands/hincrby</remarks>
        public Task<double> Increment(RedisKey key, RedisValue field, double value)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<double> result = null;
            foreach (var connection in connections)
            {
                Task<double> task = connection
                    .GetDatabase(SharedCache.Instance.Db)
                    .HashIncrementAsync(key, field, value);

                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Decrements the number stored at field in the hash stored at key by Decrement. If key does not exist, a new
        /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
        /// integer, the value is set to 0 before the operation is performed.
        /// </summary>
        /// <remarks>The range of values supported by HINCRBY is limited to 64 bit signed integers.</remarks>
        /// <returns>the value at field after the decrement operation.</returns>
        /// <remarks>http://redis.io/commands/hincrby</remarks>
        public Task<long> Decrement(RedisKey key, RedisValue field, int value = 1)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<long> result = null;
            foreach (var connection in connections)
            {
                Task<long> task = connection
                    .GetDatabase(SharedCache.Instance.Db)
                    .HashDecrementAsync(key, field, value);

                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Decrements the number stored at field in the hash stored at key by Decrement. If key does not exist, a new
        /// key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as
        /// integer, the value is set to 0 before the operation is performed.
        /// </summary>
        /// <remarks>The range of values supported by HINCRBY is limited to 64 bit signed integers.</remarks>
        /// <returns>the value at field after the decrement operation.</returns>
        /// <remarks>http://redis.io/commands/hincrby</remarks>
        public Task<double> Decrement(RedisKey key, RedisValue field, double value)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<double> result = null;
            foreach (var connection in connections)
            {
                Task<double> task = connection
                    .GetDatabase(SharedCache.Instance.Db)
                    .HashDecrementAsync(key, field, value);

                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Returns all field names in the hash stored at key.
        /// </summary>
        /// <returns>list of fields in the hash, or an empty list when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hkeys</remarks>
        public Task<RedisValue[]> GetKeys(RedisKey key)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<RedisValue[]> result = null;
            foreach (var connection in connections)
            {
                Task<RedisValue[]> task = connection
                    .GetDatabase(SharedCache.Instance.Db)
                    .HashKeysAsync(key);

                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Returns all values in the hash stored at key.
        /// </summary>
        /// <returns>list of values in the hash, or an empty list when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hvals</remarks>
        public Task<RedisValue[]> GetValues(RedisKey key)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<RedisValue[]> result = null;
            foreach (var connection in connections)
            {
                Task<RedisValue[]> task = connection
                    .GetDatabase(SharedCache.Instance.Db)
                    .HashValuesAsync(key);

                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Returns the number of fields contained in the hash stored at key.
        /// </summary>
        /// <returns>number of fields in the hash, or 0 when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hlen</remarks>
        public Task<long> GetLength(RedisKey key)
        {
            return SharedCache.Instance.GetReadConnection(key)
                .GetDatabase(SharedCache.Instance.Db)
                .HashLengthAsync(key);
        }

        /// <summary>
        /// Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created.
        /// If field already exists in the hash, it is overwritten.
        /// </summary>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and the value
        /// was updated.
        /// </returns>
        /// <remarks>http://redis.io/commands/hset</remarks>
        public Task<bool> Set(RedisKey key, RedisValue field, RedisValue value)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                Task<bool> task = connection
                    .GetDatabase(SharedCache.Instance.Db)
                    .HashSetAsync(key, field, value);

                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Sets the specified fields to their respective values in the hash stored at key. This command overwrites any
        /// existing fields in the hash. If key does not exist, a new key holding a hash is created.
        /// </summary>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and the value
        /// was updated.
        /// </returns>
        /// <remarks>http://redis.io/commands/hmset</remarks>
        public Task Set(RedisKey key, HashEntry[] values)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task result = null;

            foreach (var connection in connections)
                result = connection
                    .GetDatabase(SharedCache.Instance.Db)
                    .HashSetAsync(key, values);

            return result;
        }

        /// <summary>
        /// Sets field in the hash stored at key to value, only if field does not yet exist. If key does not exist, a
        /// new key holding a hash is created. If field already exists, this operation has no effect.
        /// </summary>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and no
        /// operation was performed.
        /// </returns>
        /// <remarks>http://redis.io/commands/hsetnx</remarks>
        public Task<bool> SetIfNotExists(RedisKey key, RedisValue field, RedisValue value)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection
                    .GetDatabase(SharedCache.Instance.Db)
                    .HashSetAsync(key, field, value, When.NotExists);

                if (null == result)
                    result = task;
            }
            return result;
        }
    }
}