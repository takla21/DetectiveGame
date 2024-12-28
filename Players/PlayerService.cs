using Detective.Level;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Detective.Players;

public interface IPlayerService : IDisposable
{
    public IEnumerable<Player> Players { get; }

    public event PlayerDeathEventHandler OnDeath;

    public void Initialize(int playerCount, string pathToNames, Clock clock);

    public void Update(float deltaT);
}

public class PlayerService : IPlayerService
{
    private readonly ILevelService _levelService;
    private readonly List<Player> _players;
    private readonly List<Player> _playersKilled;

    private readonly int _playerSize;

    public PlayerService(ILevelService levelService, int playerSize)
    {
        _players = new List<Player>();
        _playersKilled = new List<Player>();
        _playerSize = playerSize;

        _levelService = levelService;
    }

    public IEnumerable<Player> Players => _players;

    public event PlayerDeathEventHandler OnDeath;

    public void Initialize(int playerCount, string pathToNames, Clock clock)
    {
        var playerFactory = new PlayerFactory(Globals.Random, pathToNames, _levelService);

        var results = playerFactory.Create(playerCount, _playerSize, clock);

        foreach (var player in results)
        {
            player.OnPlaceEntered += OnPlaceEntered;
            player.OnPlaceExited += OnPlaceExited;
            player.OnDeath += OnPlayerDeath;

            _players.Add(player);
        }
    }

    public void Update(float deltaT)
    {
        // Clean up dead players
        foreach (var deadP in _playersKilled)
        {
            _players.Remove(deadP);
        }
        _playersKilled.Clear();

        // Move alive players
        foreach (var p in Players)
        {
            p.Move(deltaT);
        }
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
