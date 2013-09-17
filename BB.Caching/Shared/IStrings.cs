using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BB.Caching.Shared
{
    /// <summary>
    /// Commands that apply to key/value pairs, where the value can be a string, a BLOB, or interpreted as a number
    /// </summary>
    /// <remarks>http://redis.io/commands#string</remarks>
    public interface IStrings
    {
        /// <summary>
        /// If key already exists and is a string, this command appends the value at the end of the string. If key does
        /// not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// 
        /// <returns>
        /// the length of the string after the append operation.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/append
        /// </remarks>
        Task<long> Append(string key, string value);

        /// <summary>
        /// If key already exists and is a string, this command appends the value at the end of the string. If key does
        /// not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// 
        /// <returns>
        /// the length of the string after the append operation.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/append
        /// </remarks>
        Task<long> Append(string key, byte[] value);

        /// <summary>
        /// Decrements the number stored at key by decrement. If the key does not exist, it is set to 0 before
        /// performing the operation. An error is returned if the key contains a value of the wrong type or contains a
        /// string that is not representable as integer. This operation is limited to 64 bit signed integers.
        /// </summary>
        /// 
        /// <returns>
        /// the value of key after the increment
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/decrby
        /// </remarks>
        /// 
        /// <remarks>
        /// http://redis.io/commands/decr
        /// </remarks>
        Task<long> Decrement(string key, long value = 1L);

        /// <summary>
        /// Increments the number stored at key by increment. If the key does not exist, it is set to 0 before
        /// performing the operation. An error is returned if the key contains a value of the wrong type or contains a
        /// string that is not representable as integer. This operation is limited to 64 bit signed integers.
        /// </summary>
        /// 
        /// <returns>
        /// the value of key after the increment
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/incrby
        /// </remarks>
        /// 
        /// <remarks>
        /// http://redis.io/commands/incr
        /// </remarks>
        Task<long> Increment(string key, long value = 1L);

        /// <summary>
        /// Get the value of key. If the key does not exist the special value nil is returned. An error is returned if
        /// the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// 
        /// <returns>
        /// the value of key, or nil when key does not exist.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/get
        /// </remarks>
        Wrapper<byte[], byte[]> GetByteArray(string key);

        /// <summary>
        /// Get the value of key. If the key does not exist the special value nil is returned. An error is returned if
        /// the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// 
        /// <returns>
        /// the value of key, or nil when key does not exist.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/get
        /// </remarks>
        Wrapper<string, string> GetString(string key);

        /// <summary>
        /// Get the value of key. If the key does not exist the special value nil is returned. An error is returned if
        /// the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// 
        /// <returns>
        /// the value of key, or nil when key does not exist.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/get
        /// </remarks>
        Wrapper<long, long?> GetInt64(string key);

        /// <summary>
        /// Returns the substring of the string value stored at key, determined by the offsets start and end (both are
        /// inclusive).
        /// </summary>
        /// 
        /// <remarks>
        /// Negative offsets can be used in order to provide an offset starting from the end of the string. So -1 means
        /// the last character, -2 the penultimate and so forth. The function handles out of range requests by limiting
        /// the resulting range to the actual length of the string.
        /// </remarks>
        /// 
        /// <returns>
        /// the value of key, or nil when key does not exist.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/getrange
        /// </remarks>
        Wrapper<byte[], byte[]> GetByteArray(string key, int start, int end);

        /// <summary>
        /// Returns the substring of the string value stored at key, determined by the offsets start and end (both are
        /// inclusive).
        /// </summary>
        /// 
        /// <remarks>
        /// Negative offsets can be used in order to provide an offset starting from the end of the string. So -1 means
        /// the last character, -2 the penultimate and so forth. The function handles out of range requests by limiting
        /// the resulting range to the actual length of the string.
        /// </remarks>
        /// 
        /// <returns>
        /// the value of key, or nil when key does not exist.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/getrange
        /// </remarks>
        Wrapper<string, string> GetString(string key, int start, int end);

        /// <summary>
        /// Returns the values of all specified keys. For every key that does not hold a string value or does not exist,
        /// the special value nil is returned. Because of this, the operation never fails.
        /// </summary>
        /// 
        /// <returns>
        /// list of values at the specified keys.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/mget
        /// </remarks>
        Task<Wrapper<byte[], byte[]>[]> GetByteArray(string[] keys);

        /// <summary>
        /// Returns the values of all specified keys. For every key that does not hold a string value or does not exist,
        /// the special value nil is returned. Because of this, the operation never fails.
        /// </summary>
        /// 
        /// <returns>
        /// list of values at the specified keys.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/mget
        /// </remarks>
        Task<Wrapper<string, string>[]> GetString(string[] keys);

        /// <summary>
        /// Atomically sets key to value and returns the old value stored at key. Returns an error when key exists but
        /// does not hold a string value.
        /// </summary>
        /// 
        /// <returns>
        /// the old value stored at key, or nil when key did not exist.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/getset
        /// </remarks>
        Wrapper<string, string> GetSet(string key, string value);

        /// <summary>
        /// Atomically sets key to value and returns the old value stored at key. Returns an error when key exists but
        /// does not hold a string value.
        /// </summary>
        /// 
        /// <returns>
        /// the old value stored at key, or nil when key did not exist.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/getset
        /// </remarks>
        Wrapper<byte[], byte[]> GetSet(string key, byte[] value);

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// 
        /// <remarks>
        /// http://redis.io/commands/set
        /// </remarks>
        Task Set(string key, string value);

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// 
        /// <remarks>
        /// http://redis.io/commands/set
        /// </remarks>
        Task Set(string key, long value);

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// 
        /// <remarks>
        /// http://redis.io/commands/set
        /// </remarks>
        Task Set(string key, byte[] value);

        /// <summary>
        /// Set key to hold the string value and set key to timeout after a given number of seconds.
        /// </summary>
        /// 
        /// <remarks>
        /// http://redis.io/commands/setex
        /// </remarks>
        Task Set(string key, string value, TimeSpan expiry);

        /// <summary>
        /// Set key to hold the string value and set key to timeout after a given number of seconds.
        /// </summary>
        /// 
        /// <remarks>
        /// http://redis.io/commands/setex
        /// </remarks>
        Task Set(string key, byte[] value, TimeSpan expiry);

        /// <summary>
        /// Overwrites part of the string stored at key, starting at the specified offset, for the entire length of
        /// value. If the offset is larger than the current length of the string at key, the string is padded with
        /// zero-bytes to make offset fit. Non-existing keys are considered as empty strings, so this command will make
        /// sure it holds a string large enough to be able to set value at offset.
        /// </summary>
        /// 
        /// <remarks>
        /// Note that the maximum offset that you can set is 229 -1 (536870911), as Redis Strings are limited to 512
        /// megabytes. If you need to grow beyond this size, you can use multiple keys.
        /// <para>
        /// Warning: When setting the last possible byte and the string value stored at key does not yet hold a string
        /// value, or holds a small string value, Redis needs to allocate all intermediate memory which can block the
        /// server for some time. On a 2010 MacBook Pro, setting byte number 536870911 (512MB allocation) takes ~300ms,
        /// setting byte number 134217728 (128MB allocation) takes ~80ms, setting bit number 33554432 (32MB allocation)
        /// takes ~30ms and setting bit number 8388608 (8MB allocation) takes ~8ms. Note that once this first allocation
        /// is done, subsequent calls to SETRANGE for the same key will not have the allocation overhead.
        /// </para>
        /// </remarks>
        /// 
        /// <remarks>
        /// http://redis.io/commands/setrange
        /// </remarks>
        /// 
        /// <returns>
        /// the length of the string after it was modified by the command.
        /// </returns>
        Task<long> Set(string key, long offset, string value);

        /// <summary>
        /// Overwrites part of the string stored at key, starting at the specified offset, for the entire length of
        /// value. If the offset is larger than the current length of the string at key, the string is padded with
        /// zero-bytes to make offset fit. Non-existing keys are considered as empty strings, so this command will make
        /// sure it holds a string large enough to be able to set value at offset.
        /// </summary>
        /// 
        /// <remarks>
        /// Note that the maximum offset that you can set is 229 -1 (536870911), as Redis Strings are limited to 512
        /// megabytes. If you need to grow beyond this size, you can use multiple keys.
        /// <para>
        /// Warning: When setting the last possible byte and the string value stored at key does not yet hold a string
        /// value, or holds a small string value, Redis needs to allocate all intermediate memory which can block the
        /// server for some time. On a 2010 MacBook Pro, setting byte number 536870911 (512MB allocation) takes ~300ms,
        /// setting byte number 134217728 (128MB allocation) takes ~80ms, setting bit number 33554432 (32MB allocation)
        /// takes ~30ms and setting bit number 8388608 (8MB allocation) takes ~8ms. Note that once this first allocation
        /// is done, subsequent calls to SETRANGE for the same key will not have the allocation overhead.
        /// </para>
        /// </remarks>
        /// 
        /// <remarks>
        /// http://redis.io/commands/setrange
        /// </remarks>
        /// 
        /// <returns>
        /// the length of the string after it was modified by the command.
        /// </returns>
        Task<long> Set(string key, long offset, byte[] value);

        /// <summary>
        /// Sets the given keys to their respective values. MSET replaces existing values with new values, just as
        /// regular SET. See MSETNX if you don't want to overwrite existing values.
        /// </summary>
        /// 
        /// <remarks>
        /// MSET is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
        /// keys were updated while others are unchanged.
        /// </remarks>
        /// 
        /// <remarks>
        /// http://redis.io/commands/mset
        /// </remarks>
        Task Set(Dictionary<string, string> values);

        /// <summary>
        /// Sets the given keys to their respective values. MSET replaces existing values with new values, just as
        /// regular SET. See MSETNX if you don't want to overwrite existing values.
        /// </summary>
        /// 
        /// <remarks>
        /// MSET is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
        /// keys were updated while others are unchanged.
        /// </remarks>
        /// 
        /// <remarks>
        /// http://redis.io/commands/mset
        /// </remarks>
        Task Set(Dictionary<string, byte[]> values);

        /// <summary>
        /// Sets the given keys to their respective values. MSETNX will not perform any operation at all even if just a
        /// single key already exists.
        /// </summary>
        /// 
        /// <remarks>
        /// Because of this semantic MSETNX can be used in order to set different keys representing different fields of
        /// an unique logic object in a way that ensures that either all the fields or none at all are set.
        /// <para>
        /// MSETNX is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
        /// keys were updated while others are unchanged.
        /// </para>
        /// </remarks>
        /// 
        /// <returns>
        /// 1 if the all the keys were set, 0 if no key was set (at least one key already existed).
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/msetnx
        /// </remarks>
        Task<bool> SetIfNotExists(Dictionary<string, string> values);

        /// <summary>
        /// Sets the given keys to their respective values. MSETNX will not perform any operation at all even if just a
        /// single key already exists.
        /// </summary>
        /// 
        /// <remarks>
        /// Because of this semantic MSETNX can be used in order to set different keys representing different fields of
        /// an unique logic object in a way that ensures that either all the fields or none at all are set.
        /// <para>
        /// MSETNX is atomic, so all given keys are set at once. It is not possible for clients to see that some of the
        /// keys were updated while others are unchanged.
        /// </para>
        /// </remarks>
        /// 
        /// <returns>
        /// 1 if the all the keys were set, 0 if no key was set (at least one key already existed).
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/msetnx
        /// </remarks>
        Task<bool> SetIfNotExists(Dictionary<string, byte[]> values);

        /// <summary>
        /// Set key to hold string value if key does not exist. In that case, it is equal to SET. When key already holds
        /// a value, no operation is performed.
        /// </summary>
        /// 
        /// <returns>
        /// 1 if the key was set, 0 if the key was not set
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/setnx
        /// </remarks>
        Task<bool> SetIfNotExists(string key, string value);

        /// <summary>
        /// Set key to hold string value if key does not exist. In that case, it is equal to SET. When key already holds
        /// a value, no operation is performed.
        /// </summary>
        /// 
        /// <returns>
        /// 1 if the key was set, 0 if the key was not set
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/setnx
        /// </remarks>
        Task<bool> SetIfNotExists(string key, byte[] value);

        /// <summary>
        /// Returns the bit value at offset in the string value stored at key.
        /// </summary>
        /// 
        /// <remarks>
        /// When offset is beyond the string length, the string is assumed to be a contiguous space with 0 bits. When
        /// key does not exist it is assumed to be an empty string, so offset is always out of range and the value is
        /// also assumed to be a contiguous space with 0 bits.
        /// </remarks>
        /// 
        /// <returns>
        /// the bit value stored at offset.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/getbit
        /// </remarks>
        Task<bool> GetBit(string key, long offset);

        /// <summary>
        /// Returns the length of the string value stored at key. An error is returned when key holds a non-string
        /// value.
        /// </summary>
        /// 
        /// <returns>
        /// the length of the string at key, or 0 when key does not exist.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/strlen
        /// </remarks>
        Task<long> GetLength(string key);

        /// <summary>
        /// Sets or clears the bit at offset in the string value stored at key.
        /// </summary>
        /// 
        /// <remarks>
        /// The bit is either set or cleared depending on value, which can be either 0 or 1. When key does not exist, a
        /// new string value is created. The string is grown to make sure it can hold a bit at offset. The offset
        /// argument is required to be greater than or equal to 0, and smaller than 232 (this limits bitmaps to 512MB).
        /// When the string at key is grown, added bits are set to 0.
        /// <para>
        /// Warning: When setting the last possible bit (offset equal to 232 -1) and the string value stored at key does
        /// not yet hold a string value, or holds a small string value, Redis needs to allocate all intermediate memory
        /// which can block the server for some time. On a 2010 MacBook Pro, setting bit number 232 -1 (512MB
        /// allocation) takes ~300ms, setting bit number 230 -1 (128MB allocation) takes ~80ms, setting bit number
        /// 228 -1 (32MB allocation) takes ~30ms and setting bit number 226 -1 (8MB allocation) takes ~8ms. Note that
        /// once this first allocation is done, subsequent calls to SETBIT for the same key will not have the allocation
        /// overhead.
        /// </para>
        /// </remarks>
        /// 
        /// <returns>
        /// the original bit value stored at offset.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/setbit
        /// </remarks>
        Task<bool> SetBit(string key, long offset, bool value);

        /// <summary>
        /// Count the number of set bits (population counting) in a string.
        /// <para>
        /// By default all the bytes contained in the string are examined. It is possible to specify the counting
        /// operation only in an interval passing the additional arguments start and end.
        /// </para>
        /// <para>
        /// Like for the GETRANGE command start and end can contain negative values in order to index bytes starting
        /// from the end of the string, where -1 is the last byte, -2 is the penultimate, and so forth.
        /// </para>
        /// </summary>
        /// 
        /// <remarks>
        /// http://redis.io/commands/bitcount
        /// </remarks>
        Task<long> CountSetBits(string key, long start = 0L, long count = -1L);

        /// <summary>
        /// Perform a bitwise AND operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// 
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        Task<long> BitwiseAnd(string destination, string[] keys);

        /// <summary>
        /// Perform a bitwise OR operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// 
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        Task<long> BitwiseOr(string destination, string[] keys);

        /// <summary>
        /// Perform a bitwise XOR operation between multiple keys (containing string values) and store the result in the
        /// destination key.
        /// </summary>
        /// 
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        Task<long> BitwiseXOr(string destination, string[] keys);

        /// <summary>
        /// Perform a bitwise NOT operation on a key (containing a string value) and store the result in the destination
        /// key.
        /// </summary>
        /// 
        /// <returns>
        /// The size of the string stored in the destination key, that is equal to the size of the longest input string.
        /// </returns>
        /// 
        /// <remarks>
        /// http://redis.io/commands/bitop
        /// </remarks>
        Task<long> BitwiseNot(string destination, string key);

        /// <summary>
        /// This is a composite helper command, to help with using redis as a lock provider. This is achieved as a
        /// string key/value pair with timeout. If the lock does not exist (or has expired), then a new string key is
        /// created (with the supplied duration), and <c>true</c> is returned to indicate success. If the lock already
        /// exists, then no lock is taken, and <c>false</c> is returned. The value may be fetched separately, but the
        /// meaning is implementation-defined). No change is made if the lock was not successfully taken. In this case,
        /// the client should delay and retry.
        /// <para>
        /// It is expected that a well-behaved client will also release the lock in a timely fashion via
        /// <see cref="ReleaseLock">ReleaseLock</see>.
        /// </para>
        /// </summary>
        /// 
        /// <returns>
        /// <c>null</c> if the lock was successfully taken; the competing value otherwise
        /// </returns>
        /// 
        /// <remarks>
        /// It transpires that robust locking in redis is actually remarkably hard, and most implementations are broken
        /// in one way or another (most commonly: thread-race, or extending the lock duration when failing to take the
        /// lock).
        /// </remarks>
        Task<bool> TakeLock(string key, string value, TimeSpan expiry);

        /// <summary>
        /// Releases a lock that was taken successfully via TakeLock. You should not release a lock that you did not
        /// take, as this will cause problems.
        /// </summary>
        Task ReleaseLock(string key);
    }
}