using System.Collections.Generic;
using System.Threading.Tasks;

namespace BB.Caching.Caching.Shared
{
    /// <summary>
    /// Commands that apply to sorted sets per key. A sorted set keeps a "score" per element, and this score is used to
    /// order the elements. Duplicates are not allowed (typically, the score of the duplicate is added to the
    /// pre-existing element instead).
    /// </summary>
    /// <remarks>http://redis.io/commands#sorted_set</remarks>
    public interface ISortedSets
    {
        /// <summary>
        /// Adds all the specified members with the specified scores to the sorted set stored at key. It is possible to
        /// specify multiple score/member pairs. If a specified member is already a member of the sorted set, the score
        /// is updated and the element reinserted at the right position to ensure the correct ordering. If key does not
        /// exist, a new sorted set with the specified members as sole members is created, like if the sorted set was
        /// empty. If the key exists but does not hold a sorted set, an error is returned.
        /// <para>
        /// The score values should be the string representation of a numeric value, and accepts double precision
        /// floating point numbers.
        /// </para>
        /// </summary>
        /// <returns>
        /// The number of elements added to the sorted sets, not including elements already existing for which the score
        /// was updated.
        /// </returns>
        /// <remarks>http://redis.io/commands/zadd</remarks>
        Task<bool> Add(string key, string value, double score);

        /// <summary>
        /// Adds all the specified members with the specified scores to the sorted set stored at key. It is possible to
        /// specify multiple score/member pairs. If a specified member is already a member of the sorted set, the score
        /// is updated and the element reinserted at the right position to ensure the correct ordering. If key does not
        /// exist, a new sorted set with the specified members as sole members is created, like if the sorted set was
        /// empty. If the key exists but does not hold a sorted set, an error is returned.
        /// <para>
        /// The score values should be the string representation of a numeric value, and accepts double precision
        /// floating point numbers.
        /// </para>
        /// </summary>
        /// <returns>
        /// The number of elements added to the sorted sets, not including elements already existing for which the score
        /// was updated.
        /// </returns>
        /// <remarks>http://redis.io/commands/zadd</remarks>
        Task<bool> Add(string key, byte[] value, double score);

        /// <summary>
        /// Returns the sorted set cardinality (number of elements) of the sorted set stored at key.
        /// </summary>
        /// <returns>the cardinality (number of elements) of the sorted set, or 0 if key does not exist.</returns>
        /// <remarks>http://redis.io/commands/zcard</remarks>
        Task<long> GetLength(string key);

        /// <summary>
        /// Returns the number of elements in the sorted set at key with a score between min and max.
        /// The min and max arguments have the same semantic as described for ZRANGEBYSCORE.
        /// </summary>
        /// <returns>the number of elements in the specified score range.</returns>
        /// <remarks>http://redis.io/commands/zcount</remarks>
        Task<long> GetLength(string key, double min, double max);

        /// <summary>
        /// Increments the score of member in the sorted set stored at key by increment. If member does not exist in the
        /// sorted set, it is added with increment as its score (as if its previous score was 0.0). If key does not
        /// exist, a new sorted set with the specified member as its sole member is created.
        /// <para>
        /// An error is returned when key exists but does not hold a sorted set.
        /// </para>
        /// <para>
        /// The score value should be the string representation of a numeric value, and accepts double precision
        /// floating point numbers. It is possible to provide a negative value to decrement the score.
        /// </para>
        /// </summary>
        /// <remarks>http://redis.io/commands/zincrby</remarks>
        /// <returns>
        /// the new score of member (a double precision floating point number), represented as string.
        /// </returns>
        Task<double> Increment(string key, string member, double delta);

