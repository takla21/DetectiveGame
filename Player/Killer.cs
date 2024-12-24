using System;
using System.Linq;
using System.Numerics;

namespace Detective;

public sealed class Killer : PlayerRoleBase
{
    private readonly LevelTracker _levelTracker;
    private readonly Random _random;

    private PlaceInformation _currentPlace;
    private double _timeInsideRemaining;

    private KillerState _currentState;

    public Killer(string playerId, Vector2 position, LevelInformation placeInformation, LevelTracker levelTracker) : base(playerId, position, placeInformation)
    {
        _levelTracker = levelTracker;
        _timeInsideRemaining = 0;
        _currentState = KillerState.Calm;
        _random = new Random();
    }

    protected override void GenerateFutureMoves(LevelInformation levelInformation)
    {
        switch (_currentState)
        {
            default:
            case KillerState.Calm:
                InnerGenerateMove(levelInformation);

                var choice = _random.Next(2);
                _currentState = choice == 2 ? KillerState.Hunting : KillerState.Calm;
                break;
            case KillerState.Hunting:
                var result = InnerGenerateMove(levelInformation, mustGoToPlace: true, isReadyToKill: true);

                if (result == ActionTaken.ExitPlace)
                {
                    _currentState = KillerState.Cooldown;
                }
                break;
            case KillerState.Cooldown:
                InnerGenerateMove(levelInformation);

                _currentState = KillerState.Calm;
                break;
        }
    }

    private ActionTaken InnerGenerateMove(LevelInformation levelInformation, bool mustGoToPlace = false, bool isReadyToKill = false)
    {
        if (IsVisible)
        {
            return GenerateMoveWhileBeingVisible(levelInformation, mustGoToPlace);
        }

        // If stayed more than time limit, force leave.
        if (_timeInsideRemaining <= 0)
        {
            Leave();

            return ActionTaken.ExitPlace;
        }

        if (isReadyToKill)
        {
            var playersInPlace = _levelTracker
                .GetPlayersFromPlace(_currentPlace)
                .Where(x => x.Id != PlayerId)
                .ToArray();

            if (playersInPlace.Length > 0)
            {
                var shouldExecute = _random.Next(2);

                // Kill
                if (shouldExecute == 1)
                {
                    var targetPlayer = playersInPlace[_random.Next(playersInPlace.Length)];
                    targetPlayer.Die(_currentPlace);

                    return ActionTaken.Murder;
                }
            }
        }

        var nonKillerChoice = _random.Next(2);

        if (nonKillerChoice == 1 && 10 - _timeInsideRemaining >= 0.1)
        {
            Leave();

            return ActionTaken.ExitPlace;
        }
        else
        {
            var timeToIdle = _random.NextDouble() * _timeInsideRemaining;
            FutureMoves.Push(new Idle((float)timeToIdle));
            _timeInsideRemaining -= timeToIdle;

            return ActionTaken.Idle;
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

    private ActionTaken GenerateMoveWhileBeingVisible(LevelInformation levelInformation, bool mustGoToPlace)
    {
        ActionTaken action;
        Vector2 target = default;
        var selectedPlace = default(PlaceInformation);
        do
        {
            if (mustGoToPlace)
            {
                selectedPlace = levelInformation.PickPlace();
                target = selectedPlace.EntrancePosition;
            }
            else
            {
                var result = levelInformation.PickPointOrPlace();
                target = new Vector2(result.selectedPoint.X, result.selectedPoint.Y);
                selectedPlace = result.selectedPlace;
            }
        } while (target == Position);

        if (selectedPlace is null)
        {
            action = ActionTaken.MoveToPoint;

            // Add idle move after moving
            FutureMoves.Push(new Idle((float)new Random().NextDouble() * 10));
        }
        else
        {
            action = ActionTaken.EnterPlace;

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
            levelWidth: (int)levelInformation.LevelSize.X,
            levelHeight: (int)levelInformation.LevelSize.Y,
            invalidPoints: levelInformation.InvalidPositions
        );

        foreach (var move in moves)
        {
            FutureMoves.Push(move);
        }

        return action;
    }

    private enum KillerState
    {
        Calm,
        Hunting,
        Cooldown
    }

    private enum ActionTaken
    {
        MoveToPoint,
        EnterPlace,
        ExitPlace,
        Murder,
        Idle
    }
}

