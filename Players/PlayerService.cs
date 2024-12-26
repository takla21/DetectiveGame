using Detective.Level;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Detective.Players;

public interface IPlayerService : IDisposable
{
    public IEnumerable<Player> Players { get; }

    public event PlayerDeathEventHandler OnDeath;

    public void Initialize(int playerCount, string pathToNames);

    public void Update();
}

public class PlayerService : IPlayerService
{
    private const int PlayerSize = 20;

    private readonly ILevelService _levelService;
    private readonly List<Player> _players;
    private readonly List<Player> _playersKilled;

    public PlayerService(ILevelService levelService)
    {
        _players = new List<Player>();
        _playersKilled = new List<Player>();

        _levelService = levelService;
    }

    public IEnumerable<Player> Players => _players;

    public event PlayerDeathEventHandler OnDeath;

    public void Initialize(int playerCount, string pathToNames)
    {
        var random = new Random();

        var killer = random.Next(playerCount);

        var availableNames = Array.Empty<string>();
        using (var stream = Microsoft.Xna.Framework.TitleContainer.OpenStream(pathToNames))
        {
            using (var reader = new StreamReader(stream))
            {
                availableNames = reader.ReadToEnd().Split("\r\n");
            }
        }

        for (int i = 0; i < playerCount; i++)
        {
            var p = new Player(
                new PlayerProfile(
                    availableNames[random.Next(availableNames.Length)],
                    random.Next(18, 99)
                ), 
                PlayerSize
            );

            PlayerRoleBase role;
            if (i == killer)
            {
                role = new Killer(p.Id, new Vector2(1000, 500), _levelService);
            }
            else
            {
                role = new Innocent(p.Id, new Vector2(1000, 500), _levelService);
            }

            p.OnPlaceEntered += OnPlaceEntered;
            p.OnPlaceExited += OnPlaceExited;
            p.OnDeath += OnPlayerDeath;

            p.AssignRole(role);

            _players.Add(p);
        }
    }

    public void Update()
    {
        // Clean up dead players
        foreach (var deadP in _playersKilled)
        {
            _players.Remove(deadP);
        }
        _playersKilled.Clear();
    }

    private void OnPlaceEntered(object sender, PlaceUpdateArgs e)
    {
        var player = (Player)sender;
        var place = _levelService.Places.First(x => x.Information == e.Place);
        place.AddPlayer(player);
    }

    private void OnPlaceExited(object sender, PlaceUpdateArgs e)
    {
        var player = (Player)sender;
        RemovePlayerFromPlace(player, e.Place);
    }

    private void RemovePlayerFromPlace(Player player, PlaceInformation placeInformation)
    {
        var place = _levelService.Places.First(x => x.Information == placeInformation);
        place.RemovePlayer(player);
    }

    private void OnPlayerDeath(object sender, PlayerDeathEventArgs e)
    {
        var player = (Player)sender;

        RemovePlayerFromPlace(player, e.Place);

        player.OnPlaceEntered -= OnPlaceEntered;
        player.OnPlaceExited -= OnPlaceExited;
        player.OnDeath -= OnDeath;

        player.Dispose();

        _playersKilled.Add(player);

        OnDeath?.Invoke(sender, e);
    }

    public void Dispose()
    {
        foreach (var player in _players)
        {
            player.OnPlaceEntered -= OnPlaceEntered;
            player.OnPlaceExited -= OnPlaceExited;
            player.OnDeath -= OnDeath;

            player.Dispose();
        }
    }
}
