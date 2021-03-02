﻿using System;
using Telegram.Bot.Types;

namespace mcswbot2.Bot.Objects
{
    [Serializable]
    public class TgUser
    {
        /// <summary>
        ///     An object representing a Telegram user
        /// </summary>
        /// <param name="basis"></param>
        public TgUser(User basis)
        {
            Base = basis;
        }

        public User Base { get; set; }
        
    }
}