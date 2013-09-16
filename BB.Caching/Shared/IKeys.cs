using System;
using System.Threading.Tasks;

namespace BB.Caching.Shared
{
    /// <summary>
    /// Generic commands that apply to all/most data structures
    /// </summary>
    /// <remarks>http://redis.io/commands#generic</remarks>
    public interface IKeys
    {
        /// <summary>
        /// Removes the specified key. A key is ignored if it does not exist.
        /// </summary>
        /// <returns>True if the key was removed.</returns>
        /// <remarks>http://redis.io/commands/del</remarks>
        Task<bool> Remove(string key);

        /// <summary>
        /// Removes the specified keys. A key is ignored if it does not exist.
        /// </summary>
        /// <returns>The number of keys that were removed.</returns>
        /// <remarks>http://redis.io/commands/del</remarks>
        Task<long> Remove(string[] keys);

        /// <summary>
        /// Returns if key exists.
        /// </summary>
        /// <returns>1 if the key exists. 0 if the key does not exist.</returns>
        /// <remarks>http://redis.io/commands/exists</remarks>
        Task<bool> Exists(string key);

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
        Task<bool> Expire(string key, TimeSpan expiry);

        /// <summary>
        /// Remove the existing timeout on key.
        /// </summary>
        /// <returns>
        /// 1 if the timeout was removed. 0 if key does not exist or does not have an associated timeout.
        /// </returns>
        /// <remarks>Available with 2.1.2 and above only</remarks>
        /// <remarks>http://redis.io/commands/persist</remarks>
        Task<bool> Persist(string key);

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
        Task<string[]> Find(string pattern);

        /// <summary>
        /// Return a random key from the currently selected database.
        /// </summary>
        /// <returns>the random key, or nil when the database is empty.</returns>
        /// <remarks>http://redis.io/commands/randomkey</remarks>
        Task<string> Random();

        /// <summary>
        /// Renames key to newkey. It returns an error when the source and destination names are the same, or when key
        /// does not exist. If newkey already exists it is overwritten.
        /// </summary>
        /// <remarks>http://redis.io/commands/rename</remarks>
        Task Rename(string fromKey, string toKey);

        /// <summary>
        /// Renames key to newkey if newkey does not yet exist. It returns an error under the same conditions as RENAME.
        /// </summary>
        /// <returns>1 if key was renamed to newkey. 0 if newkey already exists.</returns>
        /// <remarks>http://redis.io/commands/renamenx</remarks>
        Task<bool> RenameIfNotExists(string fromKey, string toKey);

        /// <summary>
        /// Returns the remaining time to live (seconds) of a key that has a timeout.  This introspection capability
        /// allows a Redis client to check how many seconds a given key will continue to be part of the dataset.
        /// </summary>
        /// <returns>TTL in seconds or -1 when key does not exist or does not have a timeout.</returns>
        /// <remarks>http://redis.io/commands/ttl</remarks>
        Task<long> TimeToLive(string key);

        /// <summary>
        /// Returns the string representation of the type of the value stored at key. The different types that can be
        /// returned are: string, list, set, zset and hash.
        /// </summary>
        /// <returns> type of key, or none when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/type</remarks>
        Task<string> Type(string key);

        /// <summary>
        /// Return the number of keys in the currently selected database.
        /// </summary>
        /// <remarks>http://redis.io/commands/dbsize</remarks>
        Task<long> GetLength();

        /// <summary>
        /// Returns or stores the elements contained in the list, set or sorted set at key. By default, sorting is
        /// numeric and elements are compared by their value interpreted as double precision floating point number. 
        /// </summary>
        /// <remarks>http://redis.io/commands/sort</remarks>
        Task<string[]> SortString(string key, string byPattern = null, string[] getPattern = null,
            long offset = 0, long count = -1, bool alpha = false, bool ascending = true);

        /// <summary>
        /// Returns or stores the elements contained in the list, set or sorted set at key. By default, sorting is
        /// numeric and elements are compared by their value interpreted as double precision floating point number. 
        /// </summary>
        /// <remarks>http://redis.io/commands/sort</remarks>
        Task<long> SortAndStore(string destination, string key, string byPattern = null, string[] getPattern = null,
            long offset = 0, long count = -1, bool alpha = false, bool ascending = true);

        /// <summary>
        /// Returns the raw DEBUG OBJECT output for a key; this command is not fully documented and should be avoided
        /// unless you have good reason, and then avoided anyway.
        /// </summary>
        /// <remarks>http://redis.io/commands/debug-object</remarks>
        Task<string> DebugObject(string key);

        /// <summary>
        /// Invalidates the object stored at the key's location in Cache.Memory.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The number of clients that received the message.</returns>
        Task<long> Invalidate(string key);

        /// <summary>
        /// Invalidates the objects stored at the keys located in Cache.Memory.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>The number of clients that received the message.</returns>
        Task<long> Invalidate(string[] keys);
    }
}