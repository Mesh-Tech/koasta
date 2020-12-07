using Newtonsoft.Json;
using System.IO;

namespace Koasta.Shared.Configuration
{
    internal class Settings : ISettings
    {
        private string configPath;
        private FileSystemWatcher watcher;

        [JsonProperty("connection")]
        public Data Connection { get; private set; }
        [JsonProperty("meta")]
        public Meta Meta { get; private set; }
        [JsonProperty("auth")]
        public Auth Auth { get; private set; }

        internal void StartWatching(string configPath)
        {
            this.configPath = configPath;
            watcher = new FileSystemWatcher(Path.GetDirectoryName(configPath), Path.GetFileName(configPath));
            watcher.Changed += (sender, eventArgs) => Reload();
            watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        internal void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                watcher.Dispose();
            }
        }

        internal static Settings Load(string configPath)
        {
            Settings settings;

            using (StreamReader file = File.OpenText(configPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                settings = (Settings)serializer.Deserialize(file, typeof(Settings));
            }

            settings.StartWatching(configPath);
            return settings;
        }

        private void Reload()
        {
            Settings settings;

            using (StreamReader file = File.OpenText(configPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                settings = (Settings)serializer.Deserialize(file, typeof(Settings));
            }

            Connection = settings.Connection;
            Meta = settings.Meta;
            Auth = settings.Auth;
        }
    }
}
