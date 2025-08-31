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
                    Logger.WriteLine(
                        "\r\n\r\n\tWARNING: CONFIG JUST GOT CREATED. PLEASE MODIFY IT BEFORE STARTING AGAIN.\r\n", Types.LogLevel.Error);
                    Environment.Exit(0);
                }


                // done
                Logger.WriteLine($"Loaded Config.");
            }
            catch (Exception e)
            {
                Logger.WriteLine($"Error when loading Config: {e}",  Types.LogLevel.Error);
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
                
                Logger.WriteLine("Saved Config.");
            }
            catch (Exception e)
            {
                Logger.WriteLine($"Error when saving config: {e}",  Types.LogLevel.Error);
            }
        }
    }
}