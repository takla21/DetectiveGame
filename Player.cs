using System;
using System.Numerics;

namespace Detective;

public class Player : IDisposable
{
    private Inoncent _role;

    public Player(string name, int size)
    {
        Name = name;
        Size = size;
    }

    public void AssignRole(Inoncent role)
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

    public string Name { get; }

    public Vector2 Position => _role.Position;

    public bool IsVisible => _role.IsVisible;

    public event PlaceUpdateHandler OnPlaceEntered;
    public event PlaceUpdateHandler OnPlaceExited;

    public int Size { get; }

    public void Move(float deltaT)
    {
        _role.Move(deltaT);
    }

    public void Dispose()
    {
        _role.OnPlaceEntered -= InnerOnEnteredChanged;
        _role.OnPlaceExited -= InnerOnExitChanged;
    }
}
