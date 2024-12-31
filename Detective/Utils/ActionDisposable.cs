using System;

namespace Detective.Utils;

public class ActionDisposable : IDisposable
{
    private readonly Action _action;
    public ActionDisposable(Action action)
    {
        _action = action;
    }

    public void Dispose()
    {
        _action();
    }
}
