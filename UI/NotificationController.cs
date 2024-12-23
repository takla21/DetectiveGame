using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Detective.UI;

public class NotificationController
{
    private Queue<Notification> _queue;

    public NotificationController()
    {
        _queue = new Queue<Notification>();
    }

    public Notification CurrentNotification { get; private set; }

    public void Enqueue(string message, float duration)
    {
        _queue.Enqueue(new Notification(message, duration));
    }

    public void GetCurrentNotification(float dt)
    {
        if (CurrentNotification != null)
        {
            CurrentNotification.TimeElapsed += dt;
            if (CurrentNotification.TimeElapsed >= CurrentNotification.Duration)
            {
                CurrentNotification = null;
            }
        }
        else if (_queue.Count > 0)
        {
            CurrentNotification = _queue.Dequeue();
        }
    }
}

public class Notification
{
    public Notification(string message, float duration)
    {
        Message = message;
        Duration = duration;
        TimeElapsed = 0;
    }

    public string Message { get; }

    public float Duration { get; }

    public float TimeElapsed { get; set; }
}