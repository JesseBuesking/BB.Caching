using System;
using System.Threading.Tasks;

namespace BB.Caching.Shared
{
    /// <summary>
    /// Commands that apply to basic lists of items per key; lists preserve insertion order and have no enforced
    /// uniqueness (duplicates are allowed)
    /// </summary>
    /// <remarks>http://redis.io/commands#list</remarks>
    public interface ILists
    {
        /// <summary>
        /// Inserts value in the list stored at key either before or after the reference value pivot.
        /// </summary>
        /// <remarks>When key does not exist, it is considered an empty list and no operation is performed.</remarks>
        /// <returns>
        /// the length of the list after the insert operation, or -1 when the value pivot was not found.
        /// </returns>
        /// <remarks>http://redis.io/commands/linsert</remarks>
        Task<long> InsertBefore(string key, byte[] pivot, byte[] value);

        /// <summary>
        /// Inserts value in the list stored at key either before or after the reference value pivot.
        /// </summary>
        /// <remarks>When key does not exist, it is considered an empty list and no operation is performed.</remarks>
        /// <returns>
        /// the length of the list after the insert operation, or -1 when the value pivot was not found.
        /// </returns>
        /// <remarks>http://redis.io/commands/linsert</remarks>
        Task<long> InsertBefore(string key, string pivot, string value);

        /// <summary>
        /// Inserts value in the list stored at key either before or after the reference value pivot.
        /// </summary>
        /// <remarks>When key does not exist, it is considered an empty list and no operation is performed.</remarks>
        /// <returns>
        /// the length of the list after the insert operation, or -1 when the value pivot was not found.
        /// </returns>
        /// <remarks>http://redis.io/commands/linsert</remarks>
        Task<long> InsertAfter(string key, byte[] pivot, byte[] value);

        /// <summary>
        /// Inserts value in the list stored at key either before or after the reference value pivot.
        /// </summary>
        /// <remarks>When key does not exist, it is considered an empty list and no operation is performed.</remarks>
        /// <returns>
        /// the length of the list after the insert operation, or -1 when the value pivot was not found.
        /// </returns>
        /// <remarks>http://redis.io/commands/linsert</remarks>
        Task<long> InsertAfter(string key, string pivot, string value);

        /// <summary>
        /// Returns the element at index index in the list stored at key. The index is zero-based, so 0 means the first
        /// element, 1 the second element and so on. Negative indices can be used to designate elements starting at the
        /// tail of the list. Here, -1 means the last element, -2 means the penultimate and so forth.
        /// </summary>
        /// <returns>the requested element, or nil when index is out of range.</returns>
        /// <remarks>http://redis.io/commands/lindex</remarks>
        Task<byte[]> Get(string key, int index);

        /// <summary>
        /// Returns the element at index index in the list stored at key. The index is zero-based, so 0 means the first
        /// element, 1 the second element and so on. Negative indices can be used to designate elements starting at the
        /// tail of the list. Here, -1 means the last element, -2 means the penultimate and so forth.
        /// </summary>
        /// <returns>the requested element, or nil when index is out of range.</returns>
        /// <remarks>http://redis.io/commands/lindex</remarks>
        Task<string> GetString(string key, int index);

        /// <summary>
        /// Sets the list element at index to value. For more information on the index argument, see LINDEX.
        /// </summary>
        /// <remarks>An error is returned for out of range indexes.</remarks>
        /// <remarks>http://redis.io/commands/lset</remarks>
        Task Set(string key, int index, string value);

        /// <summary>
        /// Sets the list element at index to value. For more information on the index argument, see LINDEX.
        /// </summary>
        /// <remarks>An error is returned for out of range indexes.</remarks>
        /// <remarks>http://redis.io/commands/lset</remarks>
        Task Set(string key, int index, byte[] value);

        /// <summary>
        /// Returns the length of the list stored at key. If key does not exist, it is interpreted as an empty list and
        /// 0 is returned. 
        /// </summary>
        /// <returns>the length of the list at key.</returns>
        /// <remarks>http://redis.io/commands/llen</remarks>
        Task<long> GetLength(string key);

        /// <summary>
        /// Removes and returns the first element of the list stored at key.
        /// </summary>
        /// <returns>the value of the first element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/lpop</remarks>
        Task<string> RemoveFirstString(string key);

        /// <summary>
        /// Removes and returns the first element of the list stored at key.
        /// </summary>
        /// <returns>the value of the first element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/lpop</remarks>
        Task<byte[]> RemoveFirst(string key);

