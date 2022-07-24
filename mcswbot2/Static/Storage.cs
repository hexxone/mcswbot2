using System;
using System.IO;
using McswBot2.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace McswBot2.Static;

internal static class Storage
{
    private const string PathConf = "config.json";
    private const string PathUsr = "data/users.json";
    private const string PathGrp = "data/groups.json";

    /// <summary>
    ///     Load & Json Decode the user & group settings
    /// </summary>
    internal static void Load()
    {
        try
        {
            var set = new JsonSerializerSettings();

            // load config
            if (File.Exists(PathConf))
            {
                var json = File.ReadAllText(PathConf);
                McswBot.Conf = JsonConvert.DeserializeObject<Config>(json, set)
                               ?? throw new Exception("Invalid Config! Please delete it manually.");
            }
            else
            {
                Save();
                Program.WriteLine(
                    "\r\n\r\n\tWARNING: CONFIG JUST GOT CREATED. PLEASE MODIFY IT BEFORE STARTING AGAIN.\r\n");
                Environment.Exit(0);
            }

            // make data dir
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");

            // load users objects if file exists
            if (File.Exists(PathUsr))
            {
                var json = File.ReadAllText(PathUsr);
                McswBot.TgUsers.AddRange(
                    JsonConvert.DeserializeObject<TgUser[]>(json, set) ?? Array.Empty<TgUser>());
            }

            // load group objects if file exists
            if (File.Exists(PathGrp))
            {
                var json = File.ReadAllText(PathGrp);
                McswBot.TgGroups.AddRange(JsonConvert.DeserializeObject<TgGroup[]>(json, set) ??
                                          Array.Empty<TgGroup>());
            }

            // done
            Program.WriteLine($"Loaded data. [{McswBot.TgUsers.Count} Users, {McswBot.TgGroups.Count} Groups]");
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
            var str = JsonConvert.SerializeObject(McswBot.Conf, Formatting.None, set);
            str = JToken.Parse(str).ToString(Formatting.Indented);
            File.WriteAllText(PathConf, str);
            // write user if any
            if (McswBot.TgUsers.Count > 0)
            {
                str = JsonConvert.SerializeObject(McswBot.TgUsers, Formatting.None, set);
                File.WriteAllText(PathUsr, str);
            }

            // write groups if any
            if (McswBot.TgGroups.Count > 0)
            {
                str = JsonConvert.SerializeObject(McswBot.TgGroups, Formatting.None, set);
                File.WriteAllText(PathGrp, str);
            }

            // done
            Program.WriteLine($"Saved data. [{McswBot.TgUsers.Count} Users, {McswBot.TgGroups.Count} Groups]");
        }
        catch (Exception e)
        {
            Program.WriteLine("Error when saving data: " + e);
        }
    }
}