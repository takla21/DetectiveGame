using Detective.Navigation;
using Detective.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Detective.Screens;

public sealed class MenuScreen : IScreen
{
    private readonly NavigationController _navigationController;
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    private Texture2D _defaultTexture;
    private SpriteFont _font;
    private Button _startBtn;

    private const int ButtonSize = 100;

    public MenuScreen(int screenWidth, int screenHeight, NavigationController navigationController)
    {
        _navigationController = navigationController;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("default");

        _defaultTexture = new Texture2D(graphicsDevice, 1, 1);
        _defaultTexture.SetData([Color.White]);

        _startBtn = new Button(_defaultTexture, new Vector2(_screenWidth * 0.5f - ButtonSize * 0.5f, _screenHeight * 0.5f - ButtonSize * 0.5f), new Vector2(ButtonSize), Color.LightGray, "Start", _font, Color.Black);

        _startBtn.OnClick -= OnStartClick;
        _startBtn.OnClick += OnStartClick;
    }

    private void OnStartClick(object sender, ButtonClickEventArgs e)
    {
        _navigationController.NavigateAndClear(new GameScreen(_screenWidth, _screenHeight, _navigationController));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _startBtn.Draw(spriteBatch);
    }

    public void Update(float deltaT, MouseState mouseState)
    {
        _startBtn.Update(mouseState);
    }

    public void Dispose()
    {
        _startBtn.OnClick -= OnStartClick;
    }
}