        /// <summary>
        /// Removes and returns the last element of the list stored at key.
        /// </summary>
        /// <returns>the value of the first element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/rpop</remarks>
        Task<string> RemoveLastString(string key);

        /// <summary>
        /// Removes and returns the last element of the list stored at key.
        /// </summary>
        /// <returns>the value of the first element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/rpop</remarks>
        Task<byte[]> RemoveLast(string key);

        /// <summary>
        /// IMPORTANT: blocking commands will interrupt multiplexing, and should not be used on a connection being used
        /// by parallel consumers.
        /// <para>
        /// BLPOP is a blocking list pop primitive. It is the blocking version of LPOP because it blocks the connection
        /// when there are no elements to pop from any of the given lists. An element is popped from the head of the
        /// first list that is non-empty, with the given keys being checked in the order that they are given. A timeout
        /// of zero can be used to block indefinitely.
        /// </para>
        /// </summary>
        /// <returns>
        /// A null when no element could be popped and the timeout expired, otherwise the popped element.
        /// </returns>
        /// <remarks>http://redis.io/commands/blpop</remarks>
        Task<Tuple<string, string>> BlockingRemoveFirstString(string[] keys, int timeoutSeconds);

        /// <summary>
        /// IMPORTANT: blocking commands will interrupt multiplexing, and should not be used on a connection being used
        /// by parallel consumers.
        /// <para>
        /// BLPOP is a blocking list pop primitive. It is the blocking version of LPOP because it blocks the connection
        /// when there are no elements to pop from any of the given lists. An element is popped from the head of the
        /// first list that is non-empty, with the given keys being checked in the order that they are given. A timeout
        /// of zero can be used to block indefinitely.
        /// </para>
        /// </summary>
        /// <returns>
        /// A null when no element could be popped and the timeout expired, otherwise the popped element.
        /// </returns>
        /// <remarks>http://redis.io/commands/blpop</remarks>
        Task<Tuple<string, byte[]>> BlockingRemoveFirst(string[] keys, int timeoutSeconds);

        /// <summary>
        /// IMPORTANT: blocking commands will interrupt multiplexing, and should not be used on a connection being used
        /// by parallel consumers.
        /// <para>
        /// BRPOP is a blocking list pop primitive. It is the blocking version of RPOP because it blocks the connection
        /// when there are no elements to pop from any of the given lists. An element is popped from the tail of the
        /// first list that is non-empty, with the given keys being checked in the order that they are given. A timeout
        /// of zero can be used to block indefinitely.
        /// </para>
        /// </summary>
        /// <returns>
        /// A null when no element could be popped and the timeout expired, otherwise the popped element.
        /// </returns>
        /// <remarks>http://redis.io/commands/brpop</remarks>
        Task<Tuple<string, string>> BlockingRemoveLastString(string[] keys, int timeoutSeconds);

        /// <summary>
        /// IMPORTANT: blocking commands will interrupt multiplexing, and should not be used on a connection being used
        /// by parallel consumers.
        /// <para>
        /// BRPOP is a blocking list pop primitive. It is the blocking version of RPOP because it blocks the connection
        /// when there are no elements to pop from any of the given lists. An element is popped from the tail of the
        /// first list that is non-empty, with the given keys being checked in the order that they are given. A timeout
        /// of zero can be used to block indefinitely.
        /// </para>
        /// </summary>
        /// <returns>
        /// A null when no element could be popped and the timeout expired, otherwise the popped element.
        /// </returns>
        /// <remarks>http://redis.io/commands/brpop</remarks>
        Task<Tuple<string, byte[]>> BlockingRemoveLast(string[] keys, int timeoutSeconds);

        /// <summary>
        /// IMPORTANT: blocking commands will interrupt multiplexing, and should not be used on a connection being used
        /// by parallel consumers.
        /// <para>
        /// BRPOPLPUSH is the blocking variant of RPOPLPUSH. When source contains elements, this command behaves exactly
        /// like RPOPLPUSH. When source is empty, Redis will block the connection until another client pushes to it or
        /// until timeout is reached. A timeout of zero can be used to block indefinitely.
        /// </para>
        /// </summary>
        /// <string>
        /// For example: consider source holding the list a,b,c, and destination holding the list x,y,z. Executing
        /// RPOPLPUSH results in source holding a,b and destination holding c,x,y,z.
        /// </string>
        /// <remarks>
        /// If source does not exist, the value nil is returned and no operation is performed. If source and destination
        /// are the same, the operation is equivalent to removing the last element from the list and pushing it as first
        /// element of the list, so it can be considered as a list rotation command.
        /// </remarks>
        /// <returns>the element being popped and pushed.</returns>
        /// <remarks>http://redis.io/commands/brpoplpush</remarks>
        Task<byte[]> BlockingRemoveLastAndAddFirst(string source, string destination, int timeoutSeconds);

