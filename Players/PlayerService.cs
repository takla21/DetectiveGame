using Detective.Level;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Detective.Players;

public interface IPlayerService : IDisposable
{
    public IEnumerable<Player> Players { get; }

    public event PlayerDeathEventHandler OnDeath;

    public void Initialize(int playerCount, string pathToNames, Clock clock);

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

    public void Initialize(int playerCount, string pathToNames, Clock clock)
    {
        var random = Globals.Random;

        var killer = random.Next(playerCount);

        var availableNames = new List<string>();
        using (var stream = Microsoft.Xna.Framework.TitleContainer.OpenStream(pathToNames))
        {
            using (var reader = new StreamReader(stream))
            {
                availableNames.AddRange(reader.ReadToEnd().Split("\r\n"));
            }
        }

        for (int i = 0; i < playerCount; i++)
        {
            var nameChoice = random.Next(availableNames.Count);

            var p = new Player(
                new PlayerProfile(
                    availableNames[nameChoice],
                    random.Next(18, 99)
                ), 
                PlayerSize
            );

            availableNames.RemoveAt(nameChoice);

            PlayerRoleBase role;

            var schedule = new UnemployedSchedule(_levelService, clock);

            if (i == killer)
            {
                role = new Killer(p.Id, new Vector2(1000, 500), _levelService, schedule);
            }
            else
            {
                role = new Innocent(p.Name, new Vector2(1000, 500), schedule);
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
        _levelService.EnterPlayer(player, e.Place);
    }

    private void OnPlaceExited(object sender, PlaceUpdateArgs e)
    {
        var player = (Player)sender;
        RemovePlayerFromPlace(player, e.Place);
    }

    private void RemovePlayerFromPlace(Player player, PlaceInformation placeInformation)
    {
        _levelService.RemovePlayer(player, placeInformation);
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
