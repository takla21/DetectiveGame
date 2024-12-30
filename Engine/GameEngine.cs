using Detective.Level;
using Detective.Players;
using Detective.UI;
using System;

namespace Detective.Engine;

public interface IGameEngine
{
    public void Init();

    public void Update(float deltaT);
}

public class GameEngine : IGameEngine, IDisposable
{
    private readonly IPlayerService _playerService;
    private readonly ILevelService _levelService;
    private readonly INotificationService _notificationService;

    public GameEngine(IPlayerService playerService, ILevelService levelService, INotificationService notificationService)
    {
        _playerService = playerService;
        _levelService = levelService;
        _notificationService = notificationService;

        _playerService.OnDeath -= OnDeath;
        _playerService.OnDeath += OnDeath;
    }

    public void Init()
    {
        _levelService.Initialize();

        _playerService.Initialize(playerCount: 10);
    }

    private void OnDeath(object sender, PlayerDeathEventArgs e)
    {
        var player = (Player)sender;

        _notificationService.Enqueue($"{player.Name} has been killed.", 5);
    }

    public void Update(float deltaT)
    {
        _notificationService.Update(deltaT);

        _playerService.Update(deltaT);
    }

    public void Dispose()
    {
        _playerService.OnDeath -= OnDeath;
    }
}
