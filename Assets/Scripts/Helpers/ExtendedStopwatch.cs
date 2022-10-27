using System;
using System.Diagnostics;

public class ExtendedStopwatch : Stopwatch
{
    public TimeSpan StartOffset { get; private set; }

    public void Rset(TimeSpan offset)
    {
        StartOffset = offset;
        base.Reset();
    }

    public void Restart(TimeSpan offset)
    {
        StartOffset = offset;
        base.Restart();
    }

    public new long ElapsedMilliseconds
    {
        get
        {
            return base.ElapsedMilliseconds + (long)StartOffset.TotalMilliseconds;
        }
    }

    public new long ElapsedTicks
    {
        get
        {
            return base.ElapsedTicks + StartOffset.Ticks;
        }
    }
}

