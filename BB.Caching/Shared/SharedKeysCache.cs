using System;
using System.Linq;
using System.Threading.Tasks;

namespace BB.Caching.Shared
{
    public partial class SharedCache : IKeys
    {
        /// <summary>
        /// Generic commands that apply to all/most data structures
        /// </summary>
        /// <remarks>http://redis.io/commands#generic</remarks>
        public IKeys Keys
        {
            get { return this; }
        }

        /// <summary>
        /// Removes the specified key. A key is ignored if it does not exist.
        /// </summary>
        /// <returns>True if the key was removed.</returns>
        /// <remarks>http://redis.io/commands/del</remarks>
        Task<bool> IKeys.Remove(string key)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Keys.Remove(SharedCache.Instance.Db, key, SharedCache.Instance.QueueJump);
                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Removes the specified keys. A key is ignored if it does not exist.
        /// </summary>
        /// <returns>The number of keys that were removed.</returns>
        /// <remarks>http://redis.io/commands/del</remarks>
        async Task<long> IKeys.Remove(string[] keys)
        {
            var dictionary = SharedCache.Instance.GetWriteConnections(keys);
            var tasks = new Task<long>[dictionary.Count];
            for (int i = 0; i < dictionary.Count; i++)
            {
                foreach (var connection in dictionary.ElementAt(i).Key)
                {
                    var task = connection.Keys.Remove(
                        SharedCache.Instance.Db, dictionary.ElementAt(i).Value, SharedCache.Instance.QueueJump);
                    if (null == tasks[i])
                        tasks[i] = task;
                }
            }
            var results = await Task.WhenAll(tasks);
            return results.Sum();
        }

        /// <summary>
        /// Returns if key exists.
        /// </summary>
        /// <returns>1 if the key exists. 0 if the key does not exist.</returns>
        /// <remarks>http://redis.io/commands/exists</remarks>
        Task<bool> IKeys.Exists(string key)
        {
            return SharedCache.Instance.GetReadConnection(key).Keys
                .Exists(SharedCache.Instance.Db, key, SharedCache.Instance.QueueJump);
        }

        /// <summary>
        /// Set a timeout on key. After the timeout has expired, the key will automatically be deleted. A key with an
        /// associated timeout is said to be volatile in Redis terminology.
        /// </summary>
        /// <remarks>
        /// If key is updated before the timeout has expired, then the timeout is removed as if the PERSIST
        /// command was invoked on key.
        /// For Redis versions &lt; 2.1.3, existing timeouts cannot be overwritten. So, if key already has an associated
        /// timeout, it will do nothing and return 0. Since Redis 2.1.3, you can update the timeout of a key. It is also
        /// possible to remove the timeout using the PERSIST command. See the page on key expiry for more information.
        /// </remarks>
        /// <returns>1 if the timeout was set. 0 if key does not exist or the timeout could not be set.</returns>
        /// <remarks>http://redis.io/commands/expire</remarks>
        Task<bool> IKeys.Expire(string key, TimeSpan expiry)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Keys
                    .Expire(SharedCache.Instance.Db, key, (int) expiry.TotalSeconds, SharedCache.Instance.QueueJump);
                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Remove the existing timeout on key.
        /// </summary>
        /// <returns>
        /// 1 if the timeout was removed. 0 if key does not exist or does not have an associated timeout.
        /// </returns>
        /// <remarks>Available with 2.1.2 and above only</remarks>
        /// <remarks>http://redis.io/commands/persist</remarks>
        Task<bool> IKeys.Persist(string key)
        {
            var connections = SharedCache.Instance.GetWriteConnections(key);
            Task<bool> result = null;
            foreach (var connection in connections)
            {
                var task = connection.Keys.Persist(SharedCache.Instance.Db, key, SharedCache.Instance.QueueJump);
                if (null == result)
                    result = task;
            }
            return result;
        }

        /// <summary>
        /// Returns all keys matching pattern.
        /// </summary>
        /// <remarks>
        /// Warning: consider KEYS as a command that should only be used in production environments with
        /// extreme care. It may ruin performance when it is executed against large databases. This command is intended
        /// for debugging and special operations, such as changing your keyspace layout. Don't use KEYS in your regular
        /// application code. If you're looking for a way to find keys in a subset of your keyspace, consider using
        /// sets.
        /// </remarks>
        /// <remarks>http://redis.io/commands/keys</remarks>
        async Task<string[]> IKeys.Find(string pattern)
        {
            var connections = SharedCache.Instance.GetAllReadConnections();
            var tasks = new Task<string[]>[connections.Length];
            for (int i = 0; i < connections.Length; i++)
                tasks[i] = connections[i].Keys.Find(SharedCache.Instance.Db, pattern, SharedCache.Instance.QueueJump);
            var results = await Task.WhenAll(tasks);
            return results.SelectMany(s => s).ToArray();
        }

