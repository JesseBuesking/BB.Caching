// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    using System;
    using System.Text;

    /// <summary>
    /// Methods and classes for hashing data.
    /// </summary>
    public static partial class Hashing
    {
        /// <summary>
        /// Helper functions when using murmur hash.
        /// </summary>
        public static class Murmur3
        {
            /// <summary>
            /// Computes a uint representing the murmur hash generated from a string.
            /// </summary>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <returns>
            /// The <see cref="uint"/> result.
            /// </returns>
            public static uint ComputeInt(string value)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                byte[] res = Hashing.Core.Instance.MurmurHash.ComputeHash(bytes);
                return BitConverter.ToUInt32(res, 0);
            }

            /// <summary>
            /// Computes a uint representing the murmur hash generated from a byte array.
            /// </summary>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <returns>
            /// The <see cref="uint"/> result.
            /// </returns>
            public static uint ComputeInt(byte[] value)
            {
                byte[] res = Hashing.Core.Instance.MurmurHash.ComputeHash(value);
                return BitConverter.ToUInt32(res, 0);
            }

            /// <summary>
            /// Computes a byte array representing the murmur hash generated from a string.
            /// </summary>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <returns>
            /// The <see><cref>byte[]</cref></see> result.
            /// </returns>
            public static byte[] ComputeHash(string value)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                byte[] res = Hashing.Core.Instance.MurmurHash.ComputeHash(bytes);
                return res;
            }

            /// <summary>
            /// Computes a byte array representing the murmur hash generated from a byte array.
            /// </summary>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <returns>
            /// The <see><cref>byte[]</cref></see> result.
            /// </returns>
            public static byte[] ComputeHash(byte[] value)
            {
                byte[] res = Hashing.Core.Instance.MurmurHash.ComputeHash(value);
                return res;
            }
        }
    }
}