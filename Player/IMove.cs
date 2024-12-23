using System;
using System.Numerics;

namespace Detective;

public record MoveResult(bool IsDone, Vector2 Position, bool IsVisible)
{

}

public interface IMove
{
    public MoveResult Execute(float deltaT, Vector2 currentPosition, bool isVisible);
}

public class MoveTowardsPoint : IMove
{
    private readonly Vector2 _speed;
    private readonly Vector2 _destination;

    private double previousDistance = double.MaxValue;

    public MoveTowardsPoint(Vector2 endPoint, Vector2 direction, float speed)
    {
        _destination = endPoint;

        _speed = direction * speed;
    }

    public MoveResult Execute(float deltaT, Vector2 currentPosition, bool isVisible)
    {
        var newPoint = currentPosition + _speed * deltaT;
        var currentDistance = Vector2.Distance(_destination, newPoint);
        var isDone = currentDistance > previousDistance;
        if (isDone)
        {
            newPoint = _destination;
        }
        previousDistance = currentDistance;
        return new MoveResult(isDone, newPoint, true);
    }
}

public class Idle : IMove
{
    private readonly float _timeToIdle;
    private float _timeElapsed;

    public Idle(float timeToIdle)
    {
        _timeToIdle = timeToIdle;
        _timeElapsed = 0;
    }

    public MoveResult Execute(float deltaT, Vector2 currentPosition, bool isVisible)
    {
        _timeElapsed += deltaT;
        return new MoveResult(_timeElapsed >= _timeToIdle, currentPosition, isVisible);
    }
}

public class ExecuteAction : IMove
{
    private readonly Action _act;
    private readonly bool? _shouldBeVisible;

    public ExecuteAction(Action act, bool? shouldBeVisible = null)
    {
        _act = act;
        _shouldBeVisible = shouldBeVisible;
    }

    public MoveResult Execute(float deltaT, Vector2 currentPosition, bool isVisible)
    {
        _act();
        return new MoveResult(true, currentPosition, _shouldBeVisible ?? isVisible);
    }
}
