using System;
using System.Numerics;

namespace Detective;

public sealed class Innocent : PlayerRoleBase
{
    public Innocent(string playerId, Vector2 position, LevelInformation placeInformation) : base(playerId, position, placeInformation)
    {
    }

    protected override void GenerateFutureMoves(LevelInformation levelInformation)
    {
        Vector2 target = GenerateTarget(levelInformation);

        var moves = AStar.GenerateMoves(
            startPoint: Position,
            target: target,
            levelWidth: (int)levelInformation.Size.X,
            levelHeight: (int)levelInformation.Size.Y,
            invalidPoints: levelInformation.InvalidPositions
        );

        foreach (var move in moves)
        {
            FutureMoves.Push(move);
        }
    }

    private Vector2 GenerateTarget(LevelInformation levelInformation)
    {
        Vector2 target = default;
        var selectedPlace = default(PlaceInformation);

        do
        {
            var result = levelInformation.PickPointOrPlace();
            target = new Vector2(result.selectedPoint.X, result.selectedPoint.Y);
            selectedPlace = result.selectedPlace;
        } while (target == Position);

        if (selectedPlace != null)
        {
            // Add invisiblity when entering into place.
            FutureMoves.Push(new ExecuteAction(() => ExitPlace(selectedPlace), shouldBeVisible: true));

            // Add idle move after moving
            FutureMoves.Push(new Idle((float)new Random().NextDouble() * 10));

            // Add invisiblity when entering into place.
            FutureMoves.Push(new ExecuteAction(() => EnterPlace(selectedPlace), shouldBeVisible: false));
        }
        else
        {
            // Add idle move after moving
            FutureMoves.Push(new Idle((float)new Random().NextDouble() * 10));
        }

        return target;
    }
}
