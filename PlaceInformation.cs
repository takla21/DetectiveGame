using System.Numerics;

namespace Detective;

public record PlaceInformation
{
    public PlaceInformation(string name, Vector2 position, Vector2 size, Vector2 entrancePosition)
    {
        Name = name;
        Position = position;
        Size = size;
        EntrancePosition = entrancePosition;
    }

    public string Name { get; init; }

    public Vector2 Position { get; init; }

    public Vector2 Size { get; init; }

    public Vector2 EntrancePosition { get; init; }
}
