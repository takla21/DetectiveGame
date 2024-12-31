using Detective.Configuration;
using Detective.Level;
using System;
using System.Collections.Generic;

namespace Detective.Players;

public interface IPlayerService
{
    public IEnumerable<Player> Players { get; }

    public event PlayerDeathEventHandler OnDeath;

    public void Initialize(int playerCount);

    public void Update(float deltaT);
}

public class PlayerService : IPlayerService, IDisposable
{
    private readonly ILevelService _levelService;
    private readonly IPlayerFactory _playerFactory;
    private readonly PlayerConfiguration _playerConfiguration;

    private readonly List<Player> _players;
    private readonly List<Player> _playersKilled;

    public PlayerService(ILevelService levelService, PlayerConfiguration playerConfiguration, IPlayerFactory playerFactory)
    {
        _levelService = levelService;
        _playerConfiguration = playerConfiguration;
        _playerFactory = playerFactory;

        _players = new List<Player>();
        _playersKilled = new List<Player>();
    }

    public IEnumerable<Player> Players => _players;

    public event PlayerDeathEventHandler OnDeath;

    public void Initialize(int playerCount)
    {
        var results = _playerFactory.Create(playerCount);

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
