using System;
using McswBot2.Static;

namespace McswBot2.Event;

public class PlayerPayLoad
{
    /// <summary>
    ///     Manual constructor
    /// </summary>
    internal PlayerPayLoad()
    {
        Online = true;
        LastSeen = DateTime.Now;
        PlayTime = TimeSpan.Zero;
    }

    public string? Name => Types.FixMcChat(RawName);
    public string? RawName { get; set; }
    public string? Id { get; set; }


    public bool Online { get; set; }

    public DateTime LastSeen { get; set; }

    public TimeSpan PlayTime { get; set; }
}