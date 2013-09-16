using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BB.Caching.Compression
{
    public class GZipCompressor : ICompress
    {
        private static readonly Lazy<GZipCompressor> _lazy = new Lazy<GZipCompressor>(
            () => new GZipCompressor(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static GZipCompressor Instance
        {
            get { return GZipCompressor._lazy.Value; }
        }

        private GZipCompressor()
        {
            
        }

        public async Task<byte[]> CompressAsync(byte[] value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress))
                {
                    await gs.WriteAsync(value, 0, value.Length);
                }

                return ms.ToArray();
            }
        }

        public async Task<byte[]> CompressAsync(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return await CompressAsync(bytes);
        }

        public byte[] Compress(byte[] value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress))
                {
                    gs.Write(value, 0, value.Length);
                }

                return ms.ToArray();
            }
        }

        public byte[] Compress(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Compress(bytes);
        }

        public async Task<byte[]> DecompressAsync(byte[] value)
        {
            using (MemoryStream ms = new MemoryStream(value))
            {
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream fin = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];
                        int nRead;
                        while ((nRead = await gs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fin.WriteAsync(buffer, 0, nRead);
                        }

                        return fin.ToArray();
                    }
                }
            }
        }

        public async Task<string> DecompressStringAsync(byte[] value)
        {
            byte[] decompressed = await DecompressAsync(value);
            return Encoding.UTF8.GetString(decompressed);
        }

        public byte[] Decompress(byte[] value)
        {
            using (MemoryStream ms = new MemoryStream(value))
            {
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream fin = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];
                        int nRead;
                        while ((nRead = gs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fin.Write(buffer, 0, nRead);
                        }

                        return fin.ToArray();
                    }
                }
            }
        }

        public string DecompressString(byte[] value)
        {
            byte[] decompressed = Decompress(value);
            return Encoding.UTF8.GetString(decompressed);
        }
    }
}