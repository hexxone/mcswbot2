﻿using System;
using Newtonsoft.Json;

namespace McswBot2.Minecraft;

[Serializable]
public class ServerInfoBasic
{
    public ServerInfoBasic()
    {
    }

    [JsonConstructor]
    public ServerInfoBasic(bool hadSuccess, DateTime requestDate, double requestTime, double currentPlayerCount,
        int qLevel)
    {
        HadSuccess = hadSuccess;
        RequestDate = requestDate;
        RequestTime = requestTime;
        CurrentPlayerCount = currentPlayerCount;
        QLevel = qLevel;
    }

    /// <summary>
    ///     Determines if the request was successful
    /// </summary>
    public bool HadSuccess { get; set; }

    /// <summary>
    ///     TimeStamp when the request was done
    /// </summary>
    public DateTime RequestDate { get; set; }

    /// <summary>
    ///     How long did the request take to complete in MS?
    /// </summary>
    public double RequestTime { get; set; }

    /// <summary>
    ///     Gets the server's current player count
    /// </summary>
    public double CurrentPlayerCount { get; set; }

    /// <summary>
    ///     Quantization Level
    /// </summary>
    public int QLevel { get; set; }
}