        /// <summary>
        /// IMPORTANT: blocking commands will interrupt multiplexing, and should not be used on a connection being used
        /// by parallel consumers.
        /// <para>
        /// BRPOPLPUSH is the blocking variant of RPOPLPUSH. When source contains elements, this command behaves exactly
        /// like RPOPLPUSH. When source is empty, Redis will block the connection until another client pushes to it or
        /// until timeout is reached. A timeout of zero can be used to block indefinitely.
        /// </para>
        /// </summary>
        /// <string>
        /// For example: consider source holding the list a,b,c, and destination holding the list x,y,z. Executing
        /// RPOPLPUSH results in source holding a,b and destination holding c,x,y,z.
        /// </string>
        /// <remarks>
        /// If source does not exist, the value nil is returned and no operation is performed. If source and destination
        /// are the same, the operation is equivalent to removing the last element from the list and pushing it as first
        /// element of the list, so it can be considered as a list rotation command.
        /// </remarks>
        /// <returns>the element being popped and pushed.</returns>
        /// <remarks>http://redis.io/commands/brpoplpush</remarks>
        Task<string> BlockingRemoveLastAndAddFirstString(string source, string destination, int timeoutSeconds);

        /// <summary>
        /// Inserts value at the head of the list stored at key. If key does not exist and createIfMissing is true, it
        /// is created as empty list before performing the push operation. 
        /// </summary>
        /// <returns> the length of the list after the push operation.</returns>
        /// <remarks>http://redis.io/commands/lpush</remarks>
        /// <remarks>http://redis.io/commands/lpushx</remarks>
        Task<long> AddFirst(string key, string value, bool createIfMissing = true);

        /// <summary>
        /// Inserts value at the head of the list stored at key. If key does not exist and createIfMissing is true, it
        /// is created as empty list before performing the push operation. 
        /// </summary>
        /// <returns> the length of the list after the push operation.</returns>
        /// <remarks>http://redis.io/commands/lpush</remarks>
        /// <remarks>http://redis.io/commands/lpushx</remarks>
        Task<long> AddFirst(string key, byte[] value, bool createIfMissing = true);

        /// <summary>
        /// Inserts value at the tail of the list stored at key. If key does not exist and createIfMissing is true, it
        /// is created as empty list before performing the push operation. 
        /// </summary>
        /// <returns> the length of the list after the push operation.</returns>
        /// <remarks>http://redis.io/commands/rpush</remarks>
        /// <remarks>http://redis.io/commands/rpushx</remarks>
        Task<long> AddLast(string key, string value, bool createIfMissing = true);

        /// <summary>
        /// Inserts value at the tail of the list stored at key. If key does not exist and createIfMissing is true, it
        /// is created as empty list before performing the push operation. 
        /// </summary>
        /// <returns> the length of the list after the push operation.</returns>
        /// <remarks>http://redis.io/commands/rpush</remarks>
        /// <remarks>http://redis.io/commands/rpushx</remarks>
        Task<long> AddLast(string key, byte[] value, bool createIfMissing = true);

        /// <summary>
        /// Removes the first count occurrences of elements equal to value from the list stored at key.
        /// </summary>
        /// <remarks>
        /// The count argument influences the operation in the following ways:
        /// <para>
        /// count &gt; 0: Remove elements equal to value moving from head to tail.
        /// </para>
        /// <para>
        /// count &lt; 0: Remove elements equal to value moving from tail to head.
        /// </para>
        /// <para>
        /// count = 0: Remove all elements equal to value.
        /// </para>
        /// <para>
        /// For example, LREM list -2 "hello" will remove the last two occurrences of "hello" in the list stored at
        /// list.
        /// </para>
        /// </remarks>
        /// <returns>the number of removed elements.</returns>
        /// <remarks>http://redis.io/commands/lrem</remarks>
        Task<long> Remove(string key, string value, int count = 1);

