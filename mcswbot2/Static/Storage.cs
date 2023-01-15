using McswBot2.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace McswBot2.Static
{
    internal static class Storage
    {
        private const string PathConf = "config.json";
        private const string PathGrp = "data/groups.json";

        /// <summary>
        ///     Load & Json Decode the user & group settings
        /// </summary>
        internal static void Load(McswBot bot)
        {
            try
            {
                var set = new JsonSerializerSettings();

                // load config
                if (File.Exists(PathConf))
                {
                    var json = File.ReadAllText(PathConf);
                    bot.Conf = JsonConvert.DeserializeObject<Config>(json, set)
                                   ?? throw new Exception("Invalid Config! Please delete or fix it manually.");
                }
                else
                {
                    Save(bot);
                    Program.WriteLine(
                        "\r\n\r\n\tWARNING: CONFIG JUST GOT CREATED. PLEASE MODIFY IT BEFORE STARTING AGAIN.\r\n");
                    Environment.Exit(0);
                }


                // done
                Program.WriteLine($"Loaded Config.");
            }
            catch (Exception e)
            {
                Program.WriteLine("Error when loading Config: " + e);
            }
        }

        /// <summary>
        ///     Json encode & save the user & group settings
        /// </summary>
        internal static void Save(McswBot bot)
        {
            try
            {
                var set = new JsonSerializerSettings();

                // write config
                var str = JsonConvert.SerializeObject(bot.Conf, Formatting.None, set);
                str = JToken.Parse(str).ToString(Formatting.Indented);
                File.WriteAllText(PathConf, str);


                // done
                Program.WriteLine($"Saved Config.");
            }
            catch (Exception e)
            {
                Program.WriteLine("Error when saving data: " + e);
            }
        }
    }
}