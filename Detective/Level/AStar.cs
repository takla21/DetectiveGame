using Detective.Players;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Detective;

public interface ILevelPathFinding
{
    public IEnumerable<IMove> GenerateMoves(Vector2 startPoint, Vector2 target, int levelWidth, int levelHeight, ISet<Vector2> invalidPoints);
}

public class AStarLevelPathFinding : ILevelPathFinding
{
    public IEnumerable<IMove> GenerateMoves(Vector2 startPoint, Vector2 target, int levelWidth, int levelHeight, ISet<Vector2> invalidPoints)
    {
        var queue = new PriorityQueue<Vector2, float>();
        queue.Enqueue(startPoint, 0);

        var cameFrom = new Dictionary<Vector2, Vector2>();
        var gScore = new Dictionary<Vector2, float>()
        {
            { startPoint, 0 }
        };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == target)
            {
                var moves = new List<IMove>();

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
                            moves.Add(new MoveTowardsPoint(direction: currentDirection, endPoint: endMove, speed: 100));
                            endMove = current;
                        }
                        currentDirection = direction;
                    }
                } while (current != startPoint);

                moves.Add(new MoveTowardsPoint(direction: currentDirection, endPoint: endMove, speed: 100));

                return moves;
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

                    if (neighbor.X < 0 || neighbor.X > levelWidth ||
                        neighbor.Y < 0 || neighbor.Y > levelHeight ||
                        invalidPoints.Contains(neighbor))
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

        return Array.Empty<IMove>();
    }
}
