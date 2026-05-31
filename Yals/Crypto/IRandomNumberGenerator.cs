namespace Yals.Crypto;

interface IRandomNumberGenerator
{
    byte[] GetBytes(int count);
}
