namespace Memstate.Core
{
    public interface IEncryption
    {
        byte[] Encrypt(byte[] bytes);
        byte[] Decrypt(byte[] bytes);
    }
}