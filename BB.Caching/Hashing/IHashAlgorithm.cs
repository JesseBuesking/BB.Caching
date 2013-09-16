namespace BB.Caching.Hashing
{
    public interface IHashAlgorithm
    {
        uint ComputeInt(string value);

        uint ComputeInt(byte[] value);

        byte[] ComputeHash(string value);

        byte[] ComputeHash(byte[] value);
    }
}