using System.Security.Cryptography;
using Yals.Crypto;

namespace Yals.Stats;

class StatsKeyGenerator(IRandomNumberGenerator rng) : IStatsKeyGenerator
{
    const int KeyByteLength = 32;
    const int SaltByteLength = 16;

    public GeneratedStatsKey Generate()
    {
        var key = rng.GetBytes(KeyByteLength);
        var salt = rng.GetBytes(SaltByteLength);

        var concatentated = new byte[KeyByteLength + SaltByteLength];

        Buffer.BlockCopy(key, 0, concatentated, 0, KeyByteLength);
        Buffer.BlockCopy(salt, 0, concatentated, KeyByteLength, SaltByteLength);

        byte[] hashedKey;

        try
        {
            hashedKey = SHA256.HashData(concatentated);
        }
        catch (Exception e)
        {
            throw new StatsKeyHashException(e.Message);
        }

        if (hashedKey is null)
        {
            throw new StatsKeyHashException("Stats hashed key was not computed");
        }

        return new GeneratedStatsKey
        {
            RawKey = Convert.ToHexString(key),
            HashedKey = hashedKey,
            Salt = salt,
        };
    }
}
