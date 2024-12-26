using Detective.Level;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Detective.Players;

public class UnemployedSchedule : IPlayerSchedule
{
    private readonly ILevelService _levelService;

    public UnemployedSchedule(ILevelService levelService)
    {
        _levelService = levelService;
    }

    public event PlaceUpdateHandler OnPlaceEntered;
    public event PlaceUpdateHandler OnPlaceExited;

    public IEnumerable<IMove> GenerateMoves(Vector2 currentPosition)
    {
        var moves = new List<IMove>();

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
            // Add invisiblity when entering into place.
            moves.Add(new ExecuteAction(() => OnPlaceExited?.Invoke(this, new PlaceUpdateArgs(selectedPlace)), shouldBeVisible: true));

            // Add idle move after moving
            moves.Add(new Idle((float)Globals.Random.NextDouble() * 10));

            // Add invisiblity when entering into place.
            moves.Add(new ExecuteAction(() => OnPlaceEntered?.Invoke(this, new PlaceUpdateArgs(selectedPlace)), shouldBeVisible: false));
        }
        else
        {
            // Add idle move after moving
            moves.Add(new Idle((float)Globals.Random.NextDouble() * 10));
        }

        moves.AddRange(
            AStar.GenerateMoves(
                startPoint: currentPosition,
                target: target,
                levelWidth: (int)_levelService.Information.LevelSize.X,
                levelHeight: (int)_levelService.Information.LevelSize.Y,
                invalidPoints: _levelService.Information.InvalidPositions
        ));

        return moves;
    }
}
