using System;
using System.Threading.Tasks;

namespace BB.Caching
{
    public partial class InMemoryCache : ICompactCache
    {
        public bool TryGetDecompact<TObject>(string key, out TObject value)
        {
            byte[] stored;
            if (!this.TryGet(key, out stored))
            {
                value = default (TObject);
                return false;
            }

            stored = Compressor.Instance.Decompress(stored);
            value = Serializer.Instance.Deserialize<TObject>(stored);
            return true;
        }

        public Wrapper<TObject, TObject> TryGetDecompactAsync<TObject>(string key)
        {
            var result = new Wrapper<TObject, TObject>();
            byte[] stored;
            if (!this.TryGet(key, out stored))
            {
                result.ValueAsync = Task.Run(() => default (TObject));
                result.IsNilAsync = Task.Run(() => true);
                return result;
            }

            result.ValueAsync = Task.Run(async () =>
                Serializer.Instance.Deserialize<TObject>(await Compressor.Instance.DecompressAsync(stored)));
            result.IsNilAsync = Task.Run(() => false);
            return result;
        }

        public byte[] SetCompact<TObject>(string key, TObject value)
        {
            byte[] serialized = Serializer.Instance.Serialize(value);
            byte[] compressed = Compressor.Instance.Compress(serialized);
            this.Set(key, compressed);
            return compressed;
        }

        public async Task<byte[]> SetCompactAsync<TObject>(string key, TObject value)
        {
            byte[] serialized = Serializer.Instance.Serialize(value);
            byte[] compressed = await Compressor.Instance.CompressAsync(serialized);
            this.Set(key, compressed);
            return compressed;
        }

        public byte[] SetCompact<TObject>(string key, TObject value, TimeSpan absoluteExpiration)
        {
            byte[] serialized = Serializer.Instance.Serialize(value);
            byte[] compressed = Compressor.Instance.Compress(serialized);
            this.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public async Task<byte[]> SetCompactAsync<TObject>(string key, TObject value, TimeSpan absoluteExpiration)
        {
            byte[] serialized = Serializer.Instance.Serialize(value);
            byte[] compressed = await Compressor.Instance.CompressAsync(serialized);
            this.Set(key, compressed, absoluteExpiration);
            return compressed;
        }

        public byte[] SetCompactSliding<TObject>(string key, TObject value, TimeSpan slidingExpiration)
        {
            byte[] serialized = Serializer.Instance.Serialize(value);
            byte[] compressed = Compressor.Instance.Compress(serialized);
            this.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }

        public async Task<byte[]> SetCompactSlidingAsync<TObject>(string key, TObject value, TimeSpan slidingExpiration)
        {
            byte[] serialized = Serializer.Instance.Serialize(value);
            byte[] compressed = await Compressor.Instance.CompressAsync(serialized);
            this.SetSliding(key, compressed, slidingExpiration);
            return compressed;
        }
    }
}