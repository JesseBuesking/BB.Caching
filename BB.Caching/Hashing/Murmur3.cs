using System;
using System.Text;

// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    public static partial class Hashing
    {
        public static class Murmur3
        {
            public static uint ComputeInt(string value)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                byte[] res = Hashing.Core.Instance.MurmurHash.ComputeHash(bytes);
                return BitConverter.ToUInt32(res, 0);
            }

            public static uint ComputeInt(byte[] value)
            {
                byte[] res = Hashing.Core.Instance.MurmurHash.ComputeHash(value);
                return BitConverter.ToUInt32(res, 0);
            }

            public static byte[] ComputeHash(string value)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                byte[] res = Hashing.Core.Instance.MurmurHash.ComputeHash(bytes);
                return res;
            }

            public static byte[] ComputeHash(byte[] value)
            {
                byte[] res = Hashing.Core.Instance.MurmurHash.ComputeHash(value);
                return res;
            }
        }
    }
}