        /// <summary>
        /// Increments the score of member in the sorted set stored at key by increment. If member does not exist in the
        /// sorted set, it is added with increment as its score (as if its previous score was 0.0). If key does not
        /// exist, a new sorted set with the specified member as its sole member is created.
        /// <para>
        /// An error is returned when key exists but does not hold a sorted set.
        /// </para>
        /// <para>
        /// The score value should be the string representation of a numeric value, and accepts double precision
        /// floating point numbers. It is possible to provide a negative value to decrement the score.
        /// </para>
        /// </summary>
        /// <remarks>http://redis.io/commands/zincrby</remarks>
        /// <returns>
        /// the new score of member (a double precision floating point number), represented as string.
        /// </returns>
        Task<double> Increment(string key, byte[] member, double delta);

        /// <summary>
        /// Increments the score of member in the sorted set stored at key by increment. If member does not exist in the
        /// sorted set, it is added with increment as its score (as if its previous score was 0.0). If key does not
        /// exist, a new sorted set with the specified member as its sole member is created.
        /// <para>
        /// An error is returned when key exists but does not hold a sorted set.
        /// </para>
        /// <para>
        /// The score value should be the string representation of a numeric value, and accepts double precision
        /// floating point numbers. It is possible to provide a negative value to decrement the score.
        /// </para>
        /// </summary>
        /// <remarks>http://redis.io/commands/zincrby</remarks>
        /// <returns>
        /// the new score of member (a double precision floating point number), represented as string.
        /// </returns>
        Task<double>[] Increment(string key, string[] members, double delta);

        /// <summary>
        /// Increments the score of member in the sorted set stored at key by increment. If member does not exist in the
        /// sorted set, it is added with increment as its score (as if its previous score was 0.0). If key does not
        /// exist, a new sorted set with the specified member as its sole member is created.
        /// <para>
        /// An error is returned when key exists but does not hold a sorted set.
        /// </para>
        /// <para>
        /// The score value should be the string representation of a numeric value, and accepts double precision
        /// floating point numbers. It is possible to provide a negative value to decrement the score.
        /// </para>
        /// </summary>
        /// <remarks>http://redis.io/commands/zincrby</remarks>
        /// <returns>
        /// the new score of member (a double precision floating point number), represented as string.
        /// </returns>
        Task<double>[] Increment(string key, byte[][] members, double delta);

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key. The elements are considered to be
        /// ordered from the lowest to the highest score. Lexicographical order is used for elements with equal score.
        /// <para>
        /// See ZREVRANGE when you need the elements ordered from highest to lowest score (and descending
        /// lexicographical order for elements with equal score).
        /// </para>
        /// <para>
        /// Both start and stop are zero-based indexes, where 0 is the first element, 1 is the next element and so on.
        /// They can also be negative numbers indicating offsets from the end of the sorted set, with -1 being the last
        /// element of the sorted set, -2 the penultimate element and so on.
        /// </para>
        /// </summary>
        /// <remarks>http://redis.io/commands/zrange</remarks>
        /// <remarks>http://redis.io/commands/zrevrange</remarks>
        /// <returns>list of elements in the specified range (optionally with their scores).</returns>
        Task<KeyValuePair<byte[], double>[]> Range(string key, long start, long stop, bool ascending = true);

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key. The elements are considered to be
        /// ordered from the lowest to the highest score. Lexicographical order is used for elements with equal score.
        /// <para>
        /// See ZREVRANGE when you need the elements ordered from highest to lowest score (and descending
        /// lexicographical order for elements with equal score).
        /// </para>
        /// <para>
        /// Both start and stop are zero-based indexes, where 0 is the first element, 1 is the next element and so on.
        /// They can also be negative numbers indicating offsets from the end of the sorted set, with -1 being the last
        /// element of the sorted set, -2 the penultimate element and so on.
        /// </para>
        /// </summary>
        /// <remarks>http://redis.io/commands/zrange</remarks>
        /// <remarks>http://redis.io/commands/zrevrange</remarks>
        /// <returns>list of elements in the specified range (optionally with their scores).</returns>
        Task<KeyValuePair<string, double>[]> RangeString(string key, long start, long stop, bool ascending = true);

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key. The elements are considered to be
        /// ordered from the lowest to the highest score. Lexicographical order is used for elements with equal score.
        /// <para>
        /// See ZREVRANGE when you need the elements ordered from highest to lowest score (and descending
        /// lexicographical order for elements with equal score).
        /// </para>
        /// <para>
        /// Both start and stop are zero-based indexes, where 0 is the first element, 1 is the next element and so on.
        /// They can also be negative numbers indicating offsets from the end of the sorted set, with -1 being the last
        /// element of the sorted set, -2 the penultimate element and so on.
        /// </para>
        /// </summary>
        /// <remarks>http://redis.io/commands/zrange</remarks>
        /// <remarks>http://redis.io/commands/zrevrange</remarks>
        /// <returns>list of elements in the specified range (optionally with their scores).</returns>
        Task<KeyValuePair<string, double>[]> RangeString(string key,
            double min = double.NegativeInfinity, double max = double.PositiveInfinity,
            bool ascending = true,
            bool minInclusive = true, bool maxInclusive = true,
            long offset = 0, long count = long.MaxValue);

