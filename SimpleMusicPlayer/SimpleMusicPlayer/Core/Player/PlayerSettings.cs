using System.Collections.Generic;
using System.IO;
using MahApps.Metro.Native;
using Newtonsoft.Json;
using ReactiveUI;
using SimpleMusicPlayer.Core.Interfaces;

namespace SimpleMusicPlayer.Core.Player
{
    public static class PlayerSettingsExtensions
    {
        public static void WriteSettings(this PlayerSettings settings)
        {
            if (settings == null)
            {
                return;
            }
            var settingsAsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(PlayerSettings.SettingsFileName, settingsAsJson);
        }
    }

    public class PlayerSettings
    {
        public const string SettingsFileName = "settings.json";

        public MainWindowSettings MainWindow { get; set; }
        public MedialibSettings Medialib { get; set; }
        public PlayerEngineSettings PlayerEngine { get; set; }

        public static PlayerSettings ReadSettings()
        {
            if (!File.Exists(SettingsFileName))
            {
                return GetEmptySettings();
            }
            var jsonString = File.ReadAllText(SettingsFileName);
            return JsonConvert.DeserializeObject<PlayerSettings>(jsonString);
        }

        private static PlayerSettings GetEmptySettings()
        {
            return new PlayerSettings {
                MainWindow = new MainWindowSettings(),
                Medialib = new MedialibSettings(),
                PlayerEngine = new PlayerEngineSettings()
            };
        }
    }

    public class MainWindowSettings : IWindowPlacementSetting
    {
        public WINDOWPLACEMENT? Placement { get; set; }
    }

    public class MedialibSettings : IWindowPlacementSetting
    {
        public WINDOWPLACEMENT? Placement { get; set; }
    }

    public class PlayerEngineSettings : ReactiveObject
    {
        public PlayerEngineSettings()
        {
            Volume = 100f;
            FadeIn = 5000f;
            FadeOut = 5000f;
            EqualizerSettings = new EqualizerSettings() { GainValues = new Dictionary<string, float>() };
        }

        public float Volume { get; set; }

        public float FadeIn { get; set; }
        public float FadeOut { get; set; }

        public bool Mute { get; set; }

        private bool shuffleMode;

        public bool ShuffleMode
        {
            get { return this.shuffleMode; }
            set { this.RaiseAndSetIfChanged(ref shuffleMode, value); }
        }

        private bool repeatMode;

        public bool RepeatMode
        {
            get { return this.repeatMode; }
            set { this.RaiseAndSetIfChanged(ref repeatMode, value); }
        }

        public EqualizerSettings EqualizerSettings { get; set; }
    }

    public class EqualizerSettings
    {
        public string Name { get; set; }
        public Dictionary<string, float> GainValues { get; set; }
        public bool IsEnabled { get; set; }
    }
}