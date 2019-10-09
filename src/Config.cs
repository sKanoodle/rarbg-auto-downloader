using Newtonsoft.Json;
using System.IO;

namespace RarbgAutoDownloader
{
    class Config
    {
        [JsonIgnore]
        public bool NoConfigFound { get; }

        public string BasePath { get; }

        public string[] IgnoreTorrentsStartingWith { get; }

        public string[] Series { get; }

        public string[] Actors { get; }

        public TransmissionConfig TransmissionConfig { get; }


        [JsonIgnore]
        private static Config _Instance = null;

        [JsonIgnore]
        public static Config Instance => _Instance ??= Load();

        private static string ConfigFilePath => Path.GetFullPath("config.json");

        private Config() : this(default, default, default, default, default)
        {
            NoConfigFound = true;
        }

        [JsonConstructor]
        public Config(string basePath, string[] ignoreTorrentsStartingWith, string[] series, string[] actors, TransmissionConfig transmissionConfig)
        {
            BasePath = basePath;
            IgnoreTorrentsStartingWith = ignoreTorrentsStartingWith ?? new string[0];
            Series = series ?? new string[0];
            Actors = actors ?? new string[0];
            TransmissionConfig = transmissionConfig ?? new TransmissionConfig();
        }

        private static Config Load()
        {
            try
            {
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath));
            }
            catch (FileNotFoundException)
            {
                return new Config();
            }
        }

        public void Save()
        {
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    class TransmissionConfig
    {
        public string Address { get; }

        public string UserName { get; }

        public string Password { get; }

        public bool AutoStartTorrents { get; }

        public TransmissionConfig() { }

        [JsonConstructor]
        public TransmissionConfig(string address, string userName, string password, bool autoStartTorrents)
        {
            Address = address;
            UserName = userName;
            Password = password;
            AutoStartTorrents = autoStartTorrents;
        }
    }
}
