using Detective.Level;
using System;
using System.Linq;
using System.Numerics;

namespace Detective.Players;

public sealed class Killer : PlayerRoleBase
{
    private readonly ILevelService _levelService;
    private readonly Random _random;

    private PlaceInformation _currentPlace;
    private double _timeInsideRemaining;

    private KillerState _currentState;

    public Killer(string playerId, Vector2 position, ILevelService levelService, IPlayerSchedule schedule) : base(playerId, position, schedule)
    {
        _levelService = levelService;
        _timeInsideRemaining = 0;
        _currentState = KillerState.Calm;
        _random = Globals.Random;
    }

    protected override void GenerateFutureMoves()
    {
        switch (_currentState)
        {
            default:
            case KillerState.Calm:
                var moves = Schedule.GenerateMoves(Position);

                foreach (var move in moves)
                {
                    FutureMoves.Push(move);
                }

                var choice = _random.Next(3);
                _currentState = choice == 1 ? KillerState.Hunting : KillerState.Calm;
                break;
            case KillerState.Hunting:
                var result = InnerGenerateMove(mustGoToPlace: true, isReadyToKill: true);

                if (result == ActionTaken.ExitPlace)
                {
                    _currentState = KillerState.Cooldown;
                }
                break;
            case KillerState.Cooldown:
                InnerGenerateMove();

                _currentState = KillerState.Calm;
                break;
        }
    }

    private ActionTaken InnerGenerateMove(bool mustGoToPlace = false, bool isReadyToKill = false)
    {
        if (IsVisible)
        {
            return GenerateMoveWhileBeingVisible(mustGoToPlace);
        }

        // If stayed more than time limit, force leave.
        if (_timeInsideRemaining <= 0)
        {
            Leave();

            return ActionTaken.ExitPlace;
        }

        if (isReadyToKill)
        {
            var playersInPlace = _levelService
                .PlacesOccupancy[_currentPlace]
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

    private ActionTaken GenerateMoveWhileBeingVisible(bool mustGoToPlace)
    {
        ActionTaken action;
        Vector2 target = default;
        var selectedPlace = default(PlaceInformation);
        do
        {
            if (mustGoToPlace)
            {
                selectedPlace = _levelService.PickPlace();
                target = selectedPlace.EntrancePosition;
            }
            else
            {
                var result = _levelService.PickPointOrPlace();
                target = result.SelectedPoint;
                selectedPlace = result.SelectedPlace;
            }
        } while (target == Position);

        if (selectedPlace is null)
        {
            action = ActionTaken.MoveToPoint;

            // Add idle move after moving
            FutureMoves.Push(new Idle((float)Globals.RandomFactory.Random.NextDouble() * 10));
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
            levelWidth: (int)_levelService.Information.LevelSize.X,
            levelHeight: (int)_levelService.Information.LevelSize.Y,
            invalidPoints: _levelService.Information.InvalidPositions
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

