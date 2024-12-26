using Detective.Players;
using System.Collections.Generic;
using System.Numerics;

namespace Detective;

public class Place
{
    private readonly List<Player> _players;

    public Place(PlaceInformation information, Vector3 color, bool isDarkTheme = false)
    {
        Information = information;
        Color = color;
        _players = new List<Player>();
        IsDarkTheme = isDarkTheme;
    }

    public PlaceInformation Information { get; init; }

    public string Name => Information.Name;

    public Vector2 Position => Information.Position;

    public Vector2 Size => Information.Size;

    public Vector3 Color { get; init; }

    public bool IsDarkTheme { get; }

    public IEnumerable<Player> PlayersInside => _players;

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }

    public void RemovePlayer(Player player)
    {
        _players.Remove(player);
    }
}
