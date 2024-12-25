using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Detective.UI;

public class Hub : IDisposable
{
    private readonly Texture2D _defaultTexture;
    private readonly Vector2 _position;
    private readonly Vector2 _size;
    private readonly Color _color;

    private Button _expandBtn;

    public Hub(Texture2D defaultTexture, Vector2 position, Vector2 size, Color color, SpriteFont spriteFont)
    {
        _defaultTexture = defaultTexture;
        _position = position;
        _size = size;
        _color = color;

        LoadElement(spriteFont);
    }

    private void LoadElement(SpriteFont spriteFont)
    {
        _expandBtn = new Button(_defaultTexture, new Vector2(_position.X + _size.X - _size.Y, 0), new Vector2(_size.Y, _size.Y), _color, "X", spriteFont, new Color(1.0f, 1.0f, 1.0f, _color.A));

        _expandBtn.OnClick -= OnExpandButtonClicked;
        _expandBtn.OnClick += OnExpandButtonClicked;
    }

    private void OnExpandButtonClicked()
    {
        OnExpand?.Invoke();
    }

    public event Action OnExpand;

    public void Update(MouseState mouseState)
    {
        _expandBtn.Update(mouseState);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_defaultTexture, new Rectangle(x: (int)_position.X, y: (int)_position.Y, width: (int)_size.X, height: (int)_size.Y), _color);

        _expandBtn.Draw(spriteBatch);
    }

    public void Dispose()
    {
        _expandBtn.OnClick -= OnExpandButtonClicked;
    }
}
