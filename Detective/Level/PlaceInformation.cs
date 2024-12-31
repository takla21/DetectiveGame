using System;
using System.Numerics;

namespace Detective.Level;

public record PlaceInformation
{
    public PlaceInformation(string name, Vector2 position, Vector2 size, Vector2 entrancePosition)
    {
        Name = name;
        Type = PlaceType.Other;
        Position = position;
        Size = size;
        EntrancePosition = entrancePosition;
    }
    public PlaceInformation(PlaceType type, Vector2 position, Vector2 size, Vector2 entrancePosition)
    {
        Type = type;
        Name = Enum.GetName(type);
        Position = position;
        Size = size;
        EntrancePosition = entrancePosition;
    }

    public string Name { get; init; }

    public PlaceType Type { get; init; }

    public Vector2 Position { get; init; }

    public Vector2 Size { get; init; }

    public Vector2 EntrancePosition { get; init; }
}

public enum PlaceType
{
    Houses,
    Prison,
    Other
}