using System.Security.Cryptography;

namespace Yals.Crypto;

class CryptoRandomNumberGenerator : IRandomNumberGenerator
{
    public byte[] GetBytes(int count)
    {
        return RandomNumberGenerator.GetBytes(count);
    }
}
