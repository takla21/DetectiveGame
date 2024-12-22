using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Detective;

public class GameEngine
{
    private readonly List<Place> _places;
    private readonly List<Player> _players;

    public GameEngine()
    {
        _places = new List<Place>();
        _players = new List<Player>();
    }

    public IEnumerable<Place> Places => _places;

    public IEnumerable<Player> Players => _players;

    public void Init()
    {
        _places.Add(
            new Place(
                information: new PlaceInformation(
                    name: "Place 1",
                    position: new Vector2(0, 0),
                    size: new Vector2(500, 400),
                    entrancePosition: new Vector2(250, 400)
                ),
                color: new Vector3(255, 0, 0)
            )
        );
        _places.Add(
            new Place(
                information: new PlaceInformation(
                    name: "Place 2",
                    position: new Vector2(1420, 0),
                    size: new Vector2(500, 400),
                    entrancePosition: new Vector2(1655, 400)
                ),
                color: new Vector3(0, 255, 0)
            )
        );
        _places.Add(
            new Place(
                information: new PlaceInformation(
                    name: "Place 3",
                    position: new Vector2(0, 680),
                    size: new Vector2(500, 400),
                    entrancePosition: new Vector2(250, 660)
                ),
                color: new Vector3(0, 0, 255),
                isDarkTheme: true
            )
        );
        _places.Add(
            new Place(
                information: new PlaceInformation(
                    name: "Place 4",
                    position: new Vector2(1420, 680),
                    size: new Vector2(500, 400),
                    entrancePosition: new Vector2(1655, 660)
                ),
                color: new Vector3(255, 160, 160),
                isDarkTheme: false
            )
        );

        var information = new LevelInformation(
            _places.Select(x => x.Information),
            new Vector2(1920, 1080)
        );

        for (int i = 0; i < 5; i++)
        {
            var p = new Player("player" + (i + 1), 20);
            var role = new Innocent(new Vector2(1000, 500), information);

            p.AssignRole(role);

            p.OnPlaceEntered += OnPlaceEntered;
            p.OnPlaceExited += OnPlaceExited;
            
            _players.Add(p);
        }
    }

    private void OnPlaceEntered(object sender, PlaceUpdateArgs e)
    {
        var player = (Player)sender;
        var place = _places.First(x => x.Information == e.Place);
        place.AddPlayer(player);
    }

    private void OnPlaceExited(object sender, PlaceUpdateArgs e)
    {
        var player = (Player)sender;
        var place = _places.First(x => x.Information == e.Place);
        place.RemovePlayer(player);
    }

    public void Update(float deltaT)
    {
        foreach (var p in _players)
        {
            p.Move(deltaT);
        }
    }
}
