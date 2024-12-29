using Detective.Level;
using Detective.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Detective.Players
{
    public abstract class SleeperSchedule : IPlayerSchedule, IDisposable
    {
        private readonly IDisposable _clockSubscription;
        private readonly Place _home;
        private readonly LevelInformation _levelInformation;

        private int _timeToSleep;
        private int _timeToWakeUp;

        protected bool IsTimeToSleep { get; private set; }

        protected bool ShouldLeaveOnNextIteration { get; set; }

        public SleeperSchedule(Place home, LevelInformation levelInformation, Clock clock)
        {
            _home = home;
            _levelInformation = levelInformation;

            _clockSubscription = new ActionDisposable(() => clock.HourChanged -= InnterOnHourChanged);
            clock.HourChanged += InnterOnHourChanged;

            _timeToSleep = SetTimeToSleep();

            IsTimeToSleep = false;
        }

        public PlaceInformation CurrentPlace { get; protected set; }

        public event PlaceUpdateHandler OnPlaceEntered;
        public event PlaceUpdateHandler OnPlaceExited;

        private void InnterOnHourChanged(object sender, ClockTickEventArgs e)
        {
            OnHourChanged(e.Day, e.Hour, e.Minute);
        }

        protected virtual int SetTimeToSleep() => Globals.Random.Next(21, 24) % 24;

        protected virtual int SetTimeToWakeUp() => Globals.Random.Next(6, 12);

        protected virtual void OnHourChanged(int day, int hour, int minute)
        {
            var previousState = IsTimeToSleep;
            IsTimeToSleep = hour >= _timeToSleep || hour <= _timeToWakeUp;

            if (IsTimeToSleep != previousState)
            {
                _timeToSleep = IsTimeToSleep ? _timeToSleep : SetTimeToSleep();
                _timeToWakeUp = IsTimeToSleep ? SetTimeToWakeUp() : _timeToWakeUp;
                ShouldLeaveOnNextIteration = IsTimeToSleep ? ShouldLeaveOnNextIteration : true;
            }
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
                AStar.GenerateMoves(
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
}
