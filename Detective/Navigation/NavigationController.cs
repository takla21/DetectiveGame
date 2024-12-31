using Detective.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Detective.Navigation;

public class NavigationController
{
    private readonly INavigationService _navigationService;
    private readonly ScreenConfiguration _screenConfiguration;

    private Texture2D _defaultTexture;

    public int ScreenWidth { get; }

    public int ScreenHeight { get; }

    public NavigationController(ScreenConfiguration screenConfiguration, INavigationService navigationService)
    {
        _screenConfiguration = screenConfiguration;
        _navigationService = navigationService;
    }

    public void Load(ContentManager contentManager, GraphicsDevice graphicsDevice)
    {
        _defaultTexture = new Texture2D(graphicsDevice, 1, 1);
        _defaultTexture.SetData([Color.White]);
    }

    public void Update(float deltaT, MouseState mouseState)
    {
        _navigationService.Update();

        // Update modals
        foreach (var modal in _navigationService.ModalStack)
        {
            modal.Update(deltaT, mouseState);
        }

        // Update screens.
        foreach (var screen in _navigationService.NavigationStack)
        {
            screen.Update(deltaT, mouseState);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var currentModal = _navigationService.ModalStack.Count > 0 ? _navigationService.ModalStack.Peek() : default;
        if (!currentModal?.IsFullScreen ?? true)
        {
            _navigationService.NavigationStack.Peek().Draw(spriteBatch);
        }

        if (currentModal is not null)
        {
            if (!currentModal.IsFullScreen)
            {
                // Draw modal overlay
                spriteBatch.Draw(_defaultTexture, new Rectangle(x: 0, y: 0, width: _screenConfiguration.Width, height: _screenConfiguration.Height), new Color(0.25f, 0.25f, 0.25f, 0.5f));
            }

            currentModal.Draw(spriteBatch);
        }
    }
}
