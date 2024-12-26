using Detective.Level;
using System.Collections.Generic;
using System.Numerics;

namespace Detective;

public record LevelInformation
{
    public LevelInformation(IEnumerable<PlaceInformation> placeInformation, Vector2 levelSize)
    {
        PlacesInformation = placeInformation;
        LevelSize = levelSize;
        InvalidPositions = GenerateInvalidPositions(placeInformation);
    }

    private ISet<Vector2> GenerateInvalidPositions(IEnumerable<PlaceInformation> places)
    {
        var set = new HashSet<Vector2>();

        foreach (PlaceInformation place in places)
        {
            for (float i = place.Position.X; i < place.Position.X + place.Size.X; i++)
            {
                for (float j = place.Position.Y; j < place.Position.Y + place.Size.Y; j++)
                {
                    set.Add(new Vector2(i, j));
                }
            }
        }

        return set;
    }

    public IEnumerable<PlaceInformation> PlacesInformation { get; init; }

    public Vector2 LevelSize { get; init; }

    public ISet<Vector2> InvalidPositions { get; init; }
}
