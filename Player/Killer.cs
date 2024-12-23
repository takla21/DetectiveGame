using System;
using System.Linq;
using System.Numerics;

namespace Detective;

public sealed class Killer : PlayerRoleBase
{
    private readonly LevelTracker _levelTracker;

    private PlaceInformation _currentPlace;
    private double _timeInsideRemaining;

    public Killer(string playerId, Vector2 position, LevelInformation placeInformation, LevelTracker levelTracker) : base(playerId, position, placeInformation)
    {
        _levelTracker = levelTracker;
        _timeInsideRemaining = 0;
    }

    protected override void GenerateFutureMoves(LevelInformation levelInformation)
    {
        if (IsVisible)
        {
            Vector2 target = default;
            var selectedPlace = default(PlaceInformation);
            do
            {
                var result = levelInformation.PickPointOrPlace();
                target = new Vector2(result.selectedPoint.X, result.selectedPoint.Y);
                selectedPlace = result.selectedPlace;
            } while (target == Position);

            if (selectedPlace is null)
            {
                // Add idle move after moving
                FutureMoves.Push(new Idle((float)new Random().NextDouble() * 10));
            }
            else
            {
                FutureMoves.Push(new ExecuteAction(() =>
                {
                    EnterPlace(selectedPlace);
                    _currentPlace = selectedPlace;
                    _timeInsideRemaining = 10;
                }, shouldBeVisible: false));
            }

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
        else
        {
            // If stayed more than time limit, force leave.
            if (_timeInsideRemaining <= 0)
            {
                Leave();
            }

            var rand = new Random();

            var choice = rand.Next(3);

            switch (choice)
            {
                default:
                case 0:
                    Leave();
                    break;
                case 1:
                    var timeToIdle = rand.NextDouble() * _timeInsideRemaining;
                    FutureMoves.Push(new Idle((float)timeToIdle));
                    _timeInsideRemaining -= timeToIdle;
                    break;
                case 2:
                    var playersInPlace = _levelTracker
                        .GetPlayersFromPlace(_currentPlace)
                        .Where(x => x.Id != PlayerId)
                        .ToArray();

                    if (playersInPlace.Length > 0)
                    {
                        var targetPlayer = playersInPlace[rand.Next(playersInPlace.Length)];
                        targetPlayer.Die(_currentPlace);
                    }

                    break;
            }

            void Leave()
            {
                FutureMoves.Push(new ExecuteAction(() =>
                {
                    ExitPlace(_currentPlace);
                    _timeInsideRemaining = 0;
                    _currentPlace = null;
                }, shouldBeVisible: true));
            }
        }
    }
}
