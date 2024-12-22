using System;
using System.Collections.Generic;
using System.Numerics;

namespace Detective;

public abstract class PlayerRoleBase
{
    protected readonly LevelInformation LevelInformation;

    protected Stack<IMove> FutureMoves;
    protected IMove CurrentMove;

    private int _id;
    private static int _idGen = 0;

    public PlayerRoleBase(Vector2 position, LevelInformation placeInformation)
    {
        _id = _idGen++;
        Position = position;
        FutureMoves = new Stack<IMove>();
        LevelInformation = placeInformation;
        IsVisible = true;
    }

    public Vector2 Position { get; private set; }

    public bool IsVisible { get; private set; }

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

            return;
        }

        // If empty, it's time to generate future moves
        Vector2 target = GenerateTarget();

        GenerateMovesFromPath(target);
    }

    private Vector2 GenerateTarget()
    {
        Vector2 target = default;
        var selectedPlace = default(PlaceInformation);

        var x = 0;

        do
        {
            var result = LevelInformation.PickPointOrPlace();
            target = new Vector2(result.selectedPoint.X, result.selectedPoint.Y);
            selectedPlace = result.selectedPlace;
            x++;
        } while (target == Position);

        if (selectedPlace != null)
        {
            // Add invisiblity when entering into place.
            FutureMoves.Push(new ExecuteAction(() => OnPlaceExited?.Invoke(this, new PlaceUpdateArgs(selectedPlace)), shouldBeVisible: true));

            // Add idle move after moving
            FutureMoves.Push(new Idle((float)new Random().NextDouble() * 10));

            // Add invisiblity when entering into place.
            FutureMoves.Push(new ExecuteAction(() => OnPlaceEntered?.Invoke(this, new PlaceUpdateArgs(selectedPlace)), shouldBeVisible: false));
        }
        else
        {
            // Add idle move after moving
            FutureMoves.Push(new Idle((float)new Random().NextDouble() * 10));
        }

        return target;
    }

    private void GenerateMovesFromPath(Vector2 target)
    {
        // A* algorithm
        var queue = new PriorityQueue<Vector2, float>();
        queue.Enqueue(Position, 0);

        var cameFrom = new Dictionary<Vector2, Vector2>();
        var gScore = new Dictionary<Vector2, float>()
        {
            { Position, 0 }
        };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == target)
            {
                var endMove = current;
                var currentDirection = Vector2.Zero;
                do
                {
                    current = cameFrom[current];
                    var direction = Vector2.Normalize(endMove - current);

                    if (currentDirection != direction)
                    {
                        if (currentDirection != Vector2.Zero)
                        {
                            FutureMoves.Push(new MoveTowardsPoint(direction: currentDirection, endPoint: endMove, speed: 100));
                        }
                        currentDirection = direction;
                        endMove = current;
                    }
                } while (current != Position);

                FutureMoves.Push(new MoveTowardsPoint(direction: currentDirection, endPoint: endMove, speed: 100));

                return;
            }

            for (var i = -1; i <= 1; i++)
            {
                for (var j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }

                    var neighbor = new Vector2(current.X + i, current.Y + j);

                    if (neighbor.X < 0 || neighbor.X > LevelInformation.Size.X || 
                        neighbor.Y < 0 || neighbor.Y > LevelInformation.Size.Y ||
                        LevelInformation.InvalidPositions.Contains(neighbor))
                    {
                        continue;
                    }

                    var tentativeScore = gScore[current] + 1;
                    if (!gScore.ContainsKey(neighbor) || tentativeScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeScore;
                        var priority = tentativeScore + Math.Abs(target.X - neighbor.X) + Math.Abs(target.Y - neighbor.Y);
                        queue.Enqueue(neighbor, priority);
                    }
                }
            }
        }

        // TODO handle error edge case
    }

    public event PlaceUpdateHandler OnPlaceEntered;

    public event PlaceUpdateHandler OnPlaceExited;
}


public delegate void PlaceUpdateHandler(object sender, PlaceUpdateArgs e);

public record PlaceUpdateArgs(PlaceInformation Place)
{
}
