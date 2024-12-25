﻿namespace Detective;

public class Clock
{
    private float _seconds;

    public Clock()
    {
        Day = 1;
        Hour = 12;
        Minute = 0;

        _seconds = 0;
    }

    public int Day { get; private set; }

    public int Hour { get; private set; }

    public int Minute { get; private set; }

    public string FormattedTime => string.Format("{0:00}:{1:00}", Hour, Minute);

    public void Update(float deltaT)
    {
        _seconds += deltaT;

        if (_seconds < 60)
        {
            return;
        }

        Minute = ++Minute % 60;
        _seconds = 0;

        if (Minute != 0)
        {
            return;
        }

        Hour = ++Hour % 24;

        if (Hour == 0)
        {
            Day++;
        }
    }
}