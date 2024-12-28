using Detective.Navigation;
using Detective.Players;
using Detective.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Detective.Screens;

public class AccusationScreen : IModalScreen
{
    private const int ButtonPerColumn = 20;
    private const int MaxRows = 3;
    private const int ButtonWidth = 200;
    private const int ButtonHeight = 40;

    private const int ModalWidth = 800;
    private const int ModalHeight = 880;

    private readonly int _horizontalModalPadding;
    private readonly int _verticalModalPadding;
    private readonly int _horizontalButtonsPadding;
    private readonly int _verticalButtonsPadding;

    private readonly NavigationController _navigationController;
    private readonly IEnumerable<Player> _playerService;
    private readonly IDisposable _playerDeathSubscription;
    private readonly IDictionary<Player, Button> _playersButton;

    private Texture2D _defaultTexture;
    private SpriteFont _font;

    public AccusationScreen(NavigationController navigationController, IEnumerable<Player> players)
    {
        _navigationController = navigationController;
        _playerService = players;

        _playersButton = new Dictionary<Player, Button>();

        _horizontalModalPadding = (navigationController.ScreenWidth - ModalWidth) / 2;
        _verticalModalPadding = (navigationController.ScreenHeight - ModalHeight) / 2;

        _horizontalButtonsPadding = (ModalWidth - (ButtonWidth * MaxRows)) / 2;
        _verticalButtonsPadding = (ModalHeight - (ButtonHeight * ButtonPerColumn)) / 2;
    }

    public bool IsFullScreen { get; } = false;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("default");

        _defaultTexture = new Texture2D(graphicsDevice, 1, 1);
        _defaultTexture.SetData([Color.White]);

        var index = 0;

        var buttonSize = new Vector2(ButtonWidth, ButtonHeight);

        foreach (var player in _playerService)
        {
            var row = index % ButtonPerColumn;
            var column = index / ButtonPerColumn;

            var position = new Vector2(_horizontalModalPadding + _horizontalButtonsPadding + column * ButtonWidth, _verticalModalPadding + _verticalButtonsPadding + row * ButtonHeight);

            var button = new Button(_defaultTexture, position, buttonSize, Color.DarkGray, player.Name, _font, Color.Black, player);

            button.OnClick += OnPlayerButtonClicked;

            _playersButton.Add(player, button);

            index++;
        }
    }

    private void OnPlayerButtonClicked(object sender, ButtonClickEventArgs e)
    {
        var player = (Player)e.Value;

        if (player.IsKiller())
        {
            // TODO : display winning screen.
            Debug.WriteLine($"{player.Name} is the killer.");
        }
        else
        {
            // TODO : what happens when it fails
            Debug.WriteLine($"{player.Name} is NOT the killer.");
        }
    }

    public void Update(float deltaT, MouseState mouseState)
    {
        foreach (var button in _playersButton.Values)
        {
            button.Update(mouseState);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_defaultTexture, new Rectangle(x: _horizontalModalPadding, y: _verticalModalPadding, width: ModalWidth, height: ModalHeight), Color.DarkBlue);

        foreach (var button in _playersButton.Values)
        {
            button.Draw(spriteBatch);
        }
    }

    public void Dispose()
    {
        _playerDeathSubscription?.Dispose();

        foreach (var button in _playersButton.Values)
        {
            button.OnClick -= OnPlayerButtonClicked;
        }
    }
}
