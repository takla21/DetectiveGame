using Detective.Utils;
using System;

namespace Detective;

// TODO : use microsoft IoC instead
public static class Globals
{
    // Set injectedSeed to remove randomness (for debugging purposes)
    public static IRandomFactory RandomFactory { get; } = new RSeedRandom();

    public static Random Random => RandomFactory.Random;
}
