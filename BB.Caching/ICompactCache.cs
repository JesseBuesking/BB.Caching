using System;
using System.Threading.Tasks;

namespace BB.Caching
{
	public interface ICompactCache
	{
		bool TryGetDecompact<TObject>(string key, out TObject value);

		Wrapper<TObject, TObject> TryGetDecompactAsync<TObject>(string key);

		byte[] SetCompact<TObject>(string key, TObject value);

		Task<byte[]> SetCompressAsync(string key, byte[] value);

		byte[] SetCompact<TObject>(string key, TObject value, TimeSpan absoluteExpiration);

		Task<byte[]> SetCompressAsync(string key, byte[] value, TimeSpan absoluteExpiration);

		byte[] SetCompactSliding<TObject>(string key, TObject value, TimeSpan slidingExpiration);

		Task<byte[]> SetCompressSlidingAsync(string key, byte[] value, TimeSpan slidingExpiration);
	}
}