        /// <summary>
        /// Removes the first count occurrences of elements equal to value from the list stored at key.
        /// </summary>
        /// <remarks>
        /// The count argument influences the operation in the following ways:
        /// <para>
        /// count &gt; 0: Remove elements equal to value moving from head to tail.
        /// </para>
        /// <para>
        /// count &lt; 0: Remove elements equal to value moving from tail to head.
        /// </para>
        /// <para>
        /// count = 0: Remove all elements equal to value.
        /// </para>
        /// <para>
        /// For example, LREM list -2 "hello" will remove the last two occurrences of "hello" in the list stored at
        /// list.
        /// </para>
        /// </remarks>
        /// <returns>the number of removed elements.</returns>
        /// <remarks>http://redis.io/commands/lrem</remarks>
        Task<long> Remove(string key, byte[] value, int count = 1);

        /// <summary>
        /// Trim an existing list so that it will contain only the specified range of elements specified. Both start and
        /// stop are zero-based indexes, where 0 is the first element of the list (the head), 1 the next element and so
        /// on. start and end can also be negative numbers indicating offsets from the end of the list, where -1 is the
        /// last element of the list, -2 the penultimate element and so on.
        /// </summary>
        /// <example>
        /// For example: LTRIM foobar 0 2 will modify the list stored at foobar so that only the first three elements of
        /// the list will remain.
        /// </example>
        /// <remarks>
        /// Out of range indexes will not produce an error: if start is larger than the end of the list, or start > end,
        /// the result will be an empty list (which causes key to be removed). If end is larger than the end of the
        /// list, Redis will treat it like the last element of the list.
        /// </remarks>
        /// <remarks>http://redis.io/commands/ltrim</remarks>
        Task Trim(string key, int start, int stop);

        /// <summary>
        /// Trim an existing list so that it will contain only the specified count.
        /// </summary>
        /// <remarks>http://redis.io/commands/ltrim</remarks>
        Task Trim(string key, int count);

        /// <summary>
        /// Atomically returns and removes the last element (tail) of the list stored at source, and pushes the element
        /// at the first element (head) of the list stored at destination.
        /// </summary>
        /// <string>
        /// For example: consider source holding the list a,b,c, and destination holding the list x,y,z. Executing
        /// RPOPLPUSH results in source holding a,b and destination holding c,x,y,z.
        /// </string>
        /// <remarks>
        /// If source does not exist, the value nil is returned and no operation is performed. If source and destination
        /// are the same, the operation is equivalent to removing the last element from the list and pushing it as first
        /// element of the list, so it can be considered as a list rotation command.
        /// </remarks>
        /// <returns>the element being popped and pushed.</returns>
        /// <remarks>http://redis.io/commands/rpoplpush</remarks>
        Task<byte[]> RemoveLastAndAddFirst(string source, string destination);

        /// <summary>
        /// Atomically returns and removes the last element (tail) of the list stored at source, and pushes the element
        /// at the first element (head) of the list stored at destination.
        /// </summary>
        /// <string>
        /// For example: consider source holding the list a,b,c, and destination holding the list x,y,z. Executing
        /// RPOPLPUSH results in source holding a,b and destination holding c,x,y,z.
        /// </string>
        /// <remarks>
        /// If source does not exist, the value nil is returned and no operation is performed. If source and destination
        /// are the same, the operation is equivalent to removing the last element from the list and pushing it as first
        /// element of the list, so it can be considered as a list rotation command.
        /// </remarks>
        /// <returns>the element being popped and pushed.</returns>
        /// <remarks>http://redis.io/commands/rpoplpush</remarks>
        Task<string> RemoveLastAndAddFirstString(string source, string destination);

        /// <summary>
        /// Returns the specified elements of the list stored at key. The offsets start and end are zero-based indexes,
        /// with 0 being the first element of the list (the head of the list), 1 being the next element and so on.
        /// </summary>
        /// <remarks>
        /// These offsets can also be negative numbers indicating offsets starting at the end of the list. For example,
        /// -1 is the last element of the list, -2 the penultimate, and so on.
        /// </remarks>
        /// <returns>list of elements in the specified range.</returns>
        /// <remarks>http://redis.io/commands/lrange</remarks>
        Task<string[]> RangeString(string key, int start, int stop);

        /// <summary>
        /// Returns the specified elements of the list stored at key. The offsets start and end are zero-based indexes,
        /// with 0 being the first element of the list (the head of the list), 1 being the next element and so on.
        /// </summary>
        /// <remarks>
        /// These offsets can also be negative numbers indicating offsets starting at the end of the list. For example,
        /// -1 is the last element of the list, -2 the penultimate, and so on.
        /// </remarks>
        /// <returns>list of elements in the specified range.</returns>
        /// <remarks>http://redis.io/commands/lrange</remarks>
        Task<byte[][]> Range(string key, int start, int stop);
    }
}