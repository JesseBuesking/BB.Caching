namespace BB.Caching.Compression
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Intelligently compresses data using gzip or the original raw data, whichever is smallest.
    /// </summary>
    public class SmartCompressor
    {
        /// <summary>
        /// Lazily loads the instance.
        /// </summary>
        private static readonly Lazy<SmartCompressor> _Lazy = new Lazy<SmartCompressor>(
            () => new SmartCompressor(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="SmartCompressor"/> class from being created.
        /// </summary>
        private SmartCompressor()
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static SmartCompressor Instance
        {
            get { return SmartCompressor._Lazy.Value; }
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
            byte[] compressed = GZipCompressor.Instance.Compress(value);
            if (compressed.Length < value.Length)
            {
                byte[] fin = new byte[compressed.Length + 1];
                using (MemoryStream ms = new MemoryStream(fin))
                {
                    ms.Write(new byte[] { 1 }, 0, 1);
                    ms.Write(compressed, 0, compressed.Length);
                    return ms.ToArray();
                }
            }
            else
            {
                byte[] fin = new byte[value.Length + 1];
                using (MemoryStream ms = new MemoryStream(fin))
                {
                    ms.Write(new byte[] { 0 }, 0, 1);
                    ms.Write(value, 0, value.Length);
                    return ms.ToArray();
                }
            }
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
            byte[] compressed = await GZipCompressor.Instance.CompressAsync(value);
            if (compressed.Length < value.Length)
            {
                byte[] fin = new byte[compressed.Length + 1];
                using (MemoryStream ms = new MemoryStream(fin))
                {
                    await ms.WriteAsync(new byte[] { 1 }, 0, 1);
                    await ms.WriteAsync(compressed, 0, compressed.Length);
                    return ms.ToArray();
                }
            }
            else
            {
                byte[] fin = new byte[value.Length + 1];
                using (MemoryStream ms = new MemoryStream(fin))
                {
                    await ms.WriteAsync(new byte[] { 0 }, 0, 1);
                    await ms.WriteAsync(value, 0, value.Length);
                    return ms.ToArray();
                }
            }
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
            if (value.Length <= 0)
            {
                return new byte[0];
            }

            int count = value.Length - 1;
            byte[] temp = new byte[count];
            System.Buffer.BlockCopy(value, 1, temp, 0, count);

            return value[0] == 0 ? temp : GZipCompressor.Instance.Decompress(temp);
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
            if (value.Length <= 0)
            {
                return new byte[0];
            }

            int count = value.Length - 1;
            byte[] temp = new byte[count];
            System.Buffer.BlockCopy(value, 1, temp, 0, count);

            return value[0] == 0 ? temp : await GZipCompressor.Instance.DecompressAsync(temp);
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