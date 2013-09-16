using System;
using System.Security.Cryptography;
using System.Text;

namespace BB.Caching.Hashing
{
    public class Md5 : IHashAlgorithm
    {
        private readonly MD5 _md5 = MD5.Create();

        public uint ComputeInt(string value)
        {
            byte[] bytes = this._md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToUInt32(bytes, 0);
        }

        public uint ComputeInt(byte[] value)
        {
            byte[] bytes = this._md5.ComputeHash(value);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public byte[] ComputeHash(string value)
        {
            return this._md5.ComputeHash(Encoding.UTF8.GetBytes(value));
        }

        public byte[] ComputeHash(byte[] value)
        {
            return this._md5.ComputeHash(value);
        }
    }
}