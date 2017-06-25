namespace Memstate.Core
{
    public interface ICompressor
    {
        byte[] Compress(byte[] bytes);
        byte[] Decompress(byte[] bytes);
    }   
}