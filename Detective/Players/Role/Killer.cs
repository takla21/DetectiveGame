using Detective.Level;
using Detective.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Detective.Players;

public sealed class Killer : PlayerRoleBase
{
    private readonly ILevelService _levelService;
    private readonly IRandom _random;
    private readonly ILevelPathFinding _levelPathFinding;

    private PlaceInformation _currentPlace;

    private KillerState _currentState;
    private bool _didUseScheduleMoves;
    private float _timeUntilEndOfCooldown;

    public Killer(string playerId, Vector2 position, ILevelService levelService, IPlayerSchedule schedule, IRandom random, ILevelPathFinding levelPathFinding) : base(playerId, position, schedule)
    {
        _levelService = levelService;
        _random = random;
        _levelPathFinding = levelPathFinding;

        _currentState = KillerState.Calm;
        _didUseScheduleMoves = false;
    }

    protected override void GenerateFutureMoves()
    {
        if (_didUseScheduleMoves)
        {
            _currentPlace = Schedule.CurrentPlace;
        }

        // Get supposed moves according to schedule.
        var moves = Schedule.GenerateMoves(Position, _currentPlace);
        _didUseScheduleMoves = true;

        // Depending on the killer state, handle those moves.
        switch (_currentState)
        {
            default:
            case KillerState.Calm:

                var choice = _random.Next(3);
                _currentState = choice == 1 ? KillerState.Hunting : KillerState.Calm;
                break;
            case KillerState.Hunting:
                var suspicous = Schedule.CalculateSuspiciousProbability();
                suspicous += _currentPlace is null ? 0.1 : 0;

                var killerChoice = _random.NextDouble();
                if (killerChoice > 0.5 * (1 - suspicous))
                {
                    if (_currentPlace is null)
                    {
                        moves = GenerateMoveWhileBeingVisible();
                    }
                    else
                    {
                        var playersInPlace = _levelService
                            .PlacesOccupancy[_currentPlace]
                            .Where(x => x.Id != PlayerId)
                            .ToArray();

                        if (playersInPlace.Length > 0)
                        {
                            var targetPlayer = playersInPlace[_random.Next(playersInPlace.Length)];
                            targetPlayer.Die(_currentPlace);

                            _currentState = KillerState.Cooldown;
                            _timeUntilEndOfCooldown = TimeElapsed + (float)(_random.NextDouble()) * 30.0f;

                            moves = Array.Empty<IMove>();
                            _didUseScheduleMoves = false;
                        }
                    }
                }

                break;
            case KillerState.Cooldown:
                _currentState = TimeElapsed >= _timeUntilEndOfCooldown ? KillerState.Calm : KillerState.Cooldown;
                break;
        }

        foreach (var move in moves)
        {
            FutureMoves.Push(move);
        }
    }

    private IEnumerable<IMove> GenerateMoveWhileBeingVisible()
    {
        var moves = new List<IMove>();

        var selectedPlace = _levelService.PickPlace();
        var target = selectedPlace.EntrancePosition;
        _currentPlace = selectedPlace;

        moves.Add(new ExecuteAction(() =>
        {
            EnterPlace(selectedPlace);
        }, shouldBeVisible: false));

        moves.AddRange(_levelPathFinding.GenerateMoves(
            startPoint: Position,
            target: target,
            levelWidth: (int)_levelService.Information.LevelSize.X,
            levelHeight: (int)_levelService.Information.LevelSize.Y,
            invalidPoints: _levelService.Information.InvalidPositions
        ));

        _didUseScheduleMoves = false;

        return moves;
    }

    private enum KillerState
    {
        Calm,
        Hunting,
        Cooldown
    }
}

