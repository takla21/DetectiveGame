using Microsoft.Extensions.DependencyInjection;
using System;

namespace Detective.Engine;

public interface IGameState
{
    public GameStateType CurrentState { get; }

    public IServiceProvider CurrentServiceProvider { get; }

    public void LoadServiceProvider(IServiceProvider serviceProvider);

    public void StartGame();

    public void EndGame(bool hasUserWon);

    public void Reset();
}

public enum GameStateType
{
    None,
    Started,
    Lost,
    Won
}

public class GameState : IGameState, IDisposable
{
    private IServiceProvider _defaultServiceProvider;
    private IDisposable _currentGameDisposable;

    public GameState()
    {
        CurrentState = GameStateType.None;
    }

    public GameStateType CurrentState { get; private set; }

    public IServiceProvider CurrentServiceProvider { get; private set; }

    public void LoadServiceProvider(IServiceProvider serviceProvider)
    {
        _defaultServiceProvider = serviceProvider;
        CurrentServiceProvider = serviceProvider;
    }

    public void StartGame()
    {
        CurrentState = GameStateType.Started;

        var scope = _defaultServiceProvider.CreateScope();
        CurrentServiceProvider = scope.ServiceProvider;

        _currentGameDisposable = scope;
    }

    public void EndGame(bool hasUserWon)
    {
        CurrentState = hasUserWon ? GameStateType.Won : GameStateType.Lost;

        CurrentServiceProvider = _defaultServiceProvider;

        _currentGameDisposable.Dispose();
        _currentGameDisposable = null;
    }

    public void Reset()
    {
        CurrentState = GameStateType.None;
    }

    public void Dispose()
    {
        _currentGameDisposable?.Dispose();
    }
}
