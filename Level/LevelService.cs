using Detective.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Detective.Level;

public interface ILevelService
{
    public IEnumerable<Place> Places { get; }

    public Dictionary<PlaceInformation, IEnumerable<Player>> PlacesOccupancy { get; }

    public LevelInformation Information { get; }

    public void Initialize();

    public LevelChoice PickPointOrPlace();

    public Vector2 PickPointInLevel(Random random = null);

    public PlaceInformation PickPlace(Random random = null);
}

public record LevelChoice(Vector2 SelectedPoint, PlaceInformation SelectedPlace);

public class LevelService : ILevelService
{
    private readonly List<Place> _places;

    private readonly int _width;
    private readonly int _height;

    public LevelService(int width, int height)
    {
        _width = width;
        _height = height;

        _places = new List<Place>();
    }

    public void Initialize()
    {
        var placeSize = new Vector2(500, 400);

        _places.Add(
            new Place(
                information: new PlaceInformation(
                    name: "Place 1",
                    position: new Vector2(0, 0),
                    size: placeSize,
                    entrancePosition: new Vector2(250, placeSize.Y)
                ),
                color: new Vector3(1, 0, 0)
            )
        );
        _places.Add(
            new Place(
                information: new PlaceInformation(
                    name: "Place 2",
                    position: new Vector2(_width - placeSize.X, 0),
                    size: placeSize,
                    entrancePosition: new Vector2((int)(_width - placeSize.X * 0.5), placeSize.Y)
                ),
                color: new Vector3(0, 1, 0)
            )
        );
        _places.Add(
            new Place(
                information: new PlaceInformation(
                    name: "Place 3",
                    position: new Vector2(0, _height - placeSize.Y),
                    size: placeSize,
                    entrancePosition: new Vector2(250, _height - placeSize.Y - 20) // playerSize
                ),
                color: new Vector3(0, 0, 1),
                isDarkTheme: true
            )
        );
        _places.Add(
            new Place(
                information: new PlaceInformation(
                    name: "Place 4",
                    position: new Vector2(_width - placeSize.X, _height - placeSize.Y),
                    size: placeSize,
                    entrancePosition: new Vector2((int)(_width - placeSize.X * 0.5), _height - placeSize.Y - 20) // playerSize
                ),
                color: new Vector3(179 / 255.0f, 179 / 255.0f, 179 / 255.0f),
                isDarkTheme: false
            )
        );

        Information = new LevelInformation(
            _places.Select(x => x.Information),
            new Vector2(_width, _height)
        );
    }

    public IEnumerable<Place> Places => _places;

    public LevelInformation Information { get; private set; }

    public Dictionary<PlaceInformation, IEnumerable<Player>> PlacesOccupancy => _places
        .ToDictionary(x => x.Information, x => x.PlayersInside);

    public LevelChoice PickPointOrPlace()
    {
        var rand = new Random();
        var draw = rand.Next(2);
        if (draw == 1)
        {
            var selectedPlace = PickPlace(rand);
            return new LevelChoice(selectedPlace.EntrancePosition, selectedPlace);
        }
        else
        {
            var selectedPoint = PickPointInLevel(rand);
            return new LevelChoice(selectedPoint, null);
        }
    }

    public Vector2 PickPointInLevel(Random random = null)
    {
        var rand = random ?? new Random();
        var selectedPoint = default(Vector2);
        do
        {
            selectedPoint = new Vector2(rand.Next(_width), rand.Next(_height));
        } while (Information.InvalidPositions.Contains(selectedPoint));

        return selectedPoint;
    }

    public PlaceInformation PickPlace(Random random = null)
    {
        var rand = random ?? new Random();
        var draw = rand.Next(_places.Count);
        var place = _places[draw];
        return place.Information;
    }
}
