using Detective.Level;
using Detective.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Detective.Players;

public sealed class WorkerSchedule : SleeperSchedule
{
    private readonly ILevelService _levelService;
    private readonly Place _workPlace;
    private readonly WorkSchedule[] _workSchedule;

    private bool _isWorking;
    private bool _isFirstDay;
    private WorkSchedule _currentSchedule;

    public WorkerSchedule(ILevelService levelService, IClock clock, Place workPlace, IEnumerable<WorkSchedule> workSchedule, IRandom random, ILevelPathFinding levelPathFinding) : base(levelService.Places.First(x => x.Information.Type == PlaceType.Houses), levelService.Information, clock, random, levelPathFinding)
    {
        _levelService = levelService;
        _workPlace = workPlace;
        _workSchedule = workSchedule.ToArray();
        _currentSchedule = _workSchedule[clock.Day];

        _isWorking = false;
        _isFirstDay = clock.Day <= 0;
    }

    protected override void OnHourChanged(int day, int hour, int minute)
    {
        _currentSchedule = _workSchedule[day];

        if (_isFirstDay)
        {
            _isFirstDay = _currentSchedule.IsNightShift ? day <= 0 : CheckIsTimeToSleep(hour);
        }

        if (_currentSchedule.IsDayOff)
        {
            _isWorking = false;

            base.OnHourChanged(day, hour, minute);
            return;
        }

        var previousState = _isWorking;

        if (_currentSchedule.IsNightShift)
        {
            _isWorking = hour >= _currentSchedule.StartHour || hour < _currentSchedule.EndHour;
        }
        else
        {
            _isWorking = hour >= _currentSchedule.StartHour && hour < _currentSchedule.EndHour;
        }

        if (_isWorking != previousState)
        {
            ShouldLeaveOnNextIteration = _isWorking ? ShouldLeaveOnNextIteration : true;

            if (_isWorking)
            {
                ClearMoves();
            }
        }

        base.OnHourChanged(day, hour, minute);
    }

    protected override bool CheckIsTimeToSleep(int currentHour)
    {
        var isTimeToSleep = _currentSchedule.IsNightShift ? currentHour >= TimeToSleep && currentHour <= TimeToWakeUp
            : base.CheckIsTimeToSleep(currentHour);
        return !_isFirstDay && isTimeToSleep;
    }

    protected override int SetTimeToSleep()
    {
        return _currentSchedule.IsNightShift ? Random.Next(_currentSchedule.EndHour + 1, 7) : base.SetTimeToSleep();
    }

    protected override int SetTimeToWakeUp()
    {
        return _currentSchedule.IsNightShift ? Random.Next(12, _currentSchedule.StartHour - 1) : base.SetTimeToWakeUp();
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

        if (_isWorking)
        {
            if (CurrentPlace == _workPlace.Information)
            {
                return Array.Empty<IMove>();
            }

            target = _workPlace.Information.EntrancePosition;
            selectedPlace = _workPlace.Information;
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
            // Add idle move after moving to a non work place point.
            if (!_isWorking)
            {
                moves.Add(new Idle((float)Random.NextDouble() * 10));
            }

            // Add invisiblity when entering into place.
            moves.Add(new ExecuteAction(() =>
            {
                EnterPlace(selectedPlace);
                CurrentPlace = selectedPlace;
                ShouldLeaveOnNextIteration = !_isWorking;
            }, shouldBeVisible: false));
        }
        else
        {
            // Add idle move after moving
            moves.Add(new Idle((float)Random.NextDouble() * 10));
        }

        moves.AddRange(
            LevelPathFinding.GenerateMoves(
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

    public override double CalculateSuspiciousProbability()
    {
        var probablity = 0d;

        if (_isWorking)
        {
            probablity += CurrentPlace != _workPlace.Information ? 0.9d : 0.5d;
        }

        if (IsTimeToSleep)
        {
            probablity += ShouldLeaveOnNextIteration ? 0.25 : 0;
        }

        return probablity > 1 ? 1 : probablity;
    }
}

public record WorkSchedule
{
    public WorkSchedule(bool isNightShift, int startHour, int endHour)
    {
        IsNightShift = isNightShift;
        StartHour = startHour;
        EndHour = endHour;
        IsDayOff = false;
    }

    public WorkSchedule(bool isNightShift)
    {
        IsNightShift = isNightShift;
        IsDayOff = true;
    }

    public bool IsNightShift { get; init; }

    public int StartHour { get; init; }

    public int EndHour { get; init; }

    public bool IsDayOff { get; init; }
}