using System.Runtime.InteropServices;

namespace Detective.Utils;

public interface IRandomFactory
{
    public IRandom GenerateRandom(int? injectedSeed = null);
}

public class RSeedRandom : IRandomFactory
{
    // Import the function from the native DLL
    [DllImport("TrulyRandom.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetRdseed(out ulong value);

    public IRandom GenerateRandom(int? injectedSeed = null)
    {
        int actualSeed;
        if (injectedSeed is int seed)
        {
            actualSeed = seed;
        }
        else
        {
            GetRdseed(out var rdSeed);
            actualSeed = (int)rdSeed;
        }

        return new RandomWrapper(actualSeed);
    }
}
