using System;
using System.Numerics;

namespace Detective.Players;

public class Player : IDisposable
{
    private PlayerRoleBase _role;

    public Player(string name, int size)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Size = size;
    }

    public void AssignRole(PlayerRoleBase role)
    {
        _role = role;

        _role.OnPlaceEntered -= InnerOnEnteredChanged;
        _role.OnPlaceEntered += InnerOnEnteredChanged;

        _role.OnPlaceExited -= InnerOnExitChanged;
        _role.OnPlaceExited += InnerOnExitChanged;
    }

    private void InnerOnEnteredChanged(object sender, PlaceUpdateArgs e)
    {
        OnPlaceEntered?.Invoke(this, e);
    }

    private void InnerOnExitChanged(object sender, PlaceUpdateArgs e)
    {
        OnPlaceExited?.Invoke(this, e);
    }
    public string Id { get; }

    public string Name { get; }

    public Vector2 Position => _role.Position;

    public bool IsVisible => _role.IsVisible;

    public event PlaceUpdateHandler OnPlaceEntered;
    public event PlaceUpdateHandler OnPlaceExited;
    public event PlayerDeathEventHandler OnDeath;

    public int Size { get; }

    public void Move(float deltaT)
    {
        _role.Move(deltaT);
    }

    public void Die(PlaceInformation placeInformation)
    {
        OnDeath?.Invoke(this, new PlayerDeathEventArgs(placeInformation));
    }

    public void Dispose()
    {
        _role.OnPlaceEntered -= InnerOnEnteredChanged;
        _role.OnPlaceExited -= InnerOnExitChanged;
    }
}

public delegate void PlayerDeathEventHandler(object sender,  PlayerDeathEventArgs e);

public record PlayerDeathEventArgs(PlaceInformation Place);