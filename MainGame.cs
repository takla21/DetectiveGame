using Detective.Navigation;
using Detective.Screens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Detective;

public class MainGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly IHostBuilder _hostBuilder;

    private SpriteBatch _spriteBatch;
    private Texture2D _defaultTexture;
    private SpriteFont _debugFont;

    private const int ScreenWidth = 1920;
    private const int ScreenHeight = 1080;

    private bool _escapeCooldown;

    private IServiceProvider _serviceProvider;
    private INavigationService _navigationService;
    private NavigationController _navigationController;

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _escapeCooldown = false;

        _hostBuilder = GenerateHostbuilder();
    }

    private IHostBuilder GenerateHostbuilder()
    {
        var builder = new HostBuilder();

        builder.ConfigureServices(services => services
            .AddSingleton(new ScreenConfiguration(ScreenWidth, ScreenHeight))
            .AddSingleton<INavigationService, NavigationService>()
        );

        return builder;
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

        _debugFont = Content.Load<SpriteFont>("default");

        _hostBuilder.ConfigureServices(services => services
            .AddSingleton<IScreenLoader>(s => new ScreenLoader(Content, GraphicsDevice))
            .AddSingleton<NavigationController>()
            .AddTransient<MainMenuScreen>()
        );

        var host = _hostBuilder.Build();
        _serviceProvider = host.Services;

        _navigationController = _serviceProvider.GetService<NavigationController>();
        _navigationController.Load(Content, GraphicsDevice);

        _navigationService = _serviceProvider.GetRequiredService<INavigationService>();
        _navigationService.NavigateTo<MainMenuScreen>();
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
        if (_navigationService.NavigationStack.Count > 1 || _navigationService.ModalStack.Count > 0)
        {
            _navigationService.NavigateBackOrDismissModal();
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
