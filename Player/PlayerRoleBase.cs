using System.Collections.Generic;
using System.Numerics;

namespace Detective;

public abstract class PlayerRoleBase
{
    private readonly LevelInformation _levelInformation;

    protected Stack<IMove> FutureMoves;
    protected IMove CurrentMove;

    private int _id;
    private static int _idGen = 0;

    public PlayerRoleBase(Vector2 position, LevelInformation placeInformation)
    {
        _id = _idGen++;
        Position = position;
        FutureMoves = new Stack<IMove>();
        _levelInformation = placeInformation;
        IsVisible = true;
    }

    public Vector2 Position { get; private set; }

    public bool IsVisible { get; private set; }

    protected abstract void GenerateFutureMoves(LevelInformation levelInformation);

    public virtual void Move(float deltaT)
    {
        // Check if stack of moves is not empty
        if (CurrentMove == null && FutureMoves.Count > 0)
        {
            CurrentMove = FutureMoves.Pop();
        }

        if (CurrentMove != null)
        {
            var result = CurrentMove.Execute(deltaT, Position, IsVisible);
            Position = result.Position;

            IsVisible = result.IsVisible;

            if (result.IsDone)
            {
                CurrentMove = null;
            }

            return;
        }

        // If empty, it's time to generate future moves
        GenerateFutureMoves(_levelInformation);
    }

    protected void EnterPlace(PlaceInformation place)
    {
        OnPlaceEntered?.Invoke(this, new PlaceUpdateArgs(place));
    }

    protected void ExitPlace(PlaceInformation place)
    {
        OnPlaceExited?.Invoke(this, new PlaceUpdateArgs(place));
    }

    public event PlaceUpdateHandler OnPlaceEntered;

    public event PlaceUpdateHandler OnPlaceExited;
}


public delegate void PlaceUpdateHandler(object sender, PlaceUpdateArgs e);

public record PlaceUpdateArgs(PlaceInformation Place)
{
}
