using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BB.Caching.Compression
{
    public class SmartCompressor
    {
        private static readonly Lazy<SmartCompressor> _lazy = new Lazy<SmartCompressor>(
            () => new SmartCompressor(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static SmartCompressor Instance
        {
            get { return SmartCompressor._lazy.Value; }
        }

        private SmartCompressor()
        {
        }

        public async Task<byte[]> CompressAsync(byte[] value)
        {
            byte[] compressed = await GZipCompressor.Instance.CompressAsync(value);
            if (compressed.Length < value.Length)
            {
                byte[] fin = new byte[compressed.Length + 1];
                using (MemoryStream ms = new MemoryStream(fin))
                {
                    await ms.WriteAsync(new byte[] {1}, 0, 1);
                    await ms.WriteAsync(compressed, 0, compressed.Length);
                    return ms.ToArray();
                }
            }
            else
            {
                byte[] fin = new byte[value.Length + 1];
                using (MemoryStream ms = new MemoryStream(fin))
                {
                    await ms.WriteAsync(new byte[] {0}, 0, 1);
                    await ms.WriteAsync(value, 0, value.Length);
                    return ms.ToArray();
                }
            }
        }

        public async Task<byte[]> CompressAsync(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return await CompressAsync(bytes);
        }

        public byte[] Compress(byte[] value)
        {
            byte[] compressed = GZipCompressor.Instance.Compress(value);
            if (compressed.Length < value.Length)
            {
                byte[] fin = new byte[compressed.Length + 1];
                using (MemoryStream ms = new MemoryStream(fin))
                {
                    ms.Write(new byte[] {1}, 0, 1);
                    ms.Write(compressed, 0, compressed.Length);
                    return ms.ToArray();
                }
            }
            else
            {
                byte[] fin = new byte[value.Length + 1];
                using (MemoryStream ms = new MemoryStream(fin))
                {
                    ms.Write(new byte[] {0}, 0, 1);
                    ms.Write(value, 0, value.Length);
                    return ms.ToArray();
                }
            }
        }

        public byte[] Compress(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Compress(bytes);
        }

        public async Task<byte[]> DecompressAsync(byte[] value)
        {
            if (value.Length <= 0)
                return new byte[0];

            int count = value.Length - 1;
            byte[] temp = new byte[count];
            System.Buffer.BlockCopy(value, 1, temp, 0, count);

            if (value[0] == 0)
                return temp;

            return await GZipCompressor.Instance.DecompressAsync(temp);
        }

        public async Task<string> DecompressStringAsync(byte[] value)
        {
            byte[] decompressed = await this.DecompressAsync(value);
            return Encoding.UTF8.GetString(decompressed);
        }

        public byte[] Decompress(byte[] value)
        {
            if (value.Length <= 0)
                return new byte[0];

            int count = value.Length - 1;
            byte[] temp = new byte[count];
            System.Buffer.BlockCopy(value, 1, temp, 0, count);

            if (value[0] == 0)
                return temp;

            return GZipCompressor.Instance.Decompress(temp);
        }

        public string DecompressString(byte[] value)
        {
            byte[] decompressed = this.Decompress(value);
            return Encoding.UTF8.GetString(decompressed);
        }
    }
}