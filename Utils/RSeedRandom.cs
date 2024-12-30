using System;
using System.Runtime.InteropServices;

namespace Detective.Utils;

public interface IRandomFactory
{
    public int Seed { get; }

    public Random GenerateRandom(int? injectedSeed = null);
}

public class RSeedRandom : IRandomFactory
{
    // Import the function from the native DLL
    [DllImport("TrulyRandom.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetRdseed(out ulong value);

    public int Seed { get; private set; }

    public Random GenerateRandom(int? injectedSeed = null)
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

        return new Random(Seed);
    }
}
