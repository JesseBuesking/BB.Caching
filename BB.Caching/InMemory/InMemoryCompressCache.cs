using System;
using System.Text;
using System.Threading.Tasks;
using BB.Caching.Compression;

namespace BB.Caching.InMemory
{
    public partial class InMemoryCache : ICompressCache
    {
        public bool TryGetDecompress(string key, out byte[] value)
        {
            byte[] stored;
            if (!this.TryGet(key, out stored))
            {
                value = null;
                return false;
            }

            value = Compressor.Instance.Decompress(stored);
            return true;
        }

        public Wrapper<byte[], byte[]> TryGetByteArrayDecompressAsync(string key)
        {
            var result = new Wrapper<byte[], byte[]>();
            byte[] stored;
            if (!this.TryGet(key, out stored))
            {
                result.ValueAsync = Task.Run(() => (byte[]) null);
                result.IsNilAsync = Task.Run(() => true);
                return result;
            }

            result.ValueAsync = Compressor.Instance.DecompressAsync(stored);
            result.IsNilAsync = Task.Run(() => false);
            return result;
        }

        public bool TryGetDecompress(string key, out string value)
        {
            byte[] stored;
            if (!this.TryGet(key, out stored))
            {
                value = null;
                return false;
            }

            stored = Compressor.Instance.Decompress(stored);
            value = Encoding.UTF8.GetString(stored);
            return true;
        }

        public Wrapper<string, string> TryGetStringDecompressAsync(string key)
        {
            var result = new Wrapper<string, string>();
            byte[] stored;
            if (!this.TryGet(key, out stored))
            {
                result.ValueAsync = Task.Run(() => (string) null);
                result.IsNilAsync = Task.Run(() => true);
                return result;
            }

            result.ValueAsync = Task.Run(async () =>
                Encoding.UTF8.GetString(await Compressor.Instance.DecompressAsync(stored)));
            result.IsNilAsync = Task.Run(() => false);
            return result;
        }

        public byte[] SetCompress(string key, byte[] value)
        {
            byte[] compressed = Compressor.Instance.Compress(value);
            this.Set(key, compressed);
            return compressed;
        }

        public async Task<byte[]> SetCompressAsync(string key, byte[] value)
        {
            byte[] compressed = await Compressor.Instance.CompressAsync(value);
            this.Set(key, compressed);
            return compressed;
        }

        public byte[] SetCompress(string key, byte[] value, TimeSpan absoluteExpiration)
        {
            byte[] compressed = Compressor.Instance.Compress(value);
            this.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public async Task<byte[]> SetCompressAsync(string key, byte[] value, TimeSpan absoluteExpiration)
        {
            byte[] compressed = await Compressor.Instance.CompressAsync(value);
            this.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public byte[] SetCompressSliding(string key, byte[] value, TimeSpan slidingExpiration)
        {
            byte[] compressed = Compressor.Instance.Compress(value);
            this.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }

        public async Task<byte[]> SetCompressSlidingAsync(string key, byte[] value, TimeSpan slidingExpiration)
        {
            byte[] compressed = await Compressor.Instance.CompressAsync(value);
            this.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }

        public byte[] SetCompress(string key, string value)
        {
            byte[] compressed = Compressor.Instance.Compress(value);
            this.Set(key, compressed);
            return compressed;
        }

        public async Task<byte[]> SetCompressAsync(string key, string value)
        {
            byte[] compressed = await Compressor.Instance.CompressAsync(value);
            this.Set(key, compressed);
            return compressed;
        }

        public byte[] SetCompress(string key, string value, TimeSpan absoluteExpiration)
        {
            byte[] compressed = Compressor.Instance.Compress(value);
            this.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public async Task<byte[]> SetCompressAsync(string key, string value, TimeSpan absoluteExpiration)
        {
            byte[] compressed = await Compressor.Instance.CompressAsync(value);
            this.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public byte[] SetCompressSliding(string key, string value, TimeSpan slidingExpiration)
        {
            byte[] compressed = Compressor.Instance.Compress(value);
            this.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }

        public async Task<byte[]> SetCompressSlidingAsync(string key, string value, TimeSpan slidingExpiration)
        {
            byte[] compressed = await Compressor.Instance.CompressAsync(value);
            this.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }
    }
}