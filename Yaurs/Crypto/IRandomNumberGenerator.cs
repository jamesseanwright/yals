namespace Yaurs.Crypto;

interface IRandomNumberGenerator
{
    byte[] GetBytes(int count);
}
