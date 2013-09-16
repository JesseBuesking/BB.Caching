using System;
using System.Threading.Tasks;

namespace BB.Caching
{
	public interface ICompressCache
	{
		bool TryGetDecompress(string key, out byte[] value);

		Wrapper<byte[], byte[]> TryGetByteArrayDecompressAsync(string key);

		bool TryGetDecompress(string key, out string value);

		Wrapper<string, string> TryGetStringDecompressAsync(string key);

		byte[] SetCompress(string key, byte[] value);

		Task<byte[]> SetCompressAsync(string key, byte[] value);

		byte[] SetCompress(string key, byte[] value, TimeSpan absoluteExpiration);

		Task<byte[]> SetCompressAsync(string key, byte[] value, TimeSpan absoluteExpiration);

		byte[] SetCompressSliding(string key, byte[] value, TimeSpan slidingExpiration);

		Task<byte[]> SetCompressSlidingAsync(string key, byte[] value, TimeSpan slidingExpiration);

		byte[] SetCompress(string key, string value);

		Task<byte[]> SetCompressAsync(string key, string value);

		byte[] SetCompress(string key, string value, TimeSpan absoluteExpiration);

		Task<byte[]> SetCompressAsync(string key, string value, TimeSpan absoluteExpiration);

		byte[] SetCompressSliding(string key, string value, TimeSpan slidingExpiration);

		Task<byte[]> SetCompressSlidingAsync(string key, string value, TimeSpan slidingExpiration);
	}
}