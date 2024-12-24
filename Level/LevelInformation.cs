using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Detective;

public record LevelInformation
{
    private readonly int _placesCount;

    public LevelInformation(IEnumerable<PlaceInformation> placeInformation, Vector2 levelSize)
    {
        PlacesInformation = placeInformation;
        _placesCount = placeInformation.ToArray().Length;
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

    public (Vector2 selectedPoint, PlaceInformation selectedPlace) PickPointOrPlace()
    {
        var rand = new Random();
        var draw = rand.Next(_placesCount * 2);
        if (draw >= _placesCount)
        {
            var selectedPoint = default(Vector2);
            do
            {
                selectedPoint = new Vector2(rand.Next((int)LevelSize.X), rand.Next((int)LevelSize.Y));
            } while (InvalidPositions.Contains(selectedPoint));

            return (selectedPoint, selectedPlace: null);
        }
        else
        {
            var place = PlacesInformation.ToArray()[draw];
            return (selectedPoint: place.EntrancePosition, selectedPlace: place);
        }
    }
}
