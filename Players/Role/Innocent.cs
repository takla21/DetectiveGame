using System.Numerics;

namespace Detective.Players;

public sealed class Innocent : PlayerRoleBase
{
    public Innocent(string playerId, Vector2 position, IPlayerSchedule schedule) : base(playerId, position, schedule)
    {
    }

    protected override void GenerateFutureMoves()
    {
        var moves = Schedule.GenerateMoves(Position, Schedule.CurrentPlace);

        foreach (var move in moves)
        {
            FutureMoves.Push(move);
        }
    }
}
