namespace BB.Caching.Redis
{
    using System.Threading.Tasks;

    using StackExchange.Redis;

    /// <summary>
    /// Methods for performing bitwise analytics.
    /// <para>
    /// Note: bitwise operations use byte-sized chunks, so setting the first bit to a value will allocate a full byte
    /// worth of data.
    /// </para>
    /// <remarks>
    /// Check out the following links:
    /// http://blog.getspool.com/2011/11/29/fast-easy-realtime-metrics-using-redis-bitmaps
    /// http://amix.dk/blog/post/19714
    /// </remarks>
    /// </summary>
    public static class BitwiseAnalytics
    {
        /// <summary>
        /// Perform a bitwise AND operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be AND'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static long BitwiseAnd(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            long result = database
                .StringBitOperation(Bitwise.And, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise AND operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be AND'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static Task<long> BitwiseAndAsync(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            Task<long> result = database
                .StringBitOperationAsync(Bitwise.And, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise OR operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be OR'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static long BitwiseOr(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            long result = database
                .StringBitOperation(Bitwise.Or, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise OR operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be OR'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static Task<long> BitwiseOrAsync(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            Task<long> result = database
                .StringBitOperationAsync(Bitwise.Or, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise XOR operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be XOR'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static long BitwiseXOr(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            long result = database
                .StringBitOperation(Bitwise.Xor, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise XOR operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="keys">
        /// The keys where the data to be XOR'd are located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static Task<long> BitwiseXOrAsync(IDatabase database, RedisKey destination, RedisKey[] keys)
        {
            Task<long> result = database
                .StringBitOperationAsync(Bitwise.Xor, destination, keys);

            return result;
        }

        /// <summary>
        /// Perform a bitwise NOT operation on a key (containing a string value) and store the result in the destination
        /// key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="key">
        /// The key where the data to be NOT'd is located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static long BitwiseNot(IDatabase database, RedisKey destination, RedisKey key)
        {
            long result = database
                .StringBitOperation(Bitwise.Not, destination, key);

            return result;
        }

        /// <summary>
        /// Perform a bitwise NOT operation on a key (containing a string value) and store the result in the destination
        /// key.
        /// </summary>
        /// <param name="database">
        /// The database where the query will be performed. This is passed so that we can reuse the same database to
        /// perform multiple bitwise operations. Doing this with the same connection will guarantee that performance
        /// is good.
        /// </param>
        /// <param name="destination">
        /// The destination key where the result should be stored.
        /// </param>
        /// <param name="key">
        /// The key where the data to be NOT'd is located.
        /// </param>
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        public static Task<long> BitwiseNotAsync(IDatabase database, RedisKey destination, RedisKey key)
        {
            Task<long> result = database
                .StringBitOperationAsync(Bitwise.Not, destination, key);

            return result;
        }
    }
}
