using System;
using System.IO;

namespace Koasta.Shared.Configuration
{
    public static class SettingsProvider
    {
        public static ISettings LoadSettings()
        {
            var environment = System.Environment.GetEnvironmentVariable("PUBCRAWL_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environment))
            {
                environment = "default";
            }

            Console.WriteLine("Loading settings: " + environment);

            var workingDirectory = System.Environment.CurrentDirectory;
            var configPath = Path.Join(workingDirectory, "config", $"{environment}.json");

            if (!File.Exists(configPath))
            {
                throw new InvalidDataException($"Config file missing at path: {configPath}");
            }

            return Settings.Load(configPath);
        }
    }
}
