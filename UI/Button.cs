using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Detective.UI;

public sealed class Button
{
    private readonly Texture2D _texture;
    private readonly Rectangle _bounds;
    private readonly Color _initialColor;
    private readonly object _value;

    private readonly string _text;
    private readonly SpriteFont _spriteFont;
    private readonly Color _textColor;
    private readonly Vector2 _textSize;

    private Color _color;
    private bool isHovered;

    // This required to ensure it doesn't check if button are clicked even before it's visible.
    private bool _hasBeenRendered;
    private bool _isReady;

    private bool _isCooldown;

    public Vector2 Position { get; }
    public Vector2 Size { get; }
    public event ButtonClickEventHandler OnClick;

    public Button(Texture2D texture, Vector2 position, Vector2 size, Color backgroundColor, object value = null)
    {
        _texture = texture;
        _initialColor = backgroundColor;

        Position = position;
        Size = size;

        _bounds = new Rectangle(position.ToPoint(), size.ToPoint());
        _color = backgroundColor;

        _value = value;

        SetInitialVariableStates();
    }

    public Button(Texture2D texture, Vector2 position, Vector2 size, Color backgroundColor, string text, SpriteFont spriteFont, Color textColor, object value = null)
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

        _value = value;
        SetInitialVariableStates();
    }

    private void SetInitialVariableStates()
    {
        _isCooldown = false;
        _hasBeenRendered = false;
        _isReady = false;
    }

    public void Update(MouseState mouseState)
    {
        // Check if the mouse is over the button
        isHovered = _bounds.Contains(mouseState.Position);

        if (!_hasBeenRendered)
        {
            _isReady = !(isHovered && mouseState.LeftButton == ButtonState.Pressed);
            return;
        }

        if (!_isReady)
        {
            _isReady = !isHovered || mouseState.LeftButton != ButtonState.Pressed;
        }

        // Change color on hover
        _color = isHovered ? Color.Gray : _initialColor;

        // Detect click
        if (_isReady &&!_isCooldown && isHovered && mouseState.LeftButton == ButtonState.Pressed)
        {
            OnClick?.Invoke(this, new ButtonClickEventArgs(_value));
            _isCooldown = true;
        }

        if (mouseState.LeftButton == ButtonState.Released)
        {
            _isCooldown = false;
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

        _hasBeenRendered = true;
    }
}

public delegate void ButtonClickEventHandler(object sender, ButtonClickEventArgs e);

public record ButtonClickEventArgs(object Value);
