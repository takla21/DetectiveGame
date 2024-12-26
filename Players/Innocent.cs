using Detective.Level;
using System;
using System.Numerics;

namespace Detective.Players;

public sealed class Innocent : PlayerRoleBase
{
    private readonly ILevelService _levelService;

    public Innocent(string playerId, Vector2 position, ILevelService levelService) : base(playerId, position)
    {
        _levelService = levelService;
    }

    protected override void GenerateFutureMoves()
    {
        Vector2 target = GenerateTarget();

        var moves = AStar.GenerateMoves(
            startPoint: Position,
            target: target,
            levelWidth: (int)_levelService.Information.LevelSize.X,
            levelHeight: (int)_levelService.Information.LevelSize.Y,
            invalidPoints: _levelService.Information.InvalidPositions
        );

        foreach (var move in moves)
        {
            FutureMoves.Push(move);
        }
    }

    private Vector2 GenerateTarget()
    {
        Vector2 target = default;
        var selectedPlace = default(PlaceInformation);

        do
        {
            var result = _levelService.PickPointOrPlace();
            target = result.SelectedPoint;
            selectedPlace = result.SelectedPlace;
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
