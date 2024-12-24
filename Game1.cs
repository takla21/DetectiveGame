﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Detective;

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _defaultTexture;

        private GameEngine _engine;

        private SpriteFont _font;

    private const int ScreenWidth = 1920;
    private const int ScreenHeight = 1080;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

        _engine = new GameEngine(ScreenWidth, ScreenHeight);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            _engine.Init();
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

            _font = Content.Load<SpriteFont>("default");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            _engine.Update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Draw places
            foreach (var place in _engine.Places)
            {
                var textColor = place.IsDarkTheme ? Color.White : Color.Black;
            _spriteBatch.Draw(_defaultTexture, new Rectangle(x: (int)place.Position.X, y: (int)place.Position.Y, width: (int)place.Size.X, height: (int)place.Size.Y), new Color(place.Color.X, place.Color.Y, place.Color.Z));

            var textPos = new Vector2(place.Position.X + (place.Size.X * 0.5f), place.Position.Y + (place.Size.Y * 0.5f));
                _spriteBatch.DrawString(_font, place.Name, textPos, textColor);

                var pIndex = 0;
                foreach (var player in place.PlayersInside)
                {
                var playerNameTextPos = new Vector2(place.Position.X + 50, place.Position.Y + 50 + pIndex * 13);
                    _spriteBatch.DrawString(_font, player.Name, playerNameTextPos, textColor);
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

                _spriteBatch.Draw(_defaultTexture, new Rectangle(x: (int)player.Position.X, y: (int)player.Position.Y, width: player.Size, height: player.Size), Color.Black);

                var textPos = new System.Numerics.Vector2((player.Position.X - (player.Size * 0.5f)), (player.Position.Y + player.Size));
                _spriteBatch.DrawString(_font, player.Name, textPos, Color.Black);
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

                _spriteBatch.Draw(_defaultTexture, backgroundRect, backgroundColor);
                _spriteBatch.DrawString(_font, notif.Message, position + new Vector2(10, 5), textColor);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
