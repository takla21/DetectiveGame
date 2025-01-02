using Detective.Configuration;
using Detective.Players;
using Detective.Utils;
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

    public Vector2 PickPointInLevel();

    public PlaceInformation PickPlace();

    public void EnterPlayer(Player player, PlaceInformation placeInformation);

    public void RemovePlayer(Player player, PlaceInformation placeInformation);
}

public record LevelChoice(Vector2 SelectedPoint, PlaceInformation SelectedPlace);

public class LevelService : ILevelService
{
    private readonly ScreenConfiguration _screenConfiguration;
    private readonly PlayerConfiguration _playerConfiguration;
    private readonly IRandom _random;

    private readonly List<Place> _places;

    public LevelService(ScreenConfiguration screenConfiguration, PlayerConfiguration playerConfiguration, IRandom random)
    {
        _screenConfiguration = screenConfiguration;
        _playerConfiguration = playerConfiguration;
        _random = random;

        _places = new List<Place>();
    }

    public void Initialize()
    {
        var placeSize = new Vector2(500, 400);

        _places.Add(
            new Place(
                information: new PlaceInformation(
                    type: PlaceType.Houses,
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
                    position: new Vector2(_screenConfiguration.Width - placeSize.X, 0),
                    size: placeSize,
                    entrancePosition: new Vector2((int)(_screenConfiguration.Width - placeSize.X * 0.5), placeSize.Y)
                ),
                color: new Vector3(0, 1, 0)
            )
        );
        _places.Add(
            new Place(
                information: new PlaceInformation(
                    name: "Place 3",
                    position: new Vector2(0, _screenConfiguration.Height - placeSize.Y),
                    size: placeSize,
                    entrancePosition: new Vector2(250, _screenConfiguration.Height - placeSize.Y - _playerConfiguration.PlayerSize)
                ),
                color: new Vector3(0, 0, 1),
                isDarkTheme: true
            )
        );
        _places.Add(
            new Place(
                information: new PlaceInformation(
                    type: PlaceType.Prison,
                    position: new Vector2(_screenConfiguration.Width - placeSize.X, _screenConfiguration.Height - placeSize.Y),
                    size: placeSize,
                    entrancePosition: new Vector2((int)(_screenConfiguration.Width - placeSize.X * 0.5), _screenConfiguration.Height - placeSize.Y - _playerConfiguration.PlayerSize)
                ),
                color: new Vector3(179 / 255.0f, 179 / 255.0f, 179 / 255.0f),
                isDarkTheme: false
            )
        );

        Information = new LevelInformation(
            _places.Select(x => x.Information),
            new Vector2(_screenConfiguration.Width, _screenConfiguration.Height),
            _playerConfiguration.PlayerSize
        );
    }

    public IEnumerable<Place> Places => _places;

    public LevelInformation Information { get; private set; }

    public Dictionary<PlaceInformation, IEnumerable<Player>> PlacesOccupancy => _places
        .ToDictionary(x => x.Information, x => x.PlayersInside);

    public LevelChoice PickPointOrPlace()
    {
        var draw = _random.Next(2);
        if (draw == 1)
        {
            var selectedPlace = PickPlace();
            return new LevelChoice(selectedPlace.EntrancePosition, selectedPlace);
        }
        else
        {
            var selectedPoint = PickPointInLevel();
            return new LevelChoice(selectedPoint, null);
        }
    }

    public Vector2 PickPointInLevel()
    {
        var selectedPoint = default(Vector2);
        do
        {
            selectedPoint = new Vector2(_random.Next(_screenConfiguration.Width), _random.Next(_screenConfiguration.Height));
        } while (Information.InvalidPositions.Contains(selectedPoint));

        return selectedPoint;
    }

    public PlaceInformation PickPlace()
    {
        var draw = _random.Next(_places.Count);
        var place = _places[draw];
        return place.Information;
    }

    public void EnterPlayer(Player player, PlaceInformation placeInformation)
    {
        var place = _places.Find(x => x.Information == placeInformation);
        place.AddPlayer(player);
    }

    public void RemovePlayer(Player player, PlaceInformation placeInformation)
    {
        var place = _places.Find(x => x.Information == placeInformation);
        place.RemovePlayer(player);
    }
}
