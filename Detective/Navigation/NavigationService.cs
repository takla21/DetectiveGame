using Detective.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Detective.Navigation;

public interface INavigationService
{
    public Stack<IScreen> NavigationStack { get; }

    public Stack<IModalScreen> ModalStack { get; }

    public void Update();

    public void NavigateTo<TScreen>()
        where TScreen : IScreen;

    public void NavigateTo(IScreen screen);

    public void NavigateAndClear<TScreen>()
        where TScreen : IScreen;

    public void NavigateBack();

    public void ShowModal<TScreen>()
        where TScreen : IModalScreen;

    public void DismissModal();

    public void NavigateBackOrDismissModal();
}

public class NavigationService : INavigationService
{
    private readonly IScreenLoader _screenLoader;
    private readonly IGameState _gameState;

    private enum NavigationOperation
    {
        Add,
        Remove,
        Clear,
    }

    private readonly Queue<(NavigationOperation Type, IScreen Screen)> _navigationStackOperations;
    private readonly Queue<(NavigationOperation Type, IModalScreen Modal)> _modalStackOperations;

    public NavigationService(IScreenLoader screenLoader, IGameState gameState)
    {
        _screenLoader = screenLoader;
        _gameState = gameState;

        ModalStack = new Stack<IModalScreen>();
        NavigationStack = new Stack<IScreen>();

        _navigationStackOperations = new Queue<(NavigationOperation, IScreen)>();
        _modalStackOperations = new Queue<(NavigationOperation, IModalScreen)>();
    }

    public Stack<IScreen> NavigationStack { get; }

    public Stack<IModalScreen> ModalStack { get; }

    public void Update()
    {
        // Check if modals has been pushed or popped.
        while (_modalStackOperations.Count > 0)
        {
            var operation = _modalStackOperations.Dequeue();
            switch (operation.Type)
            {
                case NavigationOperation.Add:
                    ModalStack.Push(operation.Modal);
                    break;
                case NavigationOperation.Remove:
                case NavigationOperation.Clear:
                    var previousModal = ModalStack.Pop();
                    previousModal.Dispose();
                    break;
                default:
                    throw new InvalidOperationException("Unknown navigation operation " + operation.Type);
            }
        }

        // Check if screen from the main stack has been added or removed.
        while (_navigationStackOperations.Count > 0)
        {
            var operation = _navigationStackOperations.Dequeue();
            switch (operation.Type)
            {
                case NavigationOperation.Add:
                    NavigationStack.Push(operation.Screen);
                    break;
                case NavigationOperation.Remove:
                    var previousScreen = NavigationStack.Pop();
                    previousScreen.Dispose();
                    break;
                case NavigationOperation.Clear:
                    foreach (var oldScreen in NavigationStack)
                    {
                        oldScreen.Dispose();
                    }

                    NavigationStack.Clear();
                    break;
                default:
                    throw new InvalidOperationException("Unknown navigation operation " + operation.Type);
            }
        }
    }

    public void NavigateTo<TScreen>()
        where TScreen : IScreen
    {
        var screen = _gameState.CurrentServiceProvider.GetRequiredService<TScreen>();

        NavigateTo(screen);
    }

    public void NavigateTo(IScreen screen)
    {
        _screenLoader.Load(screen);

        // We cannot manipulate stacks here, since it's still being executed in the update method. Maybe I could use multi-threading in the future.
        _navigationStackOperations.Enqueue((NavigationOperation.Add, screen));
    }

    public void NavigateAndClear<TScreen>() where TScreen : IScreen
    {
        var screen = _gameState.CurrentServiceProvider.GetRequiredService<TScreen>();

        // We cannot manipulate stacks here, since it's still being executed in the update method. Maybe I could use multi-threading in the future.
        _navigationStackOperations.Enqueue((NavigationOperation.Clear, null));

        NavigateTo(screen);
    }

    public void NavigateBack()
    {
        // We cannot manipulate stacks here, since it's still being executed in the update method. Maybe I could use multi-threading in the future.
        _navigationStackOperations.Enqueue((NavigationOperation.Remove, null));
    }

    public void ShowModal<TScreen>() where TScreen : IModalScreen
    {
        var modal = _gameState.CurrentServiceProvider.GetRequiredService<TScreen>();

        _screenLoader.Load(modal);

        // We cannot manipulate stacks here, since it's still being executed in the update method. Maybe I could use multi-threading in the future.
        _modalStackOperations.Enqueue((NavigationOperation.Add, modal));
    }

    public void DismissModal()
    {
        // We cannot manipulate stacks here, since it's still being executed in the update method. Maybe I could use multi-threading in the future.
        _modalStackOperations.Enqueue((NavigationOperation.Remove, null));
    }

    public void NavigateBackOrDismissModal()
    {
        if (ModalStack.Count > 0)
        {
            DismissModal();
        }
        else
        {
            NavigateBack();
        }
    }
}
