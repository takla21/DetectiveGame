using System.Numerics;

namespace Detective;

public sealed class Killer : PlayerRoleBase
{
    public Killer(Vector2 position, LevelInformation placeInformation) : base(position, placeInformation)
    {
    }

    protected override void GenerateFutureMoves(LevelInformation levelInformation)
    {
        throw new System.NotImplementedException();
    }
}
