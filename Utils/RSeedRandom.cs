using System;
using System.Runtime.InteropServices;

namespace Detective.Utils;

public interface IRandomFactory
{
    public int Seed { get; }

    public Random Random { get; }
}

public class RSeedRandom : IRandomFactory
{
    // Import the function from the native DLL
    [DllImport("TrulyRandom.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetRdseed(out ulong value);

    public RSeedRandom(int? injectedSeed = null)
    {
        if (injectedSeed is int seed)
        {
            Seed = seed;
        }
        else
        {
            GetRdseed(out var rdSeed);
            Seed = (int)rdSeed;
        }

        Random = new Random(Seed);
    }

    public Random Random { get; }

    public int Seed { get; }
}
