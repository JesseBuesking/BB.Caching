namespace BB.Caching.Compression
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Compresses data using gzip.
    /// </summary>
    public class GZipCompressor
    {
        /// <summary>
        /// Lazily loads the instance.
        /// </summary>
        private static readonly Lazy<GZipCompressor> _Lazy = new Lazy<GZipCompressor>(
            () => new GZipCompressor(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="GZipCompressor"/> class from being created.
        /// </summary>
        private GZipCompressor()
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static GZipCompressor Instance
        {
            get { return GZipCompressor._Lazy.Value; }
        }

        /// <summary>
        /// Compresses a byte array.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// A byte array containing the compressed data.
        /// </returns>
        public byte[] Compress(byte[] value)
        {
            MemoryStream ms = new MemoryStream();
            using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress))
            {
                gs.Write(value, 0, value.Length);
            }
            
            return ms.ToArray();
        }

        /// <summary>
        /// Compresses a byte array.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// A byte array containing the compressed data.
        /// </returns>
        public async Task<byte[]> CompressAsync(byte[] value)
        {
            MemoryStream ms = new MemoryStream();
            using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress))
            {
                await gs.WriteAsync(value, 0, value.Length);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Compresses a string.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// A byte array containing the compressed data.
        /// </returns>
        public byte[] Compress(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return this.Compress(bytes);
        }

        /// <summary>
        /// Compresses a string.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// A byte array containing the compressed data.
        /// </returns>
        public async Task<byte[]> CompressAsync(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return await this.CompressAsync(bytes);
        }

        /// <summary>
        /// Decompresses a byte array containing compressed data.
        /// </summary>
        /// <param name="value">
        /// The byte array of compressed data.
        /// </param>
        /// <returns>
        /// A byte array containing decompressed data.
        /// </returns>
        public byte[] Decompress(byte[] value)
        {
            MemoryStream ms = new MemoryStream(value);
            using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
            {
                using (MemoryStream fin = new MemoryStream())
                {
                    byte[] buffer = new byte[1024];
                    int numberRead;
                    while ((numberRead = gs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fin.Write(buffer, 0, numberRead);
                    }

                    return fin.ToArray();
                }
            }
        }

        /// <summary>
        /// Decompresses a byte array containing compressed data.
        /// </summary>
        /// <param name="value">
        /// The byte array of compressed data.
        /// </param>
        /// <returns>
        /// A byte array containing decompressed data.
        /// </returns>
        public async Task<byte[]> DecompressAsync(byte[] value)
        {
            using (MemoryStream ms = new MemoryStream(value))
            {
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream fin = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];
                        int numberRead;
                        while ((numberRead = await gs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fin.WriteAsync(buffer, 0, numberRead);
                        }

                        return fin.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Decompresses a byte array containing compressed data.
        /// </summary>
        /// <param name="value">
        /// The byte array of compressed data.
        /// </param>
        /// <returns>
        /// A string containing decompressed data.
        /// </returns>
        public string DecompressString(byte[] value)
        {
            byte[] decompressed = this.Decompress(value);
            return Encoding.UTF8.GetString(decompressed);
        }

        /// <summary>
        /// Decompresses a byte array containing compressed data.
        /// </summary>
        /// <param name="value">
        /// The byte array of compressed data.
        /// </param>
        /// <returns>
        /// A string containing decompressed data.
        /// </returns>
        public async Task<string> DecompressStringAsync(byte[] value)
        {
            byte[] decompressed = await this.DecompressAsync(value);
            return Encoding.UTF8.GetString(decompressed);
        }
    }
}