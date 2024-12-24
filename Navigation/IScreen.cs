using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Detective.Navigation;

public interface IScreen : IDisposable
{
    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice);

    public void Update(float deltaT, MouseState mouseState);

    public void Draw(SpriteBatch spriteBatch);
}
