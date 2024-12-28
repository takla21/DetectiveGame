using Detective.Navigation;
using Detective.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Detective;

public class MainGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly NavigationController _navigationController;

    private SpriteBatch _spriteBatch;
    private Texture2D _defaultTexture;
    private SpriteFont _debugFont;

    private const int ScreenWidth = 1920;
    private const int ScreenHeight = 1080;

    private bool _escapeCooldown;

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _escapeCooldown = false;

        _navigationController = new NavigationController(ScreenWidth, ScreenHeight);
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.ApplyChanges();

        _defaultTexture = new Texture2D(GraphicsDevice, 1, 1);
        _defaultTexture.SetData([Color.White]);

        _navigationController.Load(Content, GraphicsDevice);
        _navigationController.NavigateTo(new MenuScreen(_navigationController));

        _debugFont = Content.Load<SpriteFont>("default");
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (!_escapeCooldown && (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape)))
        {
            OnEscapeClicked();
            _escapeCooldown = true;
        }

        if (keyboardState.IsKeyUp(Keys.Escape))
        {
            _escapeCooldown = false;
        }

        var mouseState = Mouse.GetState();

        _navigationController.Update(gameTime.ElapsedGameTime.Milliseconds * 0.001f, mouseState);

        base.Update(gameTime);
    }

    private void OnEscapeClicked()
    {
        if (_navigationController.NavigationStack.Count > 1 || _navigationController.ModalStack.Count > 0)
        {
            _navigationController.NavigateBackOrDismissModal();
            return;
        }

        Exit();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        _navigationController.Draw(_spriteBatch);

        _spriteBatch.DrawString(_debugFont, Globals.RandomFactory.Seed.ToString(), new Vector2(0, 0), Color.Black);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
