﻿namespace McswBot2.Event;

public class PlayerChangeEvent : EventBase
{
    internal PlayerChangeEvent(double diff)
    {
        PlayerDiff = diff;
    }

    public double PlayerDiff { get; }
}