using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Task<bool> Remove(string key, string field)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .Remove(SharedCache.Instance.Db, key, field, SharedCache.Instance.QueueJump);
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
        public Task<long> Remove(string key, string[] fields)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<long> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .Remove(SharedCache.Instance.Db, key, fields, SharedCache.Instance.QueueJump);
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
        public Task<bool> Exists(string key, string field)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .Exists(SharedCache.Instance.Db, key, field, SharedCache.Instance.QueueJump);
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
        public Wrapper<string, string> GetString(string key, string field)
        {
            var wrapper = new Wrapper<string, string>
                {
                    ValueAsync = SharedCache.Instance.GetReadConnection(key).Hashes
                        .GetString(SharedCache.Instance.Db, key, field, SharedCache.Instance.QueueJump)
                };
            wrapper.IsNilAsync = Task.Run(async () => null == await wrapper.ValueAsync);
            return wrapper;
        }

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        public Wrapper<long, long?> GetInt64(string key, string field)
        {
            var wrapper = new Wrapper<long, long?>
                {
                    TaskResult = SharedCache.Instance.GetReadConnection(key).Hashes
                        .GetInt64(SharedCache.Instance.Db, key, field, SharedCache.Instance.QueueJump)
                };
            wrapper.ValueAsync = Task.Run(async () =>
                {
                    var result = await wrapper.TaskResult;
                    return result.HasValue
                        ? result.Value
                        : 0L;
                });
            wrapper.IsNilAsync = Task.Run(async () => !(await wrapper.TaskResult).HasValue);
            return wrapper;
        }

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        public Wrapper<double, double?> GetDouble(string key, string field)
        {
            var wrapper = new Wrapper<double, double?>
                {
                    TaskResult = Task.Run<double?>(async () =>
                        {
                            string value = await SharedCache.Instance.GetReadConnection(key).Hashes
                                .GetString(SharedCache.Instance.Db, key, field, SharedCache.Instance.QueueJump);

                            if (string.IsNullOrWhiteSpace(value))
                                return null;

                            return double.Parse(value);
                        })
                };
            wrapper.ValueAsync = Task.Run(async () =>
                {
                    var result = await wrapper.TaskResult;
                    return result.HasValue
                        ? result.Value
                        : 0.0d;
                });
            wrapper.IsNilAsync = Task.Run(async () => !(await wrapper.TaskResult).HasValue);
            return wrapper;
        }

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>
        /// the value associated with field, or nil when field is not present in the hash or key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        public Wrapper<byte[], byte[]> GetByteArray(string key, string field)
        {
            var wrapper = new Wrapper<byte[], byte[]>
                {
                    ValueAsync = SharedCache.Instance.GetReadConnection(key).Hashes
                        .Get(SharedCache.Instance.Db, key, field, SharedCache.Instance.QueueJump)
                };
            wrapper.IsNilAsync = Task.Run(async () => null == await wrapper.ValueAsync);
            return wrapper;
        }

        /// <summary>
        /// Returns the values associated with the specified fields in the hash stored at key. For every field that does
        /// not exist in the hash, a nil value is returned.
        /// </summary>
        /// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
        /// <remarks>http://redis.io/commands/hmget</remarks>
        public Wrapper<string[], string[]> GetString(string key, string[] fields)
        {
            var wrappers = new Wrapper<string[], string[]>
                {
                    ValueAsync = SharedCache.Instance.GetReadConnection(key).Hashes
                        .GetString(SharedCache.Instance.Db, key, fields, SharedCache.Instance.QueueJump)
                };
            wrappers.IsNilAsync = Task.Run(async () => null == await wrappers.ValueAsync);
            return wrappers;
        }

        /// <summary>
        /// Returns the values associated with the specified fields in the hash stored at key. For every field that does
        /// not exist in the hash, a nil value is returned.
        /// </summary>
        /// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
        /// <remarks>http://redis.io/commands/hmget</remarks>
        public Wrapper<byte[][], byte[][]> GetByteArray(string key, string[] fields)
        {
            var wrappers = new Wrapper<byte[][], byte[][]>
                {
                    ValueAsync = SharedCache.Instance.GetReadConnection(key).Hashes
                        .Get(SharedCache.Instance.Db, key, fields, SharedCache.Instance.QueueJump)
                };
            wrappers.IsNilAsync = Task.Run(async () => null == await wrappers.ValueAsync);
            return wrappers;
        }

        /// <summary>
        /// Returns all fields and values of the hash stored at key. 
        /// </summary>
        /// <returns>
        /// list of fields and their values stored in the hash, or an empty list when key does not exist.
        /// </returns>
        /// <remarks>http://redis.io/commands/hgetall</remarks>
        public Task<Dictionary<string, byte[]>> GetAll(string key)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<Dictionary<string, byte[]>> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .GetAll(SharedCache.Instance.Db, key, SharedCache.Instance.QueueJump);
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
        public Task<long> Increment(string key, string field, int value = 1)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<long> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .Increment(SharedCache.Instance.Db, key, field, value, SharedCache.Instance.QueueJump);
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
        public Task<double> Increment(string key, string field, double value)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<double> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .Increment(SharedCache.Instance.Db, key, field, value, SharedCache.Instance.QueueJump);
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
        public Task<long> Decrement(string key, string field, int value = 1)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<long> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .Increment(SharedCache.Instance.Db, key, field, -value, SharedCache.Instance.QueueJump);
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
        public Task<double> Decrement(string key, string field, double value)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<double> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .Increment(SharedCache.Instance.Db, key, field, -value, SharedCache.Instance.QueueJump);
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
        public Task<string[]> GetKeys(string key)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<string[]> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .GetKeys(SharedCache.Instance.Db, key, SharedCache.Instance.QueueJump);
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
        public Task<byte[][]> GetValues(string key)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<byte[][]> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .GetValues(SharedCache.Instance.Db, key, SharedCache.Instance.QueueJump);
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
        public Task<long> GetLength(string key)
        {
            return SharedCache.Instance.GetReadConnection(key).Hashes
                .GetLength(SharedCache.Instance.Db, key, SharedCache.Instance.QueueJump);
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
        public Task<bool> Set(string key, string field, string value)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .Set(SharedCache.Instance.Db, key, field, value, SharedCache.Instance.QueueJump);
                if (null == result)
                    result = task;
            }
            return result;
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
        public Task<bool> Set(string key, string field, byte[] value)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .Set(SharedCache.Instance.Db, key, field, value, SharedCache.Instance.QueueJump);
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
        public Task Set(string key, Dictionary<string, byte[]> values)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            foreach (var connection in connections)
                connection.Hashes.Set(SharedCache.Instance.Db, key, values, SharedCache.Instance.QueueJump);

            return Task.FromResult(false);
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
        public Task Set(string key, Dictionary<string, string> values)
        {
            var dict = values.ToDictionary(k => k.Key, v => Encoding.UTF8.GetBytes(v.Value));
            var connections = SharedCache.Instance.GetWriteConnections(key);
            foreach (var connection in connections)
                connection.Hashes.Set(SharedCache.Instance.Db, key, dict, SharedCache.Instance.QueueJump);

            return Task.FromResult(false);
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
        public Task<bool> SetIfNotExists(string key, string field, string value)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .SetIfNotExists(SharedCache.Instance.Db, key, field, value, SharedCache.Instance.QueueJump);
                if (null == result)
                    result = task;
            }
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
        public Task<bool> SetIfNotExists(string key, string field, byte[] value)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Hashes
                    .SetIfNotExists(SharedCache.Instance.Db, key, field, value, SharedCache.Instance.QueueJump);
                if (null == result)
                    result = task;
            }
            return result;
        }
    }
}