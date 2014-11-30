using System.IO;
using Newtonsoft.Json;

namespace SimpleMusicPlayer.Core.Player
{
    public static class PlayerSettingsExtensions
    {
        public const string SettingsFileName = "settings.json";

        public static PlayerSettings ReadSettings()
        {
            if (!File.Exists(SettingsFileName))
            {
                return PlayerSettings.GetEmptySettings();
            }
            var jsonString = File.ReadAllText(SettingsFileName);
            return JsonConvert.DeserializeObject<PlayerSettings>(jsonString);
        }

        public static void WriteSettings(this PlayerSettings settings)
        {
            if (settings == null)
            {
                return;
            }
            var settingsAsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsFileName, settingsAsJson);
        }
    }
}