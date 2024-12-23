using Detective.UI;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Detective;

public class GameEngine
{
    private readonly List<Place> _places;
    private readonly List<Player> _players;
    private readonly List<Player> _playersKilled;
    private readonly LevelTracker _levelTracker;

    public GameEngine()
    {
        _places = new List<Place>();
        _players = new List<Player>();
        _playersKilled = new List<Player>();
        _levelTracker = new LevelTracker();

        NotificationController = new NotificationController();
    }

    public IEnumerable<Place> Places => _places;

    public IEnumerable<Player> Players => _players;
    
    public NotificationController NotificationController { get; }

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
                color: new Vector3(1, 0, 0)
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
                color: new Vector3(0, 1, 0)
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
                color: new Vector3(0, 0, 1),
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
                color: new Vector3(179 / 255.0f, 179 / 255.0f, 179 / 255.0f),
                isDarkTheme: false
            )
        );

        var information = new LevelInformation(
            _places.Select(x => x.Information),
            new Vector2(1920, 1080)
        );

        for (int i = 0; i < 10; i++)
        {
            var p = new Player("player" + (i + 1), 20);

            PlayerRoleBase role;
            if (i == 0)
            {
                role = new Killer(p.Id, new Vector2(1000, 500), information, _levelTracker);
            }
            else
            {
                role = new Innocent(p.Id, new Vector2(1000, 500), information);
            }

            p.AssignRole(role);

            p.OnPlaceEntered += OnPlaceEntered;
            p.OnPlaceExited += OnPlaceExited;
            p.OnDeath += OnDeath;

            _players.Add(p);
        }
    }

    private void OnDeath(object sender, PlayerDeathEventArgs e)
    {
        var player = (Player)sender;

        player.OnPlaceEntered -= OnPlaceEntered;
        player.OnPlaceExited -= OnPlaceExited;
        player.OnDeath -= OnDeath;

        RemovePlayerFromPlace(player, e.Place);

        _playersKilled.Add(player);

        NotificationController.Enqueue($"{player.Name} has been killed.", 5);
    }

    private void OnPlaceEntered(object sender, PlaceUpdateArgs e)
    {
        var player = (Player)sender;
        var place = _places.First(x => x.Information == e.Place);
        place.AddPlayer(player);

        _levelTracker.Update(e.Place, place.PlayersInside);
    }

    private void OnPlaceExited(object sender, PlaceUpdateArgs e)
    {
        var player = (Player)sender;

        RemovePlayerFromPlace(player, e.Place);
    }

    private void RemovePlayerFromPlace(Player player, PlaceInformation placeInformation)
    {
        var place = _places.First(x => x.Information == placeInformation);
        place.RemovePlayer(player);

        _levelTracker.Update(placeInformation, place.PlayersInside);
    }

    public void Update(float deltaT)
    {
        NotificationController.GetCurrentNotification(deltaT);

        foreach (var deadP in _playersKilled)
        {
            _players.Remove(deadP);
            deadP.Dispose();
        }

        _playersKilled.Clear();

        foreach (var p in _players)
        {
            p.Move(deltaT);
        }
    }
}
