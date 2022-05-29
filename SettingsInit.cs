using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clipboardhistory
{
    public class SettingsInit
    {
        public string ConfigPath { get; set; } = "Config.json";
        public static Settings Config { get; set; }

        public async Task InitializeAsync()
        {
            var json = string.Empty;

            if (!File.Exists(ConfigPath))
            {
                json = JsonConvert.SerializeObject(GenerateNewConfig(), Formatting.Indented);
                File.WriteAllText("Config.json", json, new UTF8Encoding(false));
                await Task.Delay(50);
            }

            json = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
            Config = JsonConvert.DeserializeObject<Settings>(json);
        }

        static Settings GenerateNewConfig() => new Settings
        {
            topmost = false,
            toast = false
        };
    }
}
