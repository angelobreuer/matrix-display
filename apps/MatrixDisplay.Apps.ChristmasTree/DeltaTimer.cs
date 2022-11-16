namespace MatrixDisplay.Apps.ChristmasTree;

internal ref struct DeltaTimer
{
    private readonly int _interval;
    private long _delta;
    private long _previousTickCount;

    public DeltaTimer(TimeSpan interval)
    {
        _interval = (int)Math.Round(interval.TotalMilliseconds);
        _previousTickCount = Environment.TickCount64;
    }

    public bool Tick()
    {
        var tickCount = Environment.TickCount64;
        _delta += tickCount - _previousTickCount;
        _previousTickCount = tickCount;

        if (_delta >= _interval)
        {
            _delta -= _interval;
            return true;
        }

        return false;
    }
}
