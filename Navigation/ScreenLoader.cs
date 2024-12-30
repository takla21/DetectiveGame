using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Detective.Navigation;

public interface IScreenLoader
{
    public void Load(IScreen screen);
}

public class ScreenLoader : IScreenLoader
{
    private ContentManager _contentManager;
    private GraphicsDevice _graphicsDevice;

    public ScreenLoader(ContentManager contentManager, GraphicsDevice graphicsDevice)
    {
        _contentManager = contentManager;
        _graphicsDevice = graphicsDevice;
    }

    public void Load(IScreen screen)
    {
        screen.LoadContent(_contentManager, _graphicsDevice);
    }
}