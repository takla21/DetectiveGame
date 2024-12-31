using Detective.Level;
using System.Collections.Generic;
using System.Numerics;

namespace Detective;

public record LevelInformation
{
    public LevelInformation(IEnumerable<PlaceInformation> placeInformation, Vector2 levelSize, int playerSize)
    {
        PlacesInformation = placeInformation;
        LevelSize = levelSize;
        InvalidPositions = GenerateInvalidPositions(placeInformation, playerSize);
    }

    private ISet<Vector2> GenerateInvalidPositions(IEnumerable<PlaceInformation> places, int playerSize)
    {
        var set = new HashSet<Vector2>();

        foreach (PlaceInformation place in places)
        {
            // Only decreasing by playerSize to take into account drawing top/left.
            for (float i = place.Position.X - playerSize; i < place.Position.X + place.Size.X; i++)
            {
                for (float j = place.Position.Y - playerSize; j < place.Position.Y + place.Size.Y; j++)
                {
                    // Make sure entrances are not excluded from a* algorithm.
                    if (place.EntrancePosition.X == i && place.EntrancePosition.Y == j)
                    {
                        continue;
                    }

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
