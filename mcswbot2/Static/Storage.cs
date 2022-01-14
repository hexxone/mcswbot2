﻿using mcswbot2.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace mcswbot2.Static
{
    internal static class Storage
    {
        /// <summary>
        ///     Load & Json Decode the user & group settings
        /// </summary>
        internal static void Load()
        {
            try
            {
                var set = new JsonSerializerSettings();

                // load config
                if (File.Exists("config.json"))
                {
                    var json = File.ReadAllText("config.json");
                    MCSWBot.Conf = JsonConvert.DeserializeObject<Config>(json, set);
                }
                else
                {
                    Save();
                    Program.WriteLine(
                        "\r\n\r\n\tWARNING: CONFIG JUST GOT CREATED. PLEASE MODIFY IT BEFORE STARTING AGAIN.\r\n");
                    Environment.Exit(0);
                }

                // load users objects if file exists
                if (File.Exists("users.json"))
                {
                    var json = File.ReadAllText("users.json");
                    MCSWBot.TgUsers.AddRange(
                        JsonConvert.DeserializeObject<TgUser[]>(json, set) ?? Array.Empty<TgUser>());
                }

                // load group objects if file exists
                if (File.Exists("groups.json"))
                {
                    var json = File.ReadAllText("groups.json");
                    MCSWBot.TgGroups.AddRange(JsonConvert.DeserializeObject<TgGroup[]>(json, set) ??
                                              Array.Empty<TgGroup>());
                }

                // done
                Program.WriteLine($"Loaded data. [{MCSWBot.TgUsers.Count} Users, {MCSWBot.TgGroups.Count} Groups]");
            }
            catch (Exception e)
            {
                Program.WriteLine("Error when loading data: " + e);
            }
        }

        /// <summary>
        ///     Json encode & save the user & group settings
        /// </summary>
        internal static void Save()
        {
            try
            {
                var set = new JsonSerializerSettings();

                // write config
                var str = JsonConvert.SerializeObject(MCSWBot.Conf, Formatting.None, set);
                str = JToken.Parse(str).ToString(Formatting.Indented);
                File.WriteAllText("config.json", str);
                // write user if any
                if (MCSWBot.TgUsers.Count > 0)
                {
                    str = JsonConvert.SerializeObject(MCSWBot.TgUsers, Formatting.None, set);
                    File.WriteAllText("users.json", str);
                }

                // write groups if any
                if (MCSWBot.TgGroups.Count > 0)
                {
                    str = JsonConvert.SerializeObject(MCSWBot.TgGroups, Formatting.None, set);
                    File.WriteAllText("groups.json", str);
                }

                // done
                Program.WriteLine($"Saved data. [{MCSWBot.TgUsers.Count} Users, {MCSWBot.TgGroups.Count} Groups]");
            }
            catch (Exception e)
            {
                Program.WriteLine("Error when saving data: " + e);
            }
        }
    }
}