        /// <summary>
        /// Returns all the elements in the sorted set at key with a score between min and max (including elements with
        /// score equal to min or max). The elements are considered to be ordered from low to high scores.
        /// <para>
        /// The elements having the same score are returned in lexicographical order (this follows from a property of
        /// the sorted set implementation in Redis and does not involve further computation).
        /// </para>
        /// <para>
        /// The optional LIMIT argument can be used to only get a range of the matching elements (similar to SELECT
        /// LIMIT offset, count in SQL). Keep in mind that if offset is large, the sorted set needs to be traversed for
        /// offset elements before getting to the elements to return, which can add up to O(N) time complexity.
        /// </para>
        /// </summary>
        /// <remarks>http://redis.io/commands/zrangebyscore</remarks>
        /// <remarks>http://redis.io/commands/zrevrangebyscore</remarks>
        /// <returns>list of elements in the specified score range (optionally with their scores).</returns>
        Task<KeyValuePair<byte[], double>[]> Range(string key,
            double min = double.NegativeInfinity, double max = double.PositiveInfinity,
            bool ascending = true,
            bool minInclusive = true, bool maxInclusive = true,
            long offset = 0, long count = long.MaxValue);

        /// <summary>
        /// Returns the rank of member in the sorted set stored at key, with the scores ordered from low to high. The
        /// rank (or index) is 0-based, which means that the member with the lowest score has rank 0.
        /// </summary>
        /// <remarks>http://redis.io/commands/zrank</remarks>
        /// <remarks>http://redis.io/commands/zrevrank</remarks>
        /// <returns>
        /// If member exists in the sorted set, Integer reply: the rank of member. If member does not exist in the
        /// sorted set or key does not exist, Bulk reply: nil.
        /// </returns>
        Task<long?> Rank(string key, string member, bool ascending = true);

        /// <summary>
        /// Returns the rank of member in the sorted set stored at key, with the scores ordered from low to high. The
        /// rank (or index) is 0-based, which means that the member with the lowest score has rank 0.
        /// </summary>
        /// <remarks>http://redis.io/commands/zrank</remarks>
        /// <remarks>http://redis.io/commands/zrevrank</remarks>
        /// <returns>
        /// If member exists in the sorted set, Integer reply: the rank of member. If member does not exist in the
        /// sorted set or key does not exist, Bulk reply: nil.
        /// </returns>
        Task<long?> Rank(string key, byte[] member, bool ascending = true);

        /// <summary>
        /// Returns the score of member in the sorted set at key. If member does not exist in the sorted set, or key
        /// does not exist, nil is returned.
        /// </summary>
        /// <remarks>http://redis.io/commands/zscore</remarks>
        /// <returns>the score of member (a double precision floating point number), represented as string.</returns>
        Task<double?> Score(string key, string member);

