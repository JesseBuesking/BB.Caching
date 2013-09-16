using System.Threading.Tasks;

namespace BB.Caching.Shared
{
    /// <summary>
    /// Commands that apply to sets of items per key; sets have no defined order and are strictly unique. Duplicates are
    /// not allowed (typically, duplicates are silently discarded).
    /// </summary>
    /// <remarks>http://redis.io/commands#set</remarks>
    public interface ISets
    {
        /// <summary>
        /// Add member to the set stored at key. If member is already a member of this set, no operation is performed.
        /// If key does not exist, a new set is created with member as its sole member.
        /// </summary>
        /// <returns>true if added</returns>
        /// <remarks>http://redis.io/commands/sadd</remarks>
        Task<bool> Add(string key, string value);

        /// <summary>
        /// Add member to the set stored at key. If member is already a member of this set, no operation is performed.
        /// If key does not exist, a new set is created with member as its sole member.
        /// </summary>
        /// <returns>true if added</returns>
        /// <remarks>http://redis.io/commands/sadd</remarks>
        Task<bool> Add(string key, byte[] value);

        /// <summary>
        /// Add member to the set stored at key. If member is already a member of this set, no operation is performed.
        /// If key does not exist, a new set is created with member as its sole member.
        /// </summary>
        /// <returns>the number of elements actually added to the set.</returns>
        /// <remarks>http://redis.io/commands/sadd</remarks>
        Task<long> Add(string key, string[] values);

        /// <summary>
        /// Add member to the set stored at key. If member is already a member of this set, no operation is performed.
        /// If key does not exist, a new set is created with member as its sole member.
        /// </summary>
        /// <returns>the number of elements actually added to the set.</returns>
        /// <remarks>http://redis.io/commands/sadd</remarks>
        Task<long> Add(string key, byte[][] values);

        /// <summary>
        /// Returns the set cardinality (number of elements) of the set stored at key.
        /// </summary>
        /// <returns>the cardinality (number of elements) of the set, or 0 if key does not exist.</returns>
        /// <remarks>http://redis.io/commands/scard</remarks>
        Task<long> GetLength(string key);

        /// <summary>
        /// Returns the members of the set resulting from the difference between the first set and all the successive
        /// sets.
        /// </summary>
        /// <returns>list with members of the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sdiff</remarks>
        Task<string[]> DifferenceString(string[] keys);

        /// <summary>
        /// Returns the members of the set resulting from the difference between the first set and all the successive
        /// sets.
        /// </summary>
        /// <returns>list with members of the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sdiff</remarks>
        Task<byte[][]> Difference(string[] keys);

        /// <summary>
        /// This command is equal to SDIFF, but instead of returning the resulting set, it is stored in destination.
        /// </summary>
        /// <remarks> If destination already exists, it is overwritten.</remarks>
        /// <returns>the number of elements in the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sdiffstore</remarks>
        Task<long> DifferenceAndStore(string destination, string[] keys);

        /// <summary>
        /// Returns the members of the set resulting from the intersection of all the given sets.
        /// </summary>
        /// <returns>list with members of the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sinter</remarks>
        Task<string[]> IntersectString(string[] keys);

        /// <summary>
        /// Returns the members of the set resulting from the intersection of all the given sets.
        /// </summary>
        /// <returns>list with members of the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sinter</remarks>
        Task<byte[][]> Intersect(string[] keys);

        /// <summary>
        /// This command is equal to SINTER, but instead of returning the resulting set, it is stored in destination.
        /// </summary>
        /// <remarks>If destination already exists, it is overwritten.</remarks>
        /// <returns>the number of elements in the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sinterstore</remarks>
        Task<long> IntersectAndStore(string destination, string[] keys);

        /// <summary>
        /// Returns the members of the set resulting from the union of all the given sets.
        /// </summary>
        /// <returns>list with members of the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sunion</remarks>
        Task<string[]> UnionString(string[] keys);

        /// <summary>
        /// Returns the members of the set resulting from the union of all the given sets.
        /// </summary>
        /// <returns>list with members of the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sunion</remarks>
        Task<byte[][]> Union(string[] keys);

        /// <summary>
        /// This command is equal to SUNION, but instead of returning the resulting set, it is stored in destination.
        /// </summary>
        /// <remarks>If destination already exists, it is overwritten.</remarks>
        /// <returns>the number of elements in the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sunionstore</remarks>
        Task<long> UnionAndStore(string destination, string[] keys);

        /// <summary>
        /// Returns if member is a member of the set stored at key.
        /// </summary>
        /// <returns>1 if the element is a member of the set. 0 if the element is not a member of the set, or if key
        /// does not exist.</returns>
        /// <remarks>http://redis.io/commands/sismember</remarks>
        Task<bool> Contains(string key, string value);

