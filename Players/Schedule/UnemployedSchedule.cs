using Detective.Level;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Detective.Players;

public sealed class UnemployedSchedule : SleeperSchedule
{
    private readonly ILevelService _levelService;

    public UnemployedSchedule(ILevelService levelService, Clock clock, Random random) : base(levelService.Places.First(x => x.Information.Type == PlaceType.Houses), levelService.Information, clock, random)
    {
        _levelService = levelService;
    }

    protected override void OnHourChanged(int day, int hour, int minute)
    {
        base.OnHourChanged(day, hour, minute);
    }

    public override IEnumerable<IMove> GenerateMoves(Vector2 currentPosition, PlaceInformation currentPlace)
    {
        var moves = new List<IMove>();
        var previousPlace = currentPlace;

        if (IsTimeToSleep)
        {
            return base.GenerateMoves(currentPosition, currentPlace);
        }

        Vector2 target = default;
        var selectedPlace = default(PlaceInformation);
        do
        {
            var result = _levelService.PickPointOrPlace();
            target = result.SelectedPoint;
            selectedPlace = result.SelectedPlace;
        } while (target == currentPosition);

        if (selectedPlace != null)
        {
            // Add idle move after moving
            moves.Add(new Idle((float)Random.NextDouble() * 10));

            // Add invisiblity when entering into place.
            moves.Add(new ExecuteAction(() =>
            {
                EnterPlace(selectedPlace);
                CurrentPlace = selectedPlace;
                ShouldLeaveOnNextIteration = true;
            }, shouldBeVisible: false));
        }
        else
        {
            // Add idle move after moving
            moves.Add(new Idle((float)Random.NextDouble() * 10));
        }

        moves.AddRange(
            AStar.GenerateMoves(
                startPoint: currentPosition,
                target: target,
                levelWidth: (int)_levelService.Information.LevelSize.X,
                levelHeight: (int)_levelService.Information.LevelSize.Y,
                invalidPoints: _levelService.Information.InvalidPositions
        ));

        // Add base moves at the end.
        if (ShouldLeaveOnNextIteration)
        {
            moves.Add(new ExecuteAction(() =>
            {
                ExitPlace(previousPlace);
                CurrentPlace = null;
            }, shouldBeVisible: true));

            ShouldLeaveOnNextIteration = false;
        }

        return moves;
    }

    public override double CalculateSuspiciousProbability() => IsTimeToSleep && ShouldLeaveOnNextIteration ? 0.25 : 0;
}
