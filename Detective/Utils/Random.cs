using System;

namespace Detective.Utils;

public interface IRandom
{
    public int Seed { get; }

    int Next(int maxValue);

    int Next(int minValue, int maxValue);

    double NextDouble();
}

public class RandomWrapper : IRandom
{
    private readonly Random _random;

    public RandomWrapper(int seed)
    {
        Seed = seed;

        _random = new Random(seed);
    }

    public int Seed { get; }

    public int Next(int maxValue) => _random.Next(maxValue);

    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    public double NextDouble() => _random.NextDouble();
}