        /// <summary>
        /// Return a random key from the currently selected database.
        /// </summary>
        /// <returns>the random key, or nil when the database is empty.</returns>
        /// <remarks>http://redis.io/commands/randomkey</remarks>
        Task<string> IKeys.Random()
        {
            return SharedCache.Instance.GetRandomReadConnection().Keys
                .Random(SharedCache.Instance.Db, SharedCache.Instance.QueueJump);
        }

        Task IKeys.Rename(string fromKey, string toKey)
        {
            throw new NotImplementedException();
        }

        Task<bool> IKeys.RenameIfNotExists(string fromKey, string toKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the remaining time to live (seconds) of a key that has a timeout.  This introspection capability
        /// allows a Redis client to check how many seconds a given key will continue to be part of the dataset.
        /// </summary>
        /// <returns>TTL in seconds or -1 when key does not exist or does not have a timeout.</returns>
        /// <remarks>http://redis.io/commands/ttl</remarks>
        Task<long> IKeys.TimeToLive(string key)
        {
            return SharedCache.Instance.GetReadConnection(key).Keys
                .TimeToLive(SharedCache.Instance.Db, key, SharedCache.Instance.QueueJump);
        }

        /// <summary>
        /// Returns the string representation of the type of the value stored at key. The different types that can be
        /// returned are: string, list, set, zset and hash.
        /// </summary>
        /// <returns> type of key, or none when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/type</remarks>
        Task<string> IKeys.Type(string key)
        {
            return SharedCache.Instance.GetReadConnection(key).Keys
                .Type(SharedCache.Instance.Db, key, SharedCache.Instance.QueueJump);
        }

        /// <summary>
        /// Return the number of keys in the currently selected database.
        /// </summary>
        /// <remarks>http://redis.io/commands/dbsize</remarks>
        async Task<long> IKeys.GetLength()
        {
            var connections = SharedCache.Instance.GetAllReadConnections();
            var tasks = new Task<long>[connections.Length];
            for (int i = 0; i < connections.Length; i++)
                tasks[i] = connections[i].Keys.GetLength(SharedCache.Instance.Db, SharedCache.Instance.QueueJump);
            var results = await Task.WhenAll(tasks);
            return results.Sum();
        }

#pragma warning disable 1066
        Task<string[]> IKeys.SortString(string key, string byPattern = null, string[] getPattern = null,
            long offset = 0, long count = -1, bool alpha = false, bool @ascending = true)
#pragma warning restore 1066
        {
//            return SharedCache.Instance.GetConnection(key).Keys
//                .SortString(SharedCache.Instance.Db, key, byPattern, getPattern, offset, count, alpha, @ascending,
//                    SharedCache.Instance.QueueJump);
            throw new NotImplementedException();
        }

#pragma warning disable 1066
        Task<long> IKeys.SortAndStore(string destination, string key, string byPattern = null,
            string[] getPattern = null, long offset = 0, long count = -1, bool alpha = false, bool @ascending = true)
#pragma warning restore 1066
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the raw DEBUG OBJECT output for a key; this command is not fully documented and should be avoided
        /// unless you have good reason, and then avoided anyway.
        /// </summary>
        /// <remarks>http://redis.io/commands/debug-object</remarks>
        Task<string> IKeys.DebugObject(string key)
        {
            return SharedCache.Instance.GetReadConnection(key).Keys.DebugObject(SharedCache.Instance.Db, key);
        }

        /// <summary>
        /// Invalidates the object stored at the key's location in Cache.Memory.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<long> IKeys.Invalidate(string key)
        {
            Cache.Memory.Remove(key);
            SharedCache.Instance._alreadyInvalidated.Add(key);
            return SharedCache.Instance.RedisChannelPublish(SharedCache._cacheInvalidationChannel, key);
        }

        /// <summary>
        /// Invalidates the objects stored at the keys located in Cache.Memory.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>The number of clients that received the message.</returns>
        Task<long> IKeys.Invalidate(string[] keys)
        {
            foreach (string key in keys)
            {
                Cache.Memory.Remove(key);
                SharedCache.Instance._alreadyInvalidated.Add(key);
            }

            string multipleKeys = String.Join(SharedCache._cacheMultipleInvalidationSeparator, keys);
            return SharedCache.Instance.RedisChannelPublish(SharedCache._cacheMultipleInvalidationChannel, multipleKeys);
        }
    }
}