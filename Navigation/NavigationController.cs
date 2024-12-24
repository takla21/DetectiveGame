using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Detective.Navigation;

public class NavigationController
{
    private ContentManager _contentManager;
    private GraphicsDevice _graphicsDevice;

    public Stack<IScreen> NavigationStack { get; }

    public Stack<IModalScreen> ModalStack { get; }

    private enum NavigationOperation
    {
        Add,
        Remove,
        Clear,
    }

    private readonly Queue<(NavigationOperation Type, IScreen Screen)> _navigationStackOperations;
    private readonly Queue<(NavigationOperation Type, IModalScreen Modal)> _modalStackOperations;

    public NavigationController()
    {
        ModalStack = new Stack<IModalScreen>();
        NavigationStack = new Stack<IScreen>();

        _navigationStackOperations = new Queue<(NavigationOperation, IScreen)>();
        _modalStackOperations = new Queue<(NavigationOperation, IModalScreen)>();
    }

    public void Load(ContentManager contentManager, GraphicsDevice graphicsDevice)
    {
        _contentManager = contentManager;
        _graphicsDevice = graphicsDevice;
    }

    public void Update(float deltaT, MouseState mouseState)
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

        // Update modals
        foreach (var modal in ModalStack)
        {
            modal.Update(deltaT, mouseState);
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

        // Update screens.
        foreach (var screen in NavigationStack)
        {
            screen.Update(deltaT, mouseState);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (ModalStack.Count > 0)
        {
            var currentModal = ModalStack.Peek();
            currentModal.Draw(spriteBatch);

            if (currentModal.IsFullScreen)
            {
                return;
            }
        }

        NavigationStack.Peek().Draw(spriteBatch);
    }

    public void NavigateTo(IScreen screen)
    {
        screen.LoadContent(_contentManager, _graphicsDevice);

        // We cannot manipulate stacks here, since it's still being executed in the update method. Maybe I could use multi-threading in the future.
        _navigationStackOperations.Enqueue((NavigationOperation.Add, screen));
    }

    public void NavigateAndClear(IScreen screen)
    {
        // We cannot manipulate stacks here, since it's still being executed in the update method. Maybe I could use multi-threading in the future.
        _navigationStackOperations.Enqueue((NavigationOperation.Clear, null));

        NavigateTo(screen);
    }

    public void NavigateBack()
    {
        // We cannot manipulate stacks here, since it's still being executed in the update method. Maybe I could use multi-threading in the future.
        _navigationStackOperations.Enqueue((NavigationOperation.Remove, null));
    }

    public void ShowModal(IModalScreen screen)
    {
        screen.LoadContent(_contentManager, _graphicsDevice);

        // We cannot manipulate stacks here, since it's still being executed in the update method. Maybe I could use multi-threading in the future.
        _modalStackOperations.Enqueue((NavigationOperation.Add, screen));
    }

    public void DismissModal()
    {
        // We cannot manipulate stacks here, since it's still being executed in the update method. Maybe I could use multi-threading in the future.
        _modalStackOperations.Enqueue((NavigationOperation.Remove, null));
    }
}
