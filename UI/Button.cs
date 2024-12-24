using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Detective.UI;

public sealed class Button
{
    private readonly Texture2D _texture;
    private readonly Rectangle _bounds;
    private readonly Color _initialColor;

    private readonly string _text;
    private readonly SpriteFont _spriteFont;
    private readonly Color _textColor;
    private readonly Vector2 _textSize;

    private Color _color;
    private bool isHovered;

    public Vector2 Position { get; }
    public Vector2 Size { get; }
    public event Action OnClick;

    public Button(Texture2D texture, Vector2 position, Vector2 size, Color backgroundColor)
    {
        _texture = texture;
        _initialColor = backgroundColor;

        Position = position;
        Size = size;

        _bounds = new Rectangle(position.ToPoint(), size.ToPoint());
        _color = backgroundColor;
    }

    public Button(Texture2D texture, Vector2 position, Vector2 size, Color backgroundColor, string text, SpriteFont spriteFont, Color textColor)
    {
        _texture = texture;
        _initialColor = backgroundColor;

        _text = text;
        _spriteFont = spriteFont;
        _textColor = textColor;

        Position = position;
        Size = size;

        _bounds = new Rectangle(position.ToPoint(), size.ToPoint());
        _color = backgroundColor;

        _textSize = spriteFont.MeasureString(text);
    }

    public void Update(MouseState mouseState)
    {
        // Check if the mouse is over the button
        isHovered = _bounds.Contains(mouseState.Position);

        // Change color on hover
        _color = isHovered ? Color.Gray : _initialColor;

        // Detect click
        if (isHovered && mouseState.LeftButton == ButtonState.Pressed)
        {
            OnClick?.Invoke();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, _bounds, _color);

        if (_text != null)
        {
            var textPos = new Vector2((float)(_bounds.X + _bounds.Width * 0.5 - _textSize.X * 0.5), (float)(_bounds.Y + _bounds.Height * 0.5 - _textSize.Y * 0.5));
            spriteBatch.DrawString(_spriteFont, _text, textPos, _textColor);
        }
    }
}