        /// <summary>
        /// Returns if member is a member of the set stored at key.
        /// </summary>
        /// <returns>1 if the element is a member of the set. 0 if the element is not a member of the set, or if key
        /// does not exist.</returns>
        /// <remarks>http://redis.io/commands/sismember</remarks>
        Task<bool> Contains(string key, byte[] value);

        /// <summary>
        /// Returns all the members of the set value stored at key.
        /// </summary>
        /// <returns>all elements of the set.</returns>
        /// <remarks>http://redis.io/commands/smembers</remarks>
        Task<string[]> GetAllString(string key);

        /// <summary>
        /// Returns all the members of the set value stored at key.
        /// </summary>
        /// <returns>all elements of the set.</returns>
        /// <remarks>http://redis.io/commands/smembers</remarks>
        Task<byte[][]> GetAll(string key);

        /// <summary>
        /// Move member from the set at source to the set at destination. This operation is atomic. In every given
        /// moment the element will appear to be a member of source or destination for other clients.
        /// </summary>
        /// <remarks>
        /// If the source set does not exist or does not contain the specified element, no operation is performed and 0
        /// is returned. Otherwise, the element is removed from the source set and added to the destination set. When
        /// the specified element already exists in the destination set, it is only removed from the source set.
        /// </remarks>
        /// <returns>
        /// 1 if the element is moved. 0 if the element is not a member of source and no operation was performed.
        /// </returns>
        /// <remarks>http://redis.io/commands/smove</remarks>
        Task<bool> Move(string source, string destination, string value);

        /// <summary>
        /// Move member from the set at source to the set at destination. This operation is atomic. In every given
        /// moment the element will appear to be a member of source or destination for other clients.
        /// </summary>
        /// <remarks>
        /// If the source set does not exist or does not contain the specified element, no operation is performed and 0
        /// is returned. Otherwise, the element is removed from the source set and added to the destination set. When
        /// the specified element already exists in the destination set, it is only removed from the source set.
        /// </remarks>
        /// <returns>
        /// 1 if the element is moved. 0 if the element is not a member of source and no operation was performed.
        /// </returns>
        /// <remarks>http://redis.io/commands/smove</remarks>
        Task<bool> Move(string source, string destination, byte[] value);

        /// <summary>
        /// Removes and returns a random element from the set value stored at key.
        /// </summary>
        /// <returns>the removed element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/spop</remarks>
        Task<string> RemoveRandomString(string key);

        /// <summary>
        /// Removes and returns a random element from the set value stored at key.
        /// </summary>
        /// <returns>the removed element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/spop</remarks>
        Task<byte[]> RemoveRandom(string key);

        /// <summary>
        /// Return a random element from the set value stored at key.
        /// </summary>
        /// <returns>the randomly selected element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/srandmember</remarks>
        Task<string> GetRandomString(string key);

        /// <summary>
        /// Return a random element from the set value stored at key.
        /// </summary>
        /// <returns>the randomly selected element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/srandmember</remarks>
        Task<byte[]> GetRandom(string key);

        /// <summary>
        /// Return an array of count distinct elements if count is positive. If called with a negative count the
        /// behavior changes and the command is allowed to return the same element multiple times. In this case the
        /// number of returned elements is the absolute value of the specified count.
        /// </summary>
        /// <returns>the randomly selected element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/srandmember</remarks>
        Task<string[]> GetRandomString(string key, int count);

        /// <summary>
        /// Return an array of count distinct elements if count is positive. If called with a negative count the
        /// behavior changes and the command is allowed to return the same element multiple times. In this case the
        /// number of returned elements is the absolute value of the specified count.
        /// </summary>
        /// <returns>the randomly selected element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/srandmember</remarks>
        Task<byte[][]> GetRandom(string key, int count);

        /// <summary>
        /// Remove member from the set stored at key. If member is not a member of this set, no operation is performed.
        /// </summary>
        /// <returns>1 if the element was removed. 0 if the element was not a member of the set.</returns>
        /// <remarks>http://redis.io/commands/srem</remarks>
        Task<bool> Remove(string key, string value);

        /// <summary>
        /// Remove member from the set stored at key. If member is not a member of this set, no operation is performed.
        /// </summary>
        /// <returns>1 if the element was removed. 0 if the element was not a member of the set.</returns>
        /// <remarks>http://redis.io/commands/srem</remarks>
        Task<bool> Remove(string key, byte[] value);

        /// <summary>
        /// Remove member from the set stored at key. If member is not a member of this set, no operation is performed.
        /// </summary>
        /// <returns>1 if the element was removed. 0 if the element was not a member of the set.</returns>
        /// <remarks>http://redis.io/commands/srem</remarks>
        Task<long> Remove(string key, string[] values);

        /// <summary>
        /// Remove member from the set stored at key. If member is not a member of this set, no operation is performed.
        /// </summary>
        /// <returns>1 if the element was removed. 0 if the element was not a member of the set.</returns>
        /// <remarks>http://redis.io/commands/srem</remarks>
        Task<long> Remove(string key, byte[][] values);
    }
}