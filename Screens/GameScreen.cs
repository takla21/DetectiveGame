using Detective.Navigation;
using Detective.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Detective.Screens;

public sealed class GameScreen : IScreen
{
    private const int Clock_Speed = 100;

    private readonly NavigationController _navigationController;
    private readonly GameEngine _engine;
    private readonly Clock _clock;

    private readonly int _screenWidth;
    private readonly int _screenHeight;

    private Texture2D _defaultTexture;
    private SpriteFont _font;
    private Hub _hub;

    public GameScreen(int screenWidth, int screenHeight, NavigationController navigationController)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _navigationController = navigationController;

        _engine = new GameEngine(screenWidth, screenHeight);
        _clock = new Clock();
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("default");

        _defaultTexture = new Texture2D(graphicsDevice, 1, 1);
        _defaultTexture.SetData([Color.White]);

        _hub = new Hub(_defaultTexture, new Vector2(_screenWidth * 0.33f, 0), new Vector2(_screenWidth * 0.33f, 50), new Color(new Vector4(0.25f, 0.25f, 0.25f, 0.75f)), _font);

        _hub.OnExpand -= OnExpand;
        _hub.OnExpand += OnExpand;

        _engine.Init(content);
    }

    private void OnExpand()
    {
        // TODO: Open modal with information
    }

    public void Update(float deltaT, MouseState mouseState)
    {
        _clock.Update(Clock_Speed * deltaT);

        _engine.Update(deltaT);

        _hub.Update(mouseState, _clock.Day, _clock.FormattedTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw places
        foreach (var place in _engine.Places)
        {
            var textColor = place.IsDarkTheme ? Color.White : Color.Black;
            spriteBatch.Draw(_defaultTexture, new Rectangle(x: (int)place.Position.X, y: (int)place.Position.Y, width: (int)place.Size.X, height: (int)place.Size.Y), new Color(place.Color.X, place.Color.Y, place.Color.Z));

            var textPos = new Vector2(place.Position.X + (place.Size.X * 0.5f), place.Position.Y + (place.Size.Y * 0.5f));
            spriteBatch.DrawString(_font, place.Name, textPos, textColor);

            var pIndex = 0;
            foreach (var player in place.PlayersInside)
            {
                var playerNameTextPos = new Vector2(place.Position.X + 50, place.Position.Y + 50 + pIndex * 13);
                spriteBatch.DrawString(_font, player.Name, playerNameTextPos, textColor);
                pIndex++;
            }
        }

        // Draw players
        foreach (var player in _engine.Players)
        {
            if (!player.IsVisible)
            {
                continue;
            }

            spriteBatch.Draw(_defaultTexture, new Rectangle(x: (int)player.Position.X, y: (int)player.Position.Y, width: player.Size, height: player.Size), Color.Black);

            var textPos = new System.Numerics.Vector2((player.Position.X - (player.Size * 0.5f)), (player.Position.Y + player.Size));
            spriteBatch.DrawString(_font, player.Name, textPos, Color.Black);
        }

        // Draw notifications
        if (_engine.NotificationController.CurrentNotification is not null)
        {
            var notif = _engine.NotificationController.CurrentNotification;

            Vector2 position = new Vector2(10, 10);
            Color backgroundColor = Color.Black * 0.7f;
            Color textColor = Color.White;

            // Measure text size for background
            Vector2 textSize = _font.MeasureString(notif.Message);
            Rectangle backgroundRect = new Rectangle((int)position.X, (int)position.Y, (int)textSize.X + 20, (int)textSize.Y + 10);

            spriteBatch.Draw(_defaultTexture, backgroundRect, backgroundColor);
            spriteBatch.DrawString(_font, notif.Message, position + new Vector2(10, 5), textColor);
        }

        _hub.Draw(spriteBatch);
    }

    public void Dispose()
    {
        _hub.OnExpand -= OnExpand;
        _engine.Dispose();
    }
}
