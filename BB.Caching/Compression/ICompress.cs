using System.Threading.Tasks;

namespace BB.Caching.Compression
{
	public interface ICompress
	{
		Task<byte[]> CompressAsync(byte[] value);

		Task<byte[]> CompressAsync(string value);

		byte[] Compress(byte[] value);

		byte[] Compress(string value);

		Task<byte[]> DecompressAsync(byte[] value);

		Task<string> DecompressStringAsync(byte[] value);

		byte[] Decompress(byte[] value);

		string DecompressString(byte[] value);
	}
}
