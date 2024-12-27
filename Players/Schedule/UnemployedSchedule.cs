using Detective.Level;
using Detective.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Detective.Players;

public class UnemployedSchedule : IPlayerSchedule
{
    private readonly ILevelService _levelService;
    private readonly IDisposable _clockSubscription;

    private bool _shouldGoToHouse;
    private int _timeToSleep;
    private int _timeToWakeUp;

    private bool _shouldLeaveOnNextIteration;

    public PlaceInformation CurrentPlace { get; private set; }

    public UnemployedSchedule(ILevelService levelService, Clock clock)
    {
        _levelService = levelService;

        _clockSubscription = new ActionDisposable(() => clock.HourChanged -= OnHourChanged);
        clock.HourChanged += OnHourChanged;

        _shouldGoToHouse = false;
        _shouldLeaveOnNextIteration = false;

        _timeToSleep = Globals.Random.Next(21, 23);
        _timeToWakeUp = Globals.Random.Next(6, 12);
    }

    private void OnHourChanged(object sender, ClockTickEventArgs e)
    {
        var previousState = _shouldGoToHouse;
        _shouldGoToHouse = e.Hour >= _timeToSleep || e.Hour <= _timeToWakeUp;

        if (_shouldGoToHouse != previousState)
        {
            _timeToSleep = _shouldGoToHouse ? _timeToSleep : Globals.Random.Next(21, 27) % 24;
            _timeToWakeUp = _shouldGoToHouse ? Globals.Random.Next(6, 12) : _timeToWakeUp;
            _shouldLeaveOnNextIteration = _shouldGoToHouse ? _shouldLeaveOnNextIteration : true;
        }
    }

    public event PlaceUpdateHandler OnPlaceEntered;
    public event PlaceUpdateHandler OnPlaceExited;

    public IEnumerable<IMove> GenerateMoves(Vector2 currentPosition, PlaceInformation currentPlace)
    {
        var moves = new List<IMove>();

        var previousPlace = CurrentPlace;

        Vector2 target = default;
        var selectedPlace = default(PlaceInformation);

        if (_shouldGoToHouse)
        {
            if (CurrentPlace != null && CurrentPlace.Type == PlaceType.Houses)
            {
                return Array.Empty<IMove>();
            }

            var houses = _levelService.Places.First(x => x.Information.Type == PlaceType.Houses);
            target = houses.Information.EntrancePosition;
            selectedPlace = houses.Information;
        }
        else
        {
            do
            {
                var result = _levelService.PickPointOrPlace();
                target = result.SelectedPoint;
                selectedPlace = result.SelectedPlace;
            } while (target == currentPosition);
        }

        if (selectedPlace != null)
        {
            // Add idle move after moving
            moves.Add(new Idle((float)Globals.Random.NextDouble() * 10));

            // Add invisiblity when entering into place.
            moves.Add(new ExecuteAction(() =>
            {
                OnPlaceEntered?.Invoke(this, new PlaceUpdateArgs(selectedPlace));
                _shouldLeaveOnNextIteration = !_shouldGoToHouse;
            }, shouldBeVisible: false));
            CurrentPlace = selectedPlace;
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

        if (_shouldLeaveOnNextIteration)
        {
            moves.Add(new ExecuteAction(() =>
            {
                OnPlaceExited?.Invoke(this, new PlaceUpdateArgs(previousPlace));

                if (previousPlace == CurrentPlace)
                {
                    CurrentPlace = null;
                }
            }, shouldBeVisible: true));

            _shouldLeaveOnNextIteration = false;
        }

        return moves;
    }

    public void Dispose()
    {
        _clockSubscription.Dispose();
    }
}
