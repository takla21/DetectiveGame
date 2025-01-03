using Detective.Level;
using Detective.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Detective.Players;

public abstract class SleeperSchedule : IPlayerSchedule, IDisposable
{
    private readonly IDisposable _clockSubscription;
    private readonly Place _home;
    private readonly LevelInformation _levelInformation;

    protected IRandom Random { get; }

    protected ILevelPathFinding LevelPathFinding { get; }

    protected int TimeToSleep { get; private set; }

    protected int TimeToWakeUp { get; private set; }

    protected bool IsTimeToSleep { get; private set; }

    protected bool ShouldLeaveOnNextIteration { get; set; }

    public SleeperSchedule(Place home, LevelInformation levelInformation, IClock clock, IRandom random, ILevelPathFinding levelPathFinding)
    {
        _home = home;
        _levelInformation = levelInformation;

        Random = random;
        LevelPathFinding = levelPathFinding;

        _clockSubscription = new ActionDisposable(() => clock.HourChanged -= InnterOnHourChanged);
        clock.HourChanged += InnterOnHourChanged;

        IsTimeToSleep = false;

        // Set it to a dumb value to ensure that it has been initialized properly (child ctor called).
        TimeToSleep = -1;
        TimeToWakeUp = -1;
    }

    public PlaceInformation CurrentPlace { get; protected set; }

    public event PlaceUpdateHandler OnPlaceEntered;
    public event PlaceUpdateHandler OnPlaceExited;
    public event Action OnClearMoves;

    private void InnterOnHourChanged(object sender, ClockTickEventArgs e)
    {
        OnHourChanged(e.Day, e.Hour, e.Minute);
    }

    protected virtual int SetTimeToSleep() => Random.Next(21, 24);

    protected virtual int SetTimeToWakeUp() => Random.Next(6, 12);

    protected virtual void OnHourChanged(int day, int hour, int minute)
    {
        if (TimeToSleep == -1 || TimeToWakeUp == -1)
        {
            TimeToSleep = SetTimeToSleep();
            TimeToWakeUp = SetTimeToWakeUp();
        }

        var previousState = IsTimeToSleep;
        IsTimeToSleep = CheckIsTimeToSleep(currentHour: hour);

        if (IsTimeToSleep != previousState)
        {
            if (IsTimeToSleep)
            {
                ClearMoves();
            }

            if (!IsTimeToSleep)
            {
                TimeToSleep = SetTimeToSleep();
                TimeToWakeUp = SetTimeToWakeUp();
            }

            ShouldLeaveOnNextIteration = IsTimeToSleep ? ShouldLeaveOnNextIteration : true;
        }
    }

    protected void ClearMoves()
    {
        OnClearMoves?.Invoke();
    }

    protected virtual bool CheckIsTimeToSleep(int currentHour)
    {
        return currentHour >= TimeToSleep || currentHour <= TimeToWakeUp;
    }

    public virtual IEnumerable<IMove> GenerateMoves(Vector2 currentPosition, PlaceInformation currentPlace)
    {
        var moves = new List<IMove>();

        if (CurrentPlace != null && CurrentPlace.Type == PlaceType.Houses)
        {
            return Array.Empty<IMove>();
        }

        var target = _home.Information.EntrancePosition;
        var selectedPlace = _home.Information;

        // Add invisiblity when entering into place.
        moves.Add(new ExecuteAction(() =>
        {
            EnterPlace(selectedPlace);
            CurrentPlace = selectedPlace;
        }, shouldBeVisible: false));

        moves.AddRange(
            LevelPathFinding.GenerateMoves(
                startPoint: currentPosition,
                target: target,
                levelWidth: (int)_levelInformation.LevelSize.X,
                levelHeight: (int)_levelInformation.LevelSize.Y,
                invalidPoints: _levelInformation.InvalidPositions
        ));

        if (ShouldLeaveOnNextIteration)
        {
            moves.Add(new ExecuteAction(() =>
            {
                ExitPlace(CurrentPlace);
                CurrentPlace = null;
            }, shouldBeVisible: true));

            ShouldLeaveOnNextIteration = false;
        }

        return moves;
    }

    public abstract double CalculateSuspiciousProbability();

    protected void EnterPlace(PlaceInformation place)
    {
        OnPlaceEntered?.Invoke(this, new PlaceUpdateArgs(place));
    }

    protected void ExitPlace(PlaceInformation place)
    {
        OnPlaceExited?.Invoke(this, new PlaceUpdateArgs(place));
    }

    public void Dispose()
    {
        _clockSubscription.Dispose();
    }
}
