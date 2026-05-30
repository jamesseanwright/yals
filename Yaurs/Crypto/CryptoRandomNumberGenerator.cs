using System.Security.Cryptography;

namespace Yaurs.Crypto;

class CryptoRandomNumberGenerator : IRandomNumberGenerator
{
    public byte[] GetBytes(int count)
    {
        return RandomNumberGenerator.GetBytes(count);
    }
}
