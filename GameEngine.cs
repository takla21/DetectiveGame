using Detective.Level;
using Detective.Players;
using Detective.UI;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;

namespace Detective;

public class GameEngine : IDisposable
{
    private const string NameFilesName = "../names.txt";

    private readonly IPlayerService _playerService;
    private readonly ILevelService _levelService;

    public GameEngine(int screenWidth, int screenHeight)
    {
        NotificationController = new NotificationController();
        _levelService = new LevelService(screenWidth, screenHeight);
        _playerService = new PlayerService(_levelService);

        _playerService.OnDeath -= OnDeath;
        _playerService.OnDeath += OnDeath;
    }

    public IEnumerable<Place> Places => _levelService.Places;

    public IEnumerable<Player> Players => _playerService.Players;

    public NotificationController NotificationController { get; }

    public void Init(ContentManager content, Clock clock)
    {
        _levelService.Initialize();

        var filePath = Path.Combine(content.RootDirectory, NameFilesName);
        _playerService.Initialize(playerCount: 10, filePath, clock);
    }

    private void OnDeath(object sender, PlayerDeathEventArgs e)
    {
        var player = (Player)sender;

        NotificationController.Enqueue($"{player.Name} has been killed.", 5);
    }

    public void Update(float deltaT)
    {
        NotificationController.GetCurrentNotification(deltaT);

        _playerService.Update();

        // Move alive players
        foreach (var p in _playerService.Players)
        {
            p.Move(deltaT);
        }
    }

    public void Dispose()
    {
        _playerService.OnDeath -= OnDeath;
        _playerService.Dispose();
    }
}
