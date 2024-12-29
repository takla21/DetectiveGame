﻿using Detective.Level;
using System.Collections.Generic;
using System.Numerics;

namespace Detective.Players;

public interface IPlayerSchedule
{
    public PlaceInformation CurrentPlace { get; }

    public IEnumerable<IMove> GenerateMoves(Vector2 currentPosition, PlaceInformation currentPlace);

    public event PlaceUpdateHandler OnPlaceEntered;

    public event PlaceUpdateHandler OnPlaceExited;
}