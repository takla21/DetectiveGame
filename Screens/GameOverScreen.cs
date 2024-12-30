using Detective.Navigation;
using Detective.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Detective.Screens;

public class GameOverScreen : IScreen
{
    private const int ButtonSize = 100;

    private readonly INavigationService _navigationController;
    private readonly ScreenConfiguration _screenConfiguration;
    private readonly string _screenTitle;

    private Texture2D _defaultTexture;
    private SpriteFont _font;
    private Button _backMenuBtn;
    private Vector2 _textSize;

    public GameOverScreen(INavigationService navigationController, ScreenConfiguration screenConfiguration, bool hasUserWon)
    {
        _navigationController = navigationController;
        _screenConfiguration = screenConfiguration;

        _screenTitle = hasUserWon ? "Congrats! You've found the killer!" : "Oh no! The killer got away!";
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("default");

        _defaultTexture = new Texture2D(graphicsDevice, 1, 1);
        _defaultTexture.SetData([Color.White]);

        _backMenuBtn = new Button(_defaultTexture, new Vector2(_screenConfiguration.Width * 0.5f - ButtonSize * 0.5f, _screenConfiguration.Height * 0.5f - ButtonSize * 0.5f), new Vector2(ButtonSize), Color.LightGray, "Quit", _font, Color.Black);

        _backMenuBtn.OnClick -= OnBackMenuClick;
        _backMenuBtn.OnClick += OnBackMenuClick;

        _textSize = _font.MeasureString(_screenTitle);
    }

    private void OnBackMenuClick(object sender, ButtonClickEventArgs e)
    {
        _navigationController.NavigateAndClear<MainMenuScreen>();
    }
    public void Update(float deltaT, MouseState mouseState)
    {
        _backMenuBtn.Update(mouseState);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(_font, _screenTitle, new Vector2(_screenConfiguration.Width * 0.5f - _textSize.X * 0.5f, 100.0f), Color.Black);

        _backMenuBtn.Draw(spriteBatch);
    }

    public void Dispose()
    {
        _backMenuBtn.OnClick -= OnBackMenuClick;
    }
}
