using System.Collections.Generic;
using System.Numerics;

namespace Detective.Players;

public interface IPlayerSchedule
{
    public IEnumerable<IMove> GenerateMoves(Vector2 currentPosition);

    public event PlaceUpdateHandler OnPlaceEntered;

    public event PlaceUpdateHandler OnPlaceExited;
}