        /// <summary>
        /// Returns the score of member in the sorted set at key. If member does not exist in the sorted set, or key
        /// does not exist, nil is returned.
        /// </summary>
        /// <remarks>http://redis.io/commands/zscore</remarks>
        /// <returns>the score of member (a double precision floating point number), represented as string.</returns>
        Task<double?> Score(string key, byte[] member);

        /// <summary>
        /// Removes the specified members from the sorted set stored at key. Non existing members are ignored.
        /// An error is returned when key exists and does not hold a sorted set.
        /// </summary>
        /// <remarks>http://redis.io/commands/zrem</remarks>
        /// <returns>The number of members removed from the sorted set, not including non existing members.</returns>
        Task<bool> Remove(string key, string member);

        /// <summary>
        /// Removes the specified members from the sorted set stored at key. Non existing members are ignored.
        /// An error is returned when key exists and does not hold a sorted set.
        /// </summary>
        /// <remarks>http://redis.io/commands/zrem</remarks>
        /// <returns>The number of members removed from the sorted set, not including non existing members.</returns>
        Task<bool> Remove(string key, byte[] member);

        /// <summary>
        /// Removes the specified members from the sorted set stored at key. Non existing members are ignored.
        /// An error is returned when key exists and does not hold a sorted set.
        /// </summary>
        /// <remarks>http://redis.io/commands/zrem</remarks>
        /// <returns>The number of members removed from the sorted set, not including non existing members.</returns>
        Task<long> Remove(string key, string[] members);

        /// <summary>
        /// Removes the specified members from the sorted set stored at key. Non existing members are ignored.
        /// An error is returned when key exists and does not hold a sorted set.
        /// </summary>
        /// <remarks>http://redis.io/commands/zrem</remarks>
        /// <returns>The number of members removed from the sorted set, not including non existing members.</returns>
        Task<long> Remove(string key, byte[][] members);

        /// <summary>
        /// Removes all elements in the sorted set stored at key with rank between start and stop. Both start and stop
        /// are 0-based indexes with 0 being the element with the lowest score. These indexes can be negative numbers,
        /// where they indicate offsets starting at the element with the highest score. For example: -1 is the element
        /// with the highest score, -2 the element with the second highest score and so forth.
        /// </summary>
        /// <remarks>http://redis.io/commands/zremrangebyrank</remarks>
        /// <returns>the number of elements removed.</returns>
        Task<long> RemoveRange(string key, long start, long stop);

        /// <summary>
        /// Removes all elements in the sorted set stored at key with a score between min and max (inclusive).
        /// </summary>
        /// <remarks>http://redis.io/commands/zremrangebyscore</remarks>
        /// <returns>the number of elements removed.</returns>
        /// <remarks>Since version 2.1.6, min and max can be exclusive, following the syntax of ZRANGEBYSCORE.</remarks>
        Task<long> RemoveRange(string key, double min, double max, bool minInclusive = true, bool maxInclusive = true);

        /// <summary>
        /// Computes the intersection of numkeys sorted sets given by the specified keys, and stores the result in
        /// destination.
        /// </summary>
        /// <remarks>http://redis.io/commands/zinterstore</remarks>
        /// <returns>the number of elements in the resulting set.</returns>
        Task<long> IntersectAndStore(string destination, string[] keys, CacheAggregate aggregate = CacheAggregate.Sum);

        /// <summary>
        /// Computes the union of numkeys sorted sets given by the specified keys, and stores the result in destination.
        /// It is mandatory to provide the number of input keys (numkeys) before passing the input keys and the other
        /// (optional) arguments.
        /// </summary>
        /// <remarks>http://redis.io/commands/zunionstore</remarks>
        /// <returns>the number of elements in the resulting set.</returns>
        Task<long> UnionAndStore(string destination, string[] keys, CacheAggregate aggregate = CacheAggregate.Sum);
    }
}