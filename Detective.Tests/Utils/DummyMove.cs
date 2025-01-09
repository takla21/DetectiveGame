using Detective.Players;
using System.Numerics;

namespace Detective.Tests.Utils;

internal class DummyMove : IMove
{
    public MoveResult Execute(float deltaT, Vector2 currentPosition, bool isVisible)
    {
        return new MoveResult(true, currentPosition, true);
    }
}