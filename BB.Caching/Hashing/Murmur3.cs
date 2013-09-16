using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Murmur;

namespace BB.Caching.Hashing
{
    public class Murmur3 : IHashAlgorithm
    {
        private static readonly Lazy<Murmur3> _lazy = new Lazy<Murmur3>(
            () => new Murmur3(), LazyThreadSafetyMode.ExecutionAndPublication); 

        public static Murmur3 Instance
        {
            get { return _lazy.Value; }
        }

        private HashAlgorithm _murmurHash;

        private Murmur3()
        {
            this.Config();
        }

        public void Config(uint seed = 0, bool managed = true)
        {
            this._murmurHash = MurmurHash.Create32(seed); //, managed);
        }

        public uint ComputeInt(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            byte[] res = this._murmurHash.ComputeHash(bytes);
            return BitConverter.ToUInt32(res, 0);
        }

        public uint ComputeInt(byte[] value)
        {
            byte[] res = this._murmurHash.ComputeHash(value);
            return BitConverter.ToUInt32(res, 0);
        }

        public byte[] ComputeHash(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            byte[] res = this._murmurHash.ComputeHash(bytes);
            return res;
        }

        public byte[] ComputeHash(byte[] value)
        {
            byte[] res = this._murmurHash.ComputeHash(value);
            return res;
        }
    }
}