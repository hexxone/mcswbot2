using System;
using mcswbot2.Bot.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mcswbot2.Bot
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
                var set = new JsonSerializerSettings()
                {
                    //TypeNameHandling = TypeNameHandling.Objects
                };

                // load config
                if (System.IO.File.Exists("config.json"))
                {
                    var json = System.IO.File.ReadAllText("config.json");
                    TgBot.Conf = JsonConvert.DeserializeObject<Config>(json, set);
                }
                else
                {
                    Save();
                    Program.WriteLine("\r\n\r\n\tWARNING: CONFIG JUST GOT CREATED. PLEASE MODIFY IT BEFORE STARTING AGAIN.\r\n");
                    Environment.Exit(0);
                }

                // load users objects if file exists
                if (System.IO.File.Exists("users.json"))
                {
                    var json = System.IO.File.ReadAllText("users.json");
                    TgBot.TgUsers.AddRange(JsonConvert.DeserializeObject<TgUser[]>(json, set));
                }

                // load group objects if file exists
                if (System.IO.File.Exists("groups.json"))
                {
                    var json = System.IO.File.ReadAllText("groups.json");
                    var des = JsonConvert.DeserializeObject<TgGroup[]>(json, set);

                    // Re-Register Servers with the factory
                    foreach (var g in des)
                        foreach (var pair in g.Servers)
                            g.LoadedServer(pair);

                    TgBot.TgGroups.AddRange(des);
                }
                // done
                Program.WriteLine($"Loaded data. [{TgBot.TgUsers.Count} Users, {TgBot.TgGroups.Count} Groups]");
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
                var set = new JsonSerializerSettings()
                {
                    //TypeNameHandling = TypeNameHandling.Objects
                };

                // write config
                var str = JsonConvert.SerializeObject(TgBot.Conf, Formatting.None, set);
                str = JToken.Parse(str).ToString(Formatting.Indented);
                System.IO.File.WriteAllText("config.json", str);
                // write user if any
                if (TgBot.TgUsers.Count > 0)
                {
                    str = JsonConvert.SerializeObject(TgBot.TgUsers, Formatting.None, set);
                    System.IO.File.WriteAllText("users.json", str);
                }
                // write groups if any
                if (TgBot.TgGroups.Count > 0)
                {
                    str = JsonConvert.SerializeObject(TgBot.TgGroups, Formatting.None, set);
                    System.IO.File.WriteAllText("groups.json", str);
                }
                // done
                Program.WriteLine($"Saved data. [{TgBot.TgUsers.Count} Users, {TgBot.TgGroups.Count} Groups]");
            }
            catch (Exception e)
            {
                Program.WriteLine("Error when saving data: " + e);
            }
        }
    }
}