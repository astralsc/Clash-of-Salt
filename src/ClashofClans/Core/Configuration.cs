using System;
using System.IO;
using ClashofClans.Core.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ClashofClans.Core
{
    public class Configuration
    {
        [JsonIgnore] public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Auto,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.None
        };

        [JsonProperty("mysql_database")] public string MySqlDatabase = "cosdb";
        [JsonProperty("mysql_password")] public string MySqlPassword = "";
        [JsonProperty("mysql_server")] public string MySqlServer = "127.0.0.1";
        [JsonProperty("mysql_user")] public string MySqlUserId = "root";

        [JsonProperty("server_port")] public int ServerPort = 9339;
        [JsonProperty("update_url")] public string UpdateUrl = "https://github.com/astralsc/Clash-of-Salt";
        [JsonProperty("encryption")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PacketCipherMode Encryption = PacketCipherMode.Pepper;

        /// <summary>
        ///     Loads the configuration
        /// </summary>
        public void Initialize()
        {
            if (File.Exists("config.json"))
                try
                {
                    var config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));

                    MySqlUserId = config.MySqlUserId;
                    MySqlServer = config.MySqlServer;
                    MySqlPassword = config.MySqlPassword;
                    MySqlDatabase = config.MySqlDatabase;

                    ServerPort = config.ServerPort;
                    UpdateUrl = config.UpdateUrl;
                    Encryption = config.Encryption;
                }
                catch (Exception)
                {
                    Console.WriteLine("Couldn't load configuration.");
                    Console.ReadKey(true);
                    Environment.Exit(0);
                }
            else
                try
                {
                    Save();

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("Server configuration has been created.\nNow update the config.json for your needs.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Couldn't create config file.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
        }

        public void Save()
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
