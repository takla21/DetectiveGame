﻿using Detective.Level;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Detective.Players;

public abstract class PlayerRoleBase : IDisposable
{
    protected readonly string PlayerId;
    protected readonly IPlayerSchedule Schedule;

    protected Stack<IMove> FutureMoves;
    protected IMove CurrentMove;

    protected float TimeElapsed { get; private set; }

    public PlayerRoleBase(string playerId, Vector2 position, IPlayerSchedule schedule)
    {
        PlayerId = playerId;
        Position = position;
        Schedule = schedule;

        FutureMoves = new Stack<IMove>();
        IsVisible = true;

        schedule.OnPlaceEntered += InnerOnPlaceEntered;
        schedule.OnPlaceExited += InnerOnPlaceExited;
        schedule.OnClearMoves += OnClearMoves;
    }

    protected virtual void OnClearMoves()
    {
        FutureMoves.Clear();
        CurrentMove = null;

        // Floor position to avoid float position which breaks A* algorithm
        Position = new Vector2((int)Position.X, (int)Position.Y);
    }

    public Vector2 Position { get; private set; }

    public bool IsVisible { get; private set; }

    public event PlaceUpdateHandler OnPlaceEntered;

    public event PlaceUpdateHandler OnPlaceExited;

    protected abstract void GenerateFutureMoves();

    public virtual void Move(float deltaT)
    {
        // Check if stack of moves is not empty
        if (CurrentMove == null && FutureMoves.Count > 0)
        {
            CurrentMove = FutureMoves.Pop();
        }

        if (CurrentMove != null)
        {
            var result = CurrentMove.Execute(deltaT, Position, IsVisible);
            Position = result.Position;

            IsVisible = result.IsVisible;

            if (result.IsDone)
            {
                CurrentMove = null;
            }

            TimeElapsed += deltaT;

            return;
        }

        // If empty, it's time to generate future moves
        GenerateFutureMoves();

        TimeElapsed += deltaT;
    }

    private void InnerOnPlaceEntered(object sender, PlaceUpdateArgs e)
    {
        EnterPlace(e.Place);
    }

    private void InnerOnPlaceExited(object sender, PlaceUpdateArgs e)
    {
        ExitPlace(e.Place);
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
        Schedule.OnPlaceEntered -= InnerOnPlaceEntered;
        Schedule.OnPlaceExited -= InnerOnPlaceExited;
        Schedule.OnClearMoves -= OnClearMoves;

        if (Schedule is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}


public delegate void PlaceUpdateHandler(object sender, PlaceUpdateArgs e);

public record PlaceUpdateArgs(PlaceInformation Place)
{
}
