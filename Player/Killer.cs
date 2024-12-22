using System.Numerics;

namespace Detective;

public sealed class Killer : PlayerRoleBase
{
    public Killer(Vector2 position, LevelInformation placeInformation) : base(position, placeInformation)
    {
    }

    public override void Move(float deltaT)
    {
        // TODO add killer instinct.
        
        base.Move(deltaT);
